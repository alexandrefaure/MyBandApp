using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyBandApp.ViewModels;

public partial class SetlistsViewModel : ReactiveObject
{
    private readonly SetlistService _setlistService;
    private readonly IUserDialogService _dialogs;
    private readonly IAppNavigationService _navigation;
    private bool _hasLoadedOnce;

    public ObservableCollection<SetlistWithSongs> Setlists { get; } = new();
    public ICommand LoadSetlistsCommand { get; }
    public ICommand AddSetlistCommand { get; }
    public ICommand EditSetlistCommand { get; }
    public ICommand DeleteSetlistCommand { get; }

    [Reactive] public partial bool IsBusy { get; set; }

    public SetlistsViewModel(
        SetlistService setlistService,
        IUserDialogService dialogs,
        IAppNavigationService navigation)
    {
        _setlistService = setlistService;
        _dialogs = dialogs;
        _navigation = navigation;
        LoadSetlistsCommand = ReactiveCommand.CreateFromTask(() => LoadSetlistsAsync());
        AddSetlistCommand = ReactiveCommand.CreateFromTask(() => OpenSetlistEditorAsync(null));
        EditSetlistCommand = ReactiveCommand.CreateFromTask<SetlistWithSongs>(OpenSetlistEditorAsync);
        DeleteSetlistCommand = ReactiveCommand.CreateFromTask<SetlistWithSongs>(DeleteSetlistAsync);
    }

    public async Task LoadSetlistsAsync(bool forceRefresh = false)
    {
        if (IsBusy)
        {
            return;
        }

        if (!forceRefresh && _hasLoadedOnce)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var list = await _setlistService.GetSetlistsWithSongsAsync(forceRefresh);
            _hasLoadedOnce = true;
            Setlists.Clear();
            foreach (var setlist in list)
            {
                Setlists.Add(setlist);
            }
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenSetlistEditorAsync(SetlistWithSongs? setlist)
    {
        try
        {
            await _navigation.OpenSetlistEditorAsync(setlist);
            await LoadSetlistsAsync(forceRefresh: true);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }

    private async Task DeleteSetlistAsync(SetlistWithSongs setlistWithSongs)
    {
        bool confirm = await _dialogs.ConfirmAsync(
            "Supprimer",
            $"Supprimer la setlist « {setlistWithSongs.Name} » ?",
            "Oui",
            "Non");
        if (!confirm)
        {
            return;
        }

        try
        {
            await _setlistService.DeleteSetlistAsync(setlistWithSongs.Id);
            Setlists.Remove(setlistWithSongs);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync("Erreur", ex.Message);
        }
    }
}
