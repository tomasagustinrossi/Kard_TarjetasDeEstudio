namespace Tarjetas.App;

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new AppShell()) { Title = "Tarjetas", Width = 960, Height = 720, MinimumWidth = 420, MinimumHeight = 520 };
}
