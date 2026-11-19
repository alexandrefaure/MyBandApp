using MyBandApp.Models.Entities;

namespace MyBandApp.Services;

public class SongService
{
    private readonly Supabase.Client _supabase;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private List<Song>? _cachedSongs;

    public SongService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<Song>> GetSongsAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedSongs is not null)
        {
            return _cachedSongs;
        }

        await _cacheLock.WaitAsync();
        try
        {
            if (!forceRefresh && _cachedSongs is not null)
            {
                return _cachedSongs;
            }

            await _supabase.InitializeAsync();
            var response = await _supabase
                .From<Song>()
                .Order("title", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            _cachedSongs = response.Models
                .OrderBy(song => song.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return _cachedSongs;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<Song?> AddSongAsync(Song song)
    {
        await _supabase.InitializeAsync();
        var response = await _supabase
            .From<Song>()
            .Insert(song);

        var created = response.Models.FirstOrDefault();
        if (created is not null)
        {
            _cachedSongs ??= new List<Song>();
            _cachedSongs.Add(created);
            _cachedSongs = _cachedSongs
                .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return created;
    }

    public async Task DeleteSongAsync(int id)
    {
        await _supabase.InitializeAsync();
        await _supabase
            .From<Song>()
            .Where(s => s.Id == id)
            .Delete();

        _cachedSongs = _cachedSongs?
            .Where(song => song.Id != id)
            .ToList();
    }

    public async Task UpdateSongAsync(Song song)
    {
        await _supabase.InitializeAsync();
        await _supabase
            .From<Song>()
            .Where(s => s.Id == song.Id)
            .Set(s => s.Key, song.Key)
            .Set(s => s.Bpm, song.Bpm)
            .Set(s => s.DurationSeconds, song.DurationSeconds)
            .Set(s => s.YoutubeLinks, song.YoutubeLinks)
            .Update();

        if (_cachedSongs is not null)
        {
            var index = _cachedSongs.FindIndex(item => item.Id == song.Id);
            if (index >= 0)
            {
                _cachedSongs[index] = new Song
                {
                    Id = song.Id,
                    Title = _cachedSongs[index].Title,
                    Artist = _cachedSongs[index].Artist,
                    Key = song.Key,
                    Bpm = song.Bpm,
                    DurationSeconds = song.DurationSeconds,
                    YoutubeLinks = song.YoutubeLinks
                };
            }
        }
    }
}