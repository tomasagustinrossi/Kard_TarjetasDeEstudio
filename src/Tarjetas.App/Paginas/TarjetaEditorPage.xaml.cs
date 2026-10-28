using Tarjetas.App.ViewModels;

namespace Tarjetas.App.Paginas;

public partial class TarjetaEditorPage : ContentPage
{
    public TarjetaEditorPage(TarjetaEditorViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.Reiniciado += () => EditorPregunta.Focus();
    }
}
