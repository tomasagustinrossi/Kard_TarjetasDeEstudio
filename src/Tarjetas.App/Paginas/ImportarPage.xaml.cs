using Tarjetas.App.ViewModels;

namespace Tarjetas.App.Paginas;

public partial class ImportarPage : ContentPage
{
    public ImportarPage(ImportarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
