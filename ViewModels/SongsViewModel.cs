using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyBandApp.Models.Entities;
using MyBandApp.Services;
using System.Collections.ObjectModel;

namespace MyBandApp.ViewModels;

public partial class SongsViewModel : ObservableObject
{
    private readonly SongService _songService;
    private List<Song> _allSongs = new();

    public ObservableCollection<Song> FilteredSongs { get; } = new();

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isAddFormVisible;
    [ObservableProperty] private string searchText = string.Empty;

    // Champs pour le nouveau morceau
    [ObservableProperty] private string newTitle = string.Empty;
    [ObservableProperty] private string newArtist = string.Empty;
    [ObservableProperty] private string newBpm = "115";
    [ObservableProperty] private string newKey = "Am";
    [ObservableProperty] private string newDurationSeconds = "210";

    public SongsViewModel(SongService songService)
    {
        _songService = songService;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredSongs.Clear();
        var query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

        var matches = string.IsNullOrEmpty(query)
            ? _allSongs
            : _allSongs.Where(s =>
                s.Title.ToLowerInvariant().Contains(query) ||
                s.Artist.ToLowerInvariant().Contains(query) ||
                s.Key.ToLowerInvariant().Contains(query));

        foreach (var song in matches)
            FilteredSongs.Add(song);
    }

    [RelayCommand]
    public async Task LoadSongsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            _allSongs = await _songService.GetSongsAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur Supabase", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleAddForm() => IsAddFormVisible = !IsAddFormVisible;

    [RelayCommand]
    private async Task AddSongAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewArtist))
        {
            await Shell.Current.DisplayAlert("Champs requis", "Indique au moins le titre et l'artiste.", "OK");
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
            if (created != null)
            {
                _allSongs.Add(created);
                _allSongs = _allSongs.OrderBy(s => s.Title).ToList();
                ApplyFilter();

                // Reset formulaire
                NewTitle = string.Empty;
                NewArtist = string.Empty;
                IsAddFormVisible = false;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur d'ajout", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSongAsync(Song song)
    {
        if (song == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Supprimer", $"Retirer « {song.Title} » du répertoire ?", "Oui", "Non");
        if (!confirm) return;

        try
        {
            await _songService.DeleteSongAsync(song.Id);
            _allSongs.Remove(song);
            FilteredSongs.Remove(song);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task OpenDetailAsync(Song song)
    {
        if (song == null) return;

        // Récupération du SongService depuis MauiServiceProvider ou passage direct
        var songService = Application.Current.Handler.MauiContext.Services.GetService<SongService>();

        var detailPage = new MyBandApp.Views.SongDetailPopup(song, songService);
        await Shell.Current.Navigation.PushModalAsync(detailPage);
    }
}