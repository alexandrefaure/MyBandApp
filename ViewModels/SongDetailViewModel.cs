using MyBandApp.Models;
using MyBandApp.Models.Entities;
using MyBandApp.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Globalization;
using System.Windows.Input;

namespace MyBandApp.ViewModels;

public partial class SongDetailViewModel : ReactiveObject
{
    private readonly SongService _songService;
    private readonly IUserDialogService _dialogs;
    private readonly IAppNavigationService _navigation;

    [Reactive] public partial string Title { get; private set; } = string.Empty;
    [Reactive] public partial string Artist { get; private set; } = string.Empty;
    [Reactive] public partial string? SelectedKey { get; set; }
    [Reactive] public partial string EditableBpm { get; set; } = string.Empty;
    [Reactive] public partial string EditableDuration { get; set; } = string.Empty;
    [Reactive] public partial string EditableYoutubeLinks { get; set; } = string.Empty;

    public IReadOnlyList<string> AvailableKeys { get; private set; } = MusicalKeys.All;
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }

    private Song? _currentSong;

    public SongDetailViewModel(
        SongService songService,
        IUserDialogService dialogs,
        IAppNavigationService navigation)
    {
        _songService = songService;
        _dialogs = dialogs;
        _navigation = navigation;
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CloseCommand = ReactiveCommand.CreateFromTask(CloseAsync);
    }

    public void Initialize(Song song)
    {
        _currentSong = song;
        Title = song.Title;
        Artist = song.Artist;

        var keys = MusicalKeys.All.ToList();
        if (!string.IsNullOrWhiteSpace(song.Key) && !keys.Contains(song.Key))
            keys.Insert(0, song.Key);
        AvailableKeys = keys;
        this.RaisePropertyChanged(nameof(AvailableKeys));

        SelectedKey = string.IsNullOrWhiteSpace(song.Key) ? null : song.Key;
        EditableBpm = song.Bpm > 0 ? song.Bpm.ToString(CultureInfo.InvariantCulture) : string.Empty;
        EditableDuration = FormatDurationForEdit(song.DurationSeconds);
        EditableYoutubeLinks = song.YoutubeLinks ?? string.Empty;
    }

    private async Task SaveAsync()
    {
        if (_currentSong is null)
        {
            await _dialogs.ShowAlertAsync("Erreur", "Aucun morceau n'est chargé.");
            return;
        }

        int bpm = 0;
        if (!string.IsNullOrWhiteSpace(EditableBpm) &&
            (!int.TryParse(EditableBpm.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out bpm) || bpm <= 0 || bpm > 400))
        {
            await _dialogs.ShowAlertAsync("BPM invalide", "Entre un BPM valide (entre 1 et 400).");
            return;
        }

        if (!TryParseDuration(EditableDuration, out var durationSeconds, out var durationError))
        {
            await _dialogs.ShowAlertAsync("Durée invalide", durationError);
            return;
        }

        var updatedSong = new Song
        {
            Id = _currentSong.Id,
            Title = _currentSong.Title,
            Artist = _currentSong.Artist,
            Bpm = bpm,
            Key = SelectedKey ?? string.Empty,
            DurationSeconds = durationSeconds,
            YoutubeLinks = EditableYoutubeLinks.Trim()
        };

        try
        {
            await _songService.UpdateSongAsync(updatedSong);
            _currentSong.Bpm = updatedSong.Bpm;
            _currentSong.Key = updatedSong.Key;
            _currentSong.DurationSeconds = updatedSong.DurationSeconds;
            _currentSong.YoutubeLinks = updatedSong.YoutubeLinks;
            await _navigation.CloseModalAsync();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }

    private async Task CloseAsync()
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

    private static string FormatDurationForEdit(int seconds)
    {
        if (seconds <= 0)
            return string.Empty;

        var duration = TimeSpan.FromSeconds(seconds);
        return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
    }

    private static bool TryParseDuration(string? input, out int totalSeconds, out string errorMessage)
    {
        totalSeconds = 0;
        errorMessage = "Entre une durée au format mm:ss (ex. 03:45) ou en secondes.";

        var value = input?.Trim() ?? string.Empty;
        if (value.Length == 0)
            return true;

        if (value.Contains(':'))
        {
            var parts = value.Split(':');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                return false;

            if (minutes < 0 || seconds < 0 || seconds >= 60)
            {
                errorMessage = "Les secondes doivent être comprises entre 00 et 59 (ex. 03:45).";
                return false;
            }

            totalSeconds = (int)TimeSpan.FromMinutes(minutes).TotalSeconds + seconds;
            return true;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out totalSeconds) && totalSeconds >= 0)
            return true;

        errorMessage = "Entre une durée au format mm:ss (ex. 03:45) ou en secondes.";
        return false;
    }
}
