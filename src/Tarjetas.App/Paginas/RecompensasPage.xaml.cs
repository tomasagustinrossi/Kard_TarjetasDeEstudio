using Tarjetas.App.ViewModels;

namespace Tarjetas.App.Paginas;

public partial class RecompensasPage : ContentPage
{
    private readonly RecompensasViewModel vm;

    public RecompensasPage(RecompensasViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.CargarCommand.ExecuteAsync(null);
    }
}
