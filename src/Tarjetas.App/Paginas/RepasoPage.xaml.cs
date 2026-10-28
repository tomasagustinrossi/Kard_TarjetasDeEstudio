using Tarjetas.App.ViewModels;

namespace Tarjetas.App.Paginas;

public partial class RepasoPage : ContentPage
{
    public RepasoPage(RepasoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.TarjetaMostrada += async () => await AnimarEntradaAsync();
        vm.RespuestaRevelada += async () => await AnimarRespuestaAsync();
    }

    // Cada tarjeta entra deslizándose suavemente desde la derecha.
    private async Task AnimarEntradaAsync()
    {
        PanelPregunta.Opacity = 0;
        PanelPregunta.TranslationX = 40;
        await Task.WhenAll(
            PanelPregunta.FadeToAsync(1, 240, Easing.CubicOut),
            PanelPregunta.TranslateToAsync(0, 0, 240, Easing.CubicOut));
    }

    // La respuesta aparece con un fundido corto.
    private async Task AnimarRespuestaAsync()
    {
        ZonaRespuesta.Opacity = 0.2;
        await ZonaRespuesta.FadeToAsync(1, 220, Easing.CubicOut);
    }
}
