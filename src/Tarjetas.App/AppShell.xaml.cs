using Tarjetas.App.Paginas;

namespace Tarjetas.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(MazoPage), typeof(MazoPage));
        Routing.RegisterRoute(nameof(MazoEditorPage), typeof(MazoEditorPage));
        Routing.RegisterRoute(nameof(TarjetaEditorPage), typeof(TarjetaEditorPage));
        Routing.RegisterRoute(nameof(RepasoPage), typeof(RepasoPage));
        Routing.RegisterRoute(nameof(RecompensasPage), typeof(RecompensasPage));
        Routing.RegisterRoute(nameof(ImportarPage), typeof(ImportarPage));
    }
}
