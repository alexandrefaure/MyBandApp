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

    public string FormattedDate => Setlist.Date.ToString("dd/MM/yyyy");
    public List<Song> Songs { get; set; } = new();
    public string SummarySongsCount => $"{Songs.Count} morceau(x)";
}