using MyBandApp.Models;
using MyBandApp.Models.Entities;
using MyBandApp.Services;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MyBandApp.Views;

public partial class SongDetailPopup : ContentPage
{
    private readonly SongService _songService;

    public Song CurrentSong { get; }
    public IReadOnlyList<string> AvailableKeys { get; }
    public PopupState State { get; }

    public SongDetailPopup(Song song, SongService songService)
    {
        InitializeComponent();
        CurrentSong = song;
        _songService = songService;

        var keys = MusicalKeys.All.ToList();
        if (!string.IsNullOrWhiteSpace(CurrentSong.Key) && !keys.Contains(CurrentSong.Key))
        {
            keys.Insert(0, CurrentSong.Key);
        }
        AvailableKeys = keys;

        State = new PopupState
        {
            SelectedKey = string.IsNullOrWhiteSpace(CurrentSong.Key) ? null : CurrentSong.Key,
            EditableBpm = CurrentSong.Bpm > 0 ? CurrentSong.Bpm.ToString() : string.Empty,
            EditableDuration = FormatDurationForEdit(CurrentSong.DurationSeconds), // adapte selon le type de Duration
            EditableYoutubeLinks = CurrentSong.YoutubeLinks ?? string.Empty
        };

        BindingContext = this;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validation BPM
            if (!string.IsNullOrWhiteSpace(State.EditableBpm))
            {
                if (!int.TryParse(State.EditableBpm.Trim(), out int bpm) || bpm <= 0 || bpm > 400)
                {
                    await DisplayAlertAsync("BPM invalide", "Entre un BPM valide (entre 1 et 400).", "OK");
                    return;
                }
                CurrentSong.Bpm = bpm;
            }
            else
            {
                CurrentSong.Bpm = 0;
            }

            // Validation et conversion Durée (mm:ss ou ss)
            if (!TryParseDuration(State.EditableDuration, out TimeSpan parsedDuration, out string? errorMessage))
            {
                await DisplayAlertAsync("Durée invalide", errorMessage ?? "Utilise le format mm:ss (ex. 03:45).", "OK");
                return;
            }

            ApplyDurationToSong(CurrentSong, parsedDuration);
            CurrentSong.Key = State.SelectedKey ?? string.Empty;
            CurrentSong.YoutubeLinks = State.EditableYoutubeLinks.Trim();

            await _songService.UpdateSongAsync(CurrentSong);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erreur", ex.Message, "OK");
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private static string FormatDurationForEdit(object? duration)
    {
        TimeSpan ts = duration switch
        {
            TimeSpan span => span,
            int seconds when seconds > 0 => TimeSpan.FromSeconds(seconds),
            double seconds when seconds > 0 => TimeSpan.FromSeconds(seconds),
            _ => TimeSpan.Zero
        };

        if (ts <= TimeSpan.Zero)
            return string.Empty;

        int totalMinutes = (int)ts.TotalMinutes;
        return $"{totalMinutes:00}:{ts.Seconds:00}";
    }

    private static bool TryParseDuration(string? input, out TimeSpan result, out string? errorMessage)
    {
        result = TimeSpan.Zero;
        errorMessage = null;

        var trimmed = input?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmed))
            return true;

        // Cas "mm:ss" ou "m:ss" ou "hh:mm:ss"
        if (trimmed.Contains(':'))
        {
            var parts = trimmed.Split(':');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seconds))
            {
                if (minutes < 0 || seconds < 0 || seconds >= 60)
                {
                    errorMessage = "Les secondes doivent être comprises entre 00 et 59 (ex. 03:45).";
                    return false;
                }

                result = new TimeSpan(0, minutes, seconds);
                return true;
            }

            errorMessage = "Format attendu : mm:ss (ex. 03:45).";
            return false;
        }

        // Cas saisie directe en secondes (ex. "225" -> 03:45)
        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int totalSeconds) && totalSeconds >= 0)
        {
            result = TimeSpan.FromSeconds(totalSeconds);
            return true;
        }

        errorMessage = "Entre une durée au format mm:ss (ex. 03:45) ou en secondes.";
        return false;
    }

    private static void ApplyDurationToSong(Song song, TimeSpan duration)
    {
        // Adapte cette ligne selon le type réel de Song.Duration :
        // - Si Song.Duration est un TimeSpan : song.Duration = duration;
        // - Si Song.Duration est un int (secondes) : song.Duration = (int)duration.TotalSeconds;
        song.DurationSeconds = (int)duration.TotalSeconds;
    }
}

public class PopupState : INotifyPropertyChanged
{
    private string? _selectedKey;
    private string _editableBpm = string.Empty;
    private string _editableDuration = string.Empty;
    private string _editableYoutubeLinks = string.Empty;

    public string? SelectedKey
    {
        get => _selectedKey;
        set
        {
            if (_selectedKey == value) return;
            _selectedKey = value;
            OnPropertyChanged();
        }
    }

    public string EditableBpm
    {
        get => _editableBpm;
        set
        {
            if (_editableBpm == value) return;
            _editableBpm = value;
            OnPropertyChanged();
        }
    }

    public string EditableDuration
    {
        get => _editableDuration;
        set
        {
            if (_editableDuration == value) return;
            _editableDuration = value;
            OnPropertyChanged();
        }
    }

    public string EditableYoutubeLinks
    {
        get => _editableYoutubeLinks;
        set
        {
            if (_editableYoutubeLinks == value) return;
            _editableYoutubeLinks = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}