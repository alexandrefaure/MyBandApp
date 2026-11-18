using MyBandApp.Models.Entities;
using MyBandApp.Services;

namespace MyBandApp.Views;

public partial class SongDetailPopup : ContentPage
{
    private readonly SongService _songService;
    public Song CurrentSong { get; set; }
    public string EditableYoutubeLinks { get; set; }

    public SongDetailPopup(Song song, SongService songService)
    {
        InitializeComponent();
        CurrentSong = song;
        _songService = songService;
        EditableYoutubeLinks = song.YoutubeLinks ?? string.Empty;

        BindingContext = this;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            CurrentSong.YoutubeLinks = EditableYoutubeLinks?.Trim() ?? string.Empty;
            await _songService.UpdateSongAsync(CurrentSong);

            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}