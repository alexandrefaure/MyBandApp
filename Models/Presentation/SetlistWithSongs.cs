using MyBandApp.Models.Entities;

namespace MyBandApp.Models.Presentation;

public class SetlistWithSongs
{
    public Setlist Setlist { get; set; } = new();

    public int Id => Setlist.Id;
    public string Name
    {
        get => Setlist.Name;
        set => Setlist.Name = value;
    }
    public DateTime Date
    {
        get => Setlist.Date;
        set => Setlist.Date = value;
    }

    public TimeSpan TotalDuration =>
        TimeSpan.FromSeconds(Songs?.Sum(s => s.DurationSeconds) ?? 0);

    public string FormattedTotalDuration =>
        TotalDuration == TimeSpan.Zero
            ? "--:--"
            : TotalDuration.TotalHours >= 1
                ? $"{(int)TotalDuration.TotalHours:00}h{TotalDuration.Minutes:00}"
                : $"{(int)TotalDuration.TotalMinutes:00}:{TotalDuration.Seconds:00}";

    public string FormattedDate => Setlist.Date.ToString("dd/MM/yyyy");
    public List<Song> Songs { get; set; } = new();
    public string SummarySongsCount => $"{Songs.Count} morceau(x)";
}