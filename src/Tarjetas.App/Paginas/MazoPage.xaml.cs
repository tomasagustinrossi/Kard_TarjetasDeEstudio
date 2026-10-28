using Tarjetas.App.ViewModels;

namespace Tarjetas.App.Paginas;

public partial class MazoPage : ContentPage
{
    private readonly MazoViewModel vm;

    public MazoPage(MazoViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
        SizeChanged += (_, _) => Rejilla.Ajustar(Lista, Width);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Application.Current!.RequestedThemeChanged += AlCambiarTema;
        await vm.CargarCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Application.Current!.RequestedThemeChanged -= AlCambiarTema;
    }

    private void AlCambiarTema(object? sender, AppThemeChangedEventArgs e) => vm.Refrescar();
}
