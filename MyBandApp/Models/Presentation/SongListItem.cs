using MyBandApp.Models.Entities;
using ReactiveUI;

namespace MyBandApp.Models.Presentation;

public class SongListItem : ReactiveObject
{
    public Song Song { get; }
    public string Title => Song.Title;
    public string Artist => Song.Artist;
    public string Key => Song.Key;
    public int Bpm => Song.Bpm;
    public string FormattedDuration
    {
        get
        {
            if (Song.DurationSeconds <= 0)
            {
                return "--:--";
            }

            var duration = TimeSpan.FromSeconds(Song.DurationSeconds);
            return duration.TotalHours >= 1
                ? $"{(int)duration.TotalHours:00}h{duration.Minutes:00}"
                : $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
        }
    }

    public SongListItem(Song song)
    {
        Song = song;
    }

    public void NotifySongChanged()
    {
        this.RaisePropertyChanged(nameof(Title));
        this.RaisePropertyChanged(nameof(Artist));
        this.RaisePropertyChanged(nameof(Key));
        this.RaisePropertyChanged(nameof(Bpm));
        this.RaisePropertyChanged(nameof(FormattedDuration));
    }
}
