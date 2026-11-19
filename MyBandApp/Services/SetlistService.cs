using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;

namespace MyBandApp.Services;

public class SetlistService
{
    private readonly Supabase.Client _supabase;

    public SetlistService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<SetlistWithSongs>> GetSetlistsWithSongsAsync()
    {
        await _supabase.InitializeAsync();
        var response = await _supabase.From<Setlist>().Get();
        var setlists = response.Models;

        var result = new List<SetlistWithSongs>();

        foreach (var setlist in setlists)
        {
            var songs = await GetSongsForSetlistAsync(setlist.Id);
            result.Add(new SetlistWithSongs
            {
                Setlist = setlist,
                Songs = songs
            });
        }

        return result;
    }

    public async Task<List<Song>> GetSongsForSetlistAsync(int setlistId)
    {
        var relationsResponse = await _supabase
            .From<SetlistSong>()
            .Where(x => x.SetlistId == setlistId)
            .Get();

        var songIds = relationsResponse.Models.Select(x => x.SongId).ToList();
        if (!songIds.Any()) return new List<Song>();

        var songsResponse = await _supabase
            .From<Song>()
            .Get();

        return songsResponse.Models.Where(s => songIds.Contains(s.Id)).ToList();
    }

    public async Task SaveSetlistAsync(Setlist setlist, List<int> selectedSongIds)
    {
        await _supabase.InitializeAsync();

        int setlistId = setlist.Id;

        if (setlistId == 0)
        {
            var insertResponse = await _supabase.From<Setlist>().Insert(setlist);
            var inserted = insertResponse.Models.FirstOrDefault();
            if (inserted != null) setlistId = inserted.Id;
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
    }

    public async Task DeleteSetlistAsync(int id)
    {
        await _supabase.InitializeAsync();
        await _supabase.From<Setlist>().Where(x => x.Id == id).Delete();
    }
}