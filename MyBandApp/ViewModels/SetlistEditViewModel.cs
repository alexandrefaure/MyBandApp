using MyBandApp.Extensions;
using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MyBandApp.ViewModels;

public partial class SetlistEditViewModel : ReactiveObject
{
    private readonly SetlistService _setlistService;
    private readonly SongService _songService;
    private readonly IUserDialogService _dialogs;
    private readonly IAppNavigationService _navigation;
    private Setlist _currentSetlist = new();
    private SelectableSongModel? _draggedSong;
    private string _searchText = string.Empty;

    public ObservableCollection<SelectableSongModel> AllSongs { get; } = new();
    public ObservableCollection<SelectableSongModel> SelectedSongs { get; } = new();
    public ObservableCollection<SelectableSongModel> FilteredCatalogueSongs { get; } = new();
    public ICommand ShowSelectedSongsTabCommand { get; }
    public ICommand ShowCatalogueTabCommand { get; }
    public ICommand ToggleSongSelectionCommand { get; }
    public ICommand RemoveSongCommand { get; }
    public ICommand EditSongCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    [Reactive] public partial string SetlistName { get; set; } = string.Empty;
    [Reactive] public partial DateTime SetlistDate { get; set; } = DateTime.Today;
    [Reactive] public partial string TotalDurationText { get; private set; } = "00:00";
    [Reactive] public partial bool IsSelectedSongsTab { get; private set; } = true;
    [Reactive] public partial bool IsCatalogueTab { get; private set; }
    [Reactive] public partial bool IsBusy { get; private set; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value ?? string.Empty);
            FilterCatalogue();
        }
    }

    public SetlistEditViewModel(
        SetlistService setlistService,
        SongService songService,
        IUserDialogService dialogs,
        IAppNavigationService navigation)
    {
        _setlistService = setlistService;
        _songService = songService;
        _dialogs = dialogs;
        _navigation = navigation;
        ShowSelectedSongsTabCommand = ReactiveCommand.Create(ShowSelectedSongsTab);
        ShowCatalogueTabCommand = ReactiveCommand.Create(ShowCatalogueTab);
        ToggleSongSelectionCommand = ReactiveCommand.Create<SelectableSongModel>(ToggleSongSelection);
        RemoveSongCommand = ReactiveCommand.Create<SelectableSongModel>(RemoveSong);
        EditSongCommand = ReactiveCommand.CreateFromTask<SelectableSongModel>(EditSongAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.CreateFromTask(CancelAsync);
    }

    public async Task InitializeAsync(SetlistWithSongs? setlistWithSongs)
    {
        IsBusy = true;
        try
        {
            _currentSetlist = setlistWithSongs?.Setlist ?? new Setlist { Date = DateTime.Today };
            SetlistName = _currentSetlist.Name;
            SetlistDate = _currentSetlist.Date;

            var allSongs = await _songService.GetSongsAsync();
            var selectedSongIds = setlistWithSongs?.Songs.Select(song => song.Id).ToHashSet() ?? new HashSet<int>();
            var songsInSetOrder = setlistWithSongs?.Songs
                .Select(selectedSong => allSongs.FirstOrDefault(song => song.Id == selectedSong.Id))
                .Where(song => song is not null)
                .Cast<Song>()
                .ToList() ?? new List<Song>();
            var unselectedSongs = allSongs.Where(song => !selectedSongIds.Contains(song.Id));

            AllSongs.Clear();
            SelectedSongs.Clear();
            foreach (var song in songsInSetOrder)
            {
                AddSongModel(song, isSelected: true);
            }

            foreach (var song in unselectedSongs)
            {
                AddSongModel(song, isSelected: false);
            }

            FilterCatalogue();
            RecalculateTotalDuration();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddSongModel(Song song, bool isSelected)
    {
        var model = new SelectableSongModel { Song = song, IsSelected = isSelected };
        model.PropertyChanged += OnSongModelPropertyChanged;
        AllSongs.Add(model);

        if (isSelected)
        {
            SelectedSongs.Add(model);
        }

        UpdateDisplayIndices();
    }

    private void OnSongModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SelectableSongModel.IsSelected) || sender is not SelectableSongModel model)
        {
            return;
        }

        if (model.IsSelected && !SelectedSongs.Contains(model))
        {
            SelectedSongs.Add(model);
        }
        else if (!model.IsSelected)
        {
            SelectedSongs.Remove(model);
        }

        UpdateDisplayIndices();
        RecalculateTotalDuration();
    }

    private void UpdateDisplayIndices()
    {
        for (int i = 0; i < SelectedSongs.Count; i++)
        {
            SelectedSongs[i].DisplayIndex = i + 1;
        }
    }

    private void FilterCatalogue()
    {
        var query = SearchText.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? AllSongs
            : AllSongs.Where(model =>
                model.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                model.Artist.Contains(query, StringComparison.OrdinalIgnoreCase));

        FilteredCatalogueSongs.Clear();
        foreach (var model in filtered)
        {
            FilteredCatalogueSongs.Add(model);
        }
    }

    private void RecalculateTotalDuration()
    {
        TotalDurationText = DurationFormatter.FormatTotal(
            SelectedSongs.Select(model => TimeSpan.FromSeconds(model.Song.DurationSeconds)));
    }

    private void ShowSelectedSongsTab()
    {
        IsSelectedSongsTab = true;
        IsCatalogueTab = false;
    }

    private void ShowCatalogueTab()
    {
        IsSelectedSongsTab = false;
        IsCatalogueTab = true;
    }

    private void ToggleSongSelection(SelectableSongModel model)
    {
        model.IsSelected = !model.IsSelected;
    }

    private void RemoveSong(SelectableSongModel model)
    {
        model.IsSelected = false;
    }

    private async Task EditSongAsync(SelectableSongModel model)
    {
        try
        {
            await _navigation.OpenSongDetailAsync(model.Song);
            var matchingSong = AllSongs.FirstOrDefault(item => item.Song.Id == model.Song.Id);
            matchingSong?.NotifySongChanged();
            RecalculateTotalDuration();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(SetlistName))
        {
            await _dialogs.ShowAlertAsync("Erreur", "Donne un nom à la setlist.");
            return;
        }

        try
        {
            _currentSetlist.Name = SetlistName.Trim();
            _currentSetlist.Date = SetlistDate.ToUniversalTime();
            var selectedIds = SelectedSongs.Select(model => model.Song.Id).ToList();

            await _setlistService.SaveSetlistAsync(_currentSetlist, selectedIds);
            await _navigation.CloseModalAsync();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }

    private async Task CancelAsync()
    {
        try
        {
            await _navigation.CloseModalAsync();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }

    public void StartDragging(SelectableSongModel? model)
    {
        _draggedSong = model;
    }

    public void ReorderSelectedSongs(SelectableSongModel? targetSong)
    {
        var draggedSong = _draggedSong;
        _draggedSong = null;

        if (draggedSong is null || targetSong is null || draggedSong == targetSong)
        {
            return;
        }

        var oldIndex = SelectedSongs.IndexOf(draggedSong);
        var newIndex = SelectedSongs.IndexOf(targetSong);
        if (oldIndex < 0 || newIndex < 0)
        {
            return;
        }

        SelectedSongs.Move(oldIndex, newIndex);
        UpdateDisplayIndices();
    }
}
