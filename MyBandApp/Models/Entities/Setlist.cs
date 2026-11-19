using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyBandApp.Models.Entities;

[Table("setlists")]
public class Setlist : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; } = DateTime.Today;
}

[Table("setlist_songs")] // <-- Ajoute cet attribut ici
public class SetlistSong : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("setlist_id")]
    public int SetlistId { get; set; }

    [Column("song_id")]
    public int SongId { get; set; }
}