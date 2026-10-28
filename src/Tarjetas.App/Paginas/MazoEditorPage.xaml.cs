using Tarjetas.App.ViewModels;

namespace Tarjetas.App.Paginas;

public partial class MazoEditorPage : ContentPage
{
    public MazoEditorPage(MazoEditorViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
