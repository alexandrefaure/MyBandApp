using MyBandApp.Models.Entities;

namespace MyBandApp.Services;

public class SongService
{
    private readonly Supabase.Client _supabase;

    public SongService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<Song>> GetSongsAsync()
    {
        await _supabase.InitializeAsync();
        var response = await _supabase
            .From<Song>()
            .Order("title", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        return response.Models;
    }

    public async Task<Song?> AddSongAsync(Song song)
    {
        await _supabase.InitializeAsync();
        var response = await _supabase
            .From<Song>()
            .Insert(song);

        return response.Models.FirstOrDefault();
    }

    public async Task DeleteSongAsync(int id)
    {
        await _supabase.InitializeAsync();
        await _supabase
            .From<Song>()
            .Where(s => s.Id == id)
            .Delete();
    }

    public async Task UpdateSongAsync(Song song)
    {
        await _supabase.InitializeAsync();
        await _supabase
            .From<Song>()
            .Where(s => s.Id == song.Id)
            .Set(s => s.YoutubeLinks, song.YoutubeLinks)
            .Update();
    }
}