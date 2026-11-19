using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyBandApp.ViewModels;

public partial class SongsViewModel : ReactiveObject
{
    private readonly SongService _songService;
    private readonly IUserDialogService _dialogs;
    private readonly IAppNavigationService _navigation;
    private List<SongListItem> _allSongs = new();
    private string _searchText = string.Empty;

    public ObservableCollection<SongListItem> FilteredSongs { get; } = new();
    public ICommand LoadSongsCommand { get; }
    public ICommand ToggleAddFormCommand { get; }
    public ICommand AddSongCommand { get; }
    public ICommand DeleteSongCommand { get; }
    public ICommand OpenDetailCommand { get; }

    [Reactive] public partial bool IsBusy { get; set; }
    [Reactive] public partial bool IsAddFormVisible { get; set; }
    [Reactive] public partial string NewTitle { get; set; } = string.Empty;
    [Reactive] public partial string NewArtist { get; set; } = string.Empty;
    [Reactive] public partial string NewBpm { get; set; } = "115";
    [Reactive] public partial string NewKey { get; set; } = "Am";
    [Reactive] public partial string NewDurationSeconds { get; set; } = "210";

    public SongsViewModel(
        SongService songService,
        IUserDialogService dialogs,
        IAppNavigationService navigation)
    {
        _songService = songService;
        _dialogs = dialogs;
        _navigation = navigation;
        LoadSongsCommand = ReactiveCommand.CreateFromTask(LoadSongsAsync);
        ToggleAddFormCommand = ReactiveCommand.Create(ToggleAddForm);
        AddSongCommand = ReactiveCommand.CreateFromTask(AddSongAsync);
        DeleteSongCommand = ReactiveCommand.CreateFromTask<SongListItem>(DeleteSongAsync);
        OpenDetailCommand = ReactiveCommand.CreateFromTask<SongListItem>(OpenDetailAsync);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value ?? string.Empty);
            ApplyFilter();
        }
    }

    private void ApplyFilter()
    {
        FilteredSongs.Clear();
        var query = SearchText?.Trim() ?? string.Empty;

        var matches = string.IsNullOrEmpty(query)
            ? _allSongs
            : _allSongs.Where(song =>
                song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                song.Artist.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                song.Key.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var song in matches)
            FilteredSongs.Add(song);
    }

    public async Task LoadSongsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            _allSongs = (await _songService.GetSongsAsync()).Select(song => new SongListItem(song)).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur Supabase", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ToggleAddForm() => IsAddFormVisible = !IsAddFormVisible;

    private async Task AddSongAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewArtist))
        {
            await _dialogs.ShowAlertAsync("Champs requis", "Indique au moins le titre et l'artiste.");
            return;
        }

        try
        {
            IsBusy = true;
            int.TryParse(NewBpm, out int bpm);
            int.TryParse(NewDurationSeconds, out int duration);

            var song = new Song
            {
                Title = NewTitle.Trim(),
                Artist = NewArtist.Trim(),
                Bpm = bpm > 0 ? bpm : 120,
                Key = string.IsNullOrWhiteSpace(NewKey) ? "-" : NewKey.Trim(),
                DurationSeconds = duration > 0 ? duration : 180
            };

            var created = await _songService.AddSongAsync(song);
            if (created is not null)
            {
                _allSongs.Add(new SongListItem(created));
                _allSongs = _allSongs.OrderBy(item => item.Title).ToList();
                ApplyFilter();

                NewTitle = string.Empty;
                NewArtist = string.Empty;
                IsAddFormVisible = false;
            }
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur d'ajout", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteSongAsync(SongListItem songItem)
    {
        bool confirm = await _dialogs.ConfirmAsync(
            "Supprimer",
            $"Retirer « {songItem.Title} » du répertoire ?",
            "Oui",
            "Non");
        if (!confirm) return;

        try
        {
            await _songService.DeleteSongAsync(songItem.Song.Id);
            _allSongs.Remove(songItem);
            FilteredSongs.Remove(songItem);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }

    private async Task OpenDetailAsync(SongListItem songItem)
    {
        try
        {
            await _navigation.OpenSongDetailAsync(songItem.Song);
            songItem.NotifySongChanged();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }
}
