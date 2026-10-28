using Tarjetas.App.Paginas;
using Tarjetas.App.ViewModels;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Infraestructura;

namespace Tarjetas.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        var servicios = builder.Services;

        // Infraestructura: un archivo liviano por mazo
        var carpetaMazos = Path.Combine(FileSystem.AppDataDirectory, "mazos");
        servicios.AddSingleton<IMazoRepositorio>(_ => new RepositorioDeMazosJson(carpetaMazos));
        servicios.AddSingleton(TimeProvider.System);

        // Aplicación
        servicios.AddSingleton<MazosServicio>();

        // Presentación
        servicios.AddTransient<MazosViewModel>();
        servicios.AddTransient<MazoViewModel>();
        servicios.AddTransient<MazoEditorViewModel>();
        servicios.AddTransient<TarjetaEditorViewModel>();
        servicios.AddTransient<RepasoViewModel>();

        servicios.AddTransient<MazosPage>();
        servicios.AddTransient<MazoPage>();
        servicios.AddTransient<MazoEditorPage>();
        servicios.AddTransient<TarjetaEditorPage>();
        servicios.AddTransient<RepasoPage>();

        return builder.Build();
    }
}
