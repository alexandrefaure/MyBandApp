using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using System.Collections.ObjectModel;

namespace MyBandApp.Views;

public partial class SetlistEditPage : ContentPage
{
    private readonly Setlist _currentSetlist;
    private readonly SetlistService _setlistService;
    private SelectableSongModel? _draggedItem;
    private string _searchText = string.Empty;

    public string SetlistName { get; set; } = string.Empty;
    public DateTime SetlistDate { get; set; } = DateTime.Today;

    public ObservableCollection<SelectableSongModel> AllSongs { get; set; } = new();

    private ObservableCollection<SelectableSongModel> _selectedSongs = new();
    public ObservableCollection<SelectableSongModel> SelectedSongs
    {
        get => _selectedSongs;
        set { _selectedSongs = value; OnPropertyChanged(); }
    }

    private ObservableCollection<SelectableSongModel> _filteredCatalogueSongs = new();
    public ObservableCollection<SelectableSongModel> FilteredCatalogueSongs
    {
        get => _filteredCatalogueSongs;
        set { _filteredCatalogueSongs = value; OnPropertyChanged(); }
    }

    private string _totalDurationText = "00:00";
    public string TotalDurationText
    {
        get => _totalDurationText;
        set
        {
            if (_totalDurationText == value) return;
            _totalDurationText = value;
            OnPropertyChanged();
        }
    }

    private void RecalculateTotalDuration()
    {
        var totalSeconds = SelectedSongs
            .Where(s => s.IsSelected) // garde uniquement les morceaux cochés/sélectionnés si SelectedSongs contient tous les morceaux
            .Sum(s => s.Song.DurationSeconds);

        var total = TimeSpan.FromSeconds(totalSeconds);

        TotalDurationText = total.TotalHours >= 1
            ? $"{(int)total.TotalHours:00}h{total.Minutes:00}"
            : $"{(int)total.TotalMinutes:00}:{total.Seconds:00}";
    }

    public SetlistEditPage(SetlistWithSongs? setlistWithSongs, List<Song> allSongs, SetlistService setlistService)
    {
        InitializeComponent();
        _setlistService = setlistService;

        _currentSetlist = setlistWithSongs?.Setlist ?? new Setlist { Date = DateTime.Today };
        SetlistName = _currentSetlist.Name;
        SetlistDate = _currentSetlist.Date;

        var selectedSongIds = setlistWithSongs?.Songs?.Select(s => s.Id).ToList() ?? new List<int>();

        InitializeLists(allSongs, selectedSongIds);
        RecalculateTotalDuration();
        BindingContext = this;
    }

    private void InitializeLists(List<Song> allSongs, List<int> selectedSongIds)
    {
        AllSongs.Clear();

        if (selectedSongIds.Any())
        {
            foreach (var songId in selectedSongIds)
            {
                var original = allSongs.FirstOrDefault(s => s.Id == songId);
                if (original != null)
                {
                    AllSongs.Add(new SelectableSongModel { Song = original, IsSelected = true });
                }
            }
            foreach (var song in allSongs.Where(s => !selectedSongIds.Contains(s.Id)))
            {
                AllSongs.Add(new SelectableSongModel { Song = song, IsSelected = false });
            }
        }
        else
        {
            foreach (var song in allSongs)
            {
                AllSongs.Add(new SelectableSongModel { Song = song, IsSelected = false });
            }
        }

        UpdateSelectedSongs();
        FilterCatalogue(string.Empty);
    }

    private void UpdateSelectedSongs()
    {
        var selectedList = AllSongs.Where(x => x.IsSelected).ToList();
        for (int i = 0; i < selectedList.Count; i++)
        {
            selectedList[i].DisplayIndex = i + 1;
        }
        SelectedSongs = new ObservableCollection<SelectableSongModel>(selectedList);
    }

    private void FilterCatalogue(string query)
    {
        var filtered = string.IsNullOrWhiteSpace(query)
            ? AllSongs.ToList()
            : AllSongs.Where(x => x.Song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                  x.Song.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        FilteredCatalogueSongs = new ObservableCollection<SelectableSongModel>(filtered);
    }

    private void OnTab1Clicked(object sender, EventArgs e)
    {
        View1Layout.IsVisible = true;
        View2Layout.IsVisible = false;
        Tab1Button.BackgroundColor = Color.FromArgb("#F59E0B");
        Tab1Button.TextColor = Color.FromArgb("#0F1115");
        Tab2Button.BackgroundColor = Color.FromArgb("#1A1D24");
        Tab2Button.TextColor = Color.FromArgb("#94A3B8");
    }

    private void OnTab2Clicked(object sender, EventArgs e)
    {
        View1Layout.IsVisible = false;
        View2Layout.IsVisible = true;
        Tab2Button.BackgroundColor = Color.FromArgb("#F59E0B");
        Tab2Button.TextColor = Color.FromArgb("#0F1115");
        Tab1Button.BackgroundColor = Color.FromArgb("#1A1D24");
        Tab1Button.TextColor = Color.FromArgb("#94A3B8");
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        FilterCatalogue(_searchText);
    }

    private void OnSongCardTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is SelectableSongModel model)
        {
            model.IsSelected = !model.IsSelected;
            UpdateSelectedSongs();
            RecalculateTotalDuration();
        }
    }

    private void OnCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        UpdateSelectedSongs();
        RecalculateTotalDuration();
    }

    private void OnRemoveSongClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SelectableSongModel model)
        {
            model.IsSelected = false;
            UpdateSelectedSongs();
            RecalculateTotalDuration();
        }
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is SelectableSongModel model)
        {
            _draggedItem = model;

            if (e.Data.Properties.ContainsKey("DraggedItem"))
            {
                e.Data.Properties["DraggedItem"] = model;
            }
            else
            {
                e.Data.Properties.Add("DraggedItem", model);
            }
        }
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        if (_draggedItem == null) return;

        SelectableSongModel? targetItem = null;

        if (sender is BindableObject bindable && bindable.BindingContext is SelectableSongModel target)
        {
            targetItem = target;
        }

        if (targetItem != null && _draggedItem != targetItem)
        {
            int oldIndex = SelectedSongs.IndexOf(_draggedItem);
            int newIndex = SelectedSongs.IndexOf(targetItem);

            if (oldIndex >= 0 && newIndex >= 0)
            {
                SelectedSongs.Move(oldIndex, newIndex);

                int globalOldIndex = AllSongs.IndexOf(_draggedItem);
                int globalNewIndex = AllSongs.IndexOf(targetItem);
                if (globalOldIndex >= 0 && globalNewIndex >= 0)
                {
                    AllSongs.Move(globalOldIndex, globalNewIndex);
                }

                for (int i = 0; i < SelectedSongs.Count; i++)
                {
                    SelectedSongs[i].DisplayIndex = i + 1;
                }
            }
        }

        _draggedItem = null;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SetlistName))
        {
            await DisplayAlert("Erreur", "Donne un nom à la setlist.", "OK");
            return;
        }

        try
        {
            _currentSetlist.Name = SetlistName.Trim();
            _currentSetlist.Date = SetlistDate.ToUniversalTime();

            var selectedIds = SelectedSongs.Select(s => s.Song.Id).ToList();

            await _setlistService.SaveSetlistAsync(_currentSetlist, selectedIds);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}