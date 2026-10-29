using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.App.ViewModels;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Dominio.Recompensas;
using Tarjetas.Infraestructura;
using Tarjetas.Infraestructura.Intercambio;

namespace Tarjetas.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        var servicios = builder.Services;

        // Infraestructura: un archivo liviano por mazo + un archivo pequeño de recompensas
        var carpetaDatos = FileSystem.AppDataDirectory;
        servicios.AddSingleton(new RutasDeDatos(carpetaDatos));
        servicios.AddSingleton<IMazoRepositorio>(_ => new RepositorioDeMazosJson(Path.Combine(carpetaDatos, "mazos")));
        servicios.AddSingleton<IRecompensasRepositorio>(_ =>
            new RepositorioDeRecompensasJson(Path.Combine(carpetaDatos, "recompensas.json")));
        servicios.AddSingleton(TimeProvider.System);

        // Formatos de intercambio (el orden importa: el de texto libre va al final)
        servicios.AddSingleton<IFormatoDeIntercambio, FormatoNativo>();
        servicios.AddSingleton<IFormatoDeIntercambio, FormatoCsv>();
        servicios.AddSingleton<IFormatoDeIntercambio, FormatoTexto>();

        // Aplicación
        servicios.AddSingleton<MazosServicio>();
        servicios.AddSingleton<RecompensasServicio>();
        servicios.AddSingleton<IntercambioServicio>();

        // Presentación
        servicios.AddTransient<MazosViewModel>();
        servicios.AddTransient<MazoViewModel>();
        servicios.AddTransient<MazoEditorViewModel>();
        servicios.AddTransient<TarjetaEditorViewModel>();
        servicios.AddTransient<RepasoViewModel>();
        servicios.AddTransient<RecompensasViewModel>();
        servicios.AddTransient<ImportarViewModel>();

        servicios.AddTransient<MazosPage>();
        servicios.AddTransient<MazoPage>();
        servicios.AddTransient<MazoEditorPage>();
        servicios.AddTransient<TarjetaEditorPage>();
        servicios.AddTransient<RepasoPage>();
        servicios.AddTransient<RecompensasPage>();
        servicios.AddTransient<ImportarPage>();

        return builder.Build();
    }
}
