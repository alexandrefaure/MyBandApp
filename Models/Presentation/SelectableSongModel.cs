using MyBandApp.Models.Entities;
using ReactiveUI;

namespace MyBandApp.Models.Presentation;

public class SelectableSongModel : ReactiveObject
{
    private Song _song = new();
    public Song Song
    {
        get => _song;
        set
        {
            this.RaiseAndSetIfChanged(ref _song, value);
            RefreshDuration();
            RaiseSongPropertiesChanged();
        }
    }

    public string Title => Song?.Title ?? string.Empty;
    public string Artist => Song?.Artist ?? string.Empty;
    public string Key => Song?.Key ?? string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    private int _displayIndex;
    public int DisplayIndex
    {
        get => _displayIndex;
        set => this.RaiseAndSetIfChanged(ref _displayIndex, value);
    }

    private string _formattedDuration = "--:--";
    public string FormattedDuration
    {
        get => _formattedDuration;
        private set => this.RaiseAndSetIfChanged(ref _formattedDuration, value);
    }

    public void RefreshDuration()
    {
        var seconds = Song?.DurationSeconds ?? 0;
        if (seconds <= 0)
        {
            FormattedDuration = "--:--";
            return;
        }

        var ts = TimeSpan.FromSeconds(seconds);
        FormattedDuration = ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours:00}h{ts.Minutes:00}"
            : $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
    }

    public void NotifySongChanged()
    {
        RefreshDuration();
        RaiseSongPropertiesChanged();
    }

    private void RaiseSongPropertiesChanged()
    {
        this.RaisePropertyChanged(nameof(Song));
        this.RaisePropertyChanged(nameof(Title));
        this.RaisePropertyChanged(nameof(Artist));
        this.RaisePropertyChanged(nameof(Key));
        this.RaisePropertyChanged(nameof(FormattedDuration));
    }
}