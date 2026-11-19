using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyBandApp.Models.Entities;

[Table("songs")]
public class Song : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("artist")]
    public string Artist { get; set; } = string.Empty;

    [Column("bpm")]
    public int Bpm { get; set; }

    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("youtube_links")]
    public string YoutubeLinks { get; set; } = string.Empty;
}