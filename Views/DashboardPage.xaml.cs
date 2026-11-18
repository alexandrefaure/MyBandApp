namespace MyBandApp.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void OnGoToSongsClicked(object sender, EventArgs e)
    {
        // Redirige vers l'onglet Répertoire
        await Shell.Current.GoToAsync("///SongsPage");
    }

    private async void OnGoToSetlistsClicked(object sender, EventArgs e)
    {
        // Redirige vers l'onglet Setlists
        await Shell.Current.GoToAsync("///SetlistsPage");
    }
}