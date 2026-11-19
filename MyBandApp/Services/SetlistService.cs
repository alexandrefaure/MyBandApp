using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;

namespace MyBandApp.Services;

public class SetlistService
{
    private readonly Supabase.Client _supabase;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private List<SetlistWithSongs>? _cachedSetlists;

    public SetlistService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<SetlistWithSongs>> GetSetlistsWithSongsAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedSetlists is not null)
        {
            return _cachedSetlists;
        }

        await _cacheLock.WaitAsync();
        try
        {
            if (!forceRefresh && _cachedSetlists is not null)
            {
                return _cachedSetlists;
            }

            await _supabase.InitializeAsync();
            var setlistsResponse = await _supabase.From<Setlist>().Get();
            var relationsResponse = await _supabase.From<SetlistSong>().Get();

            var setlistSongsById = relationsResponse.Models
                .GroupBy(relation => relation.SetlistId)
                .ToDictionary(group => group.Key, group => group.Select(relation => relation.SongId).ToList());

            var allSongIds = setlistSongsById.Values
                .SelectMany(songIds => songIds)
                .Distinct()
                .ToList();

            var songsById = allSongIds.Count == 0
                ? new Dictionary<int, Song>()
                : (await _supabase.From<Song>().Get()).Models
                    .GroupBy(song => song.Id)
                    .ToDictionary(group => group.Key, group => group.First());

            var result = new List<SetlistWithSongs>();
            foreach (var setlist in setlistsResponse.Models)
            {
                var songIds = setlistSongsById.TryGetValue(setlist.Id, out var ids)
                    ? ids
                    : new List<int>();

                var songs = songIds
                    .Select(songId => songsById.TryGetValue(songId, out var song) ? song : null)
                    .Where(song => song is not null)
                    .Cast<Song>()
                    .ToList();

                result.Add(new SetlistWithSongs
                {
                    Setlist = setlist,
                    Songs = songs
                });
            }

            _cachedSetlists = result
                .OrderByDescending(setlist => setlist.Date)
                .ToList();

            return _cachedSetlists;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<List<Song>> GetSongsForSetlistAsync(int setlistId)
    {
        var cachedSetlists = await GetSetlistsWithSongsAsync();
        return cachedSetlists
            .FirstOrDefault(setlist => setlist.Id == setlistId)
            ?.Songs ?? new List<Song>();
    }

    public async Task SaveSetlistAsync(Setlist setlist, List<int> selectedSongIds)
    {
        await _supabase.InitializeAsync();

        int setlistId = setlist.Id;

        if (setlistId == 0)
        {
            var insertResponse = await _supabase.From<Setlist>().Insert(setlist);
            var inserted = insertResponse.Models.FirstOrDefault();
            if (inserted != null)
            {
                setlistId = inserted.Id;
            }
        }
        else
        {
            await _supabase.From<Setlist>()
                .Where(x => x.Id == setlistId)
                .Set(x => x.Name, setlist.Name)
                .Set(x => x.Date, setlist.Date)
                .Update();

            var existingRelations = await _supabase.From<SetlistSong>().Where(x => x.SetlistId == setlistId).Get();
            foreach (var rel in existingRelations.Models)
            {
                await _supabase.From<SetlistSong>().Where(x => x.Id == rel.Id).Delete();
            }
        }

        foreach (var songId in selectedSongIds)
        {
            var relation = new SetlistSong { SetlistId = setlistId, SongId = songId };
            await _supabase.From<SetlistSong>().Insert(relation);
        }

        _cachedSetlists = null;
    }

    public async Task DeleteSetlistAsync(int id)
    {
        await _supabase.InitializeAsync();
        await _supabase.From<Setlist>().Where(x => x.Id == id).Delete();
        _cachedSetlists = _cachedSetlists?
            .Where(setlist => setlist.Id != id)
            .ToList();
    }
}