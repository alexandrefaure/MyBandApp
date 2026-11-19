using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using System.Collections.ObjectModel;

namespace MyBandApp.ViewModels;

public partial class SetlistsViewModel : ObservableObject
{
    private readonly SetlistService _setlistService;
    private readonly SongService _songService;

    public ObservableCollection<SetlistWithSongs> Setlists { get; } = new();

    [ObservableProperty] private bool isBusy;

    public SetlistsViewModel(SetlistService setlistService, SongService songService)
    {
        _setlistService = setlistService;
        _songService = songService;
    }

    [RelayCommand]
    public async Task LoadSetlistsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var list = await _setlistService.GetSetlistsWithSongsAsync();
            Setlists.Clear();
            foreach (var s in list) Setlists.Add(s);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddSetlistAsync()
    {
        var allSongs = await _songService.GetSongsAsync();
        // On passe null pour la création
        var editPage = new MyBandApp.Views.SetlistEditPage(null, allSongs, _setlistService, _songService);
        await Shell.Current.Navigation.PushModalAsync(editPage);
    }

    [RelayCommand]
    private async Task EditSetlistAsync(SetlistWithSongs setlistWithSongs)
    {
        if (setlistWithSongs == null) return;
        var allSongs = await _songService.GetSongsAsync();
        // On passe l'objet SetlistWithSongs complet
        var editPage = new MyBandApp.Views.SetlistEditPage(setlistWithSongs, allSongs, _setlistService, _songService);
        await Shell.Current.Navigation.PushModalAsync(editPage);
    }

    [RelayCommand]
    private async Task DeleteSetlistAsync(SetlistWithSongs setlistWithSongs)
    {
        if (setlistWithSongs == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Supprimer", $"Supprimer la setlist « {setlistWithSongs.Name} » ?", "Oui", "Non");
        if (!confirm) return;

        try
        {
            await _setlistService.DeleteSetlistAsync(setlistWithSongs.Id);
            Setlists.Remove(setlistWithSongs);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
    }
}