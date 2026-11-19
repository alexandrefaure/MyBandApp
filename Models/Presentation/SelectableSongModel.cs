using MyBandApp.Models.Entities;

namespace MyBandApp.Models.Presentation;

public class SelectableSongModel : BindableObject
{
    private Song _song = new();
    public Song Song
    {
        get => _song;
        set
        {
            if (_song == value) return;
            _song = value;
            RefreshDuration();
            OnPropertyChanged();
            NotifySongChanged();
        }
    }

    public string Title => Song?.Title ?? string.Empty;
    public string Artist => Song?.Artist ?? string.Empty;
    public string Key => Song?.Key ?? string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    private int _displayIndex;
    public int DisplayIndex
    {
        get => _displayIndex;
        set
        {
            if (_displayIndex != value)
            {
                _displayIndex = value;
                OnPropertyChanged();
            }
        }
    }

    private string _formattedDuration = "--:--";
    public string FormattedDuration
    {
        get => _formattedDuration;
        private set
        {
            if (_formattedDuration == value) return;
            _formattedDuration = value;
            OnPropertyChanged();
        }
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
        OnPropertyChanged(nameof(Song));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Artist));
        OnPropertyChanged(nameof(Key));
        OnPropertyChanged(nameof(FormattedDuration));
    }
}