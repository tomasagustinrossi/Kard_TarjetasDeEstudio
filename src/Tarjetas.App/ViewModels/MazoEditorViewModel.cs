using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.App.ViewModels;

public partial class MazoEditorViewModel(MazosServicio servicio) : ObservableObject, IQueryAttributable
{
    private Guid? mazoId;

    public ObservableCollection<ColorOpcion> Colores { get; } = CrearColores();

    [ObservableProperty] private string nombre = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TituloPagina))]
    private bool esEdicion;

    public string TituloPagina => EsEdicion ? "Editar mazo" : "Nuevo mazo";

    private ColorMazo ColorElegido => Colores.First(c => c.Seleccionado).Valor;

    private static ObservableCollection<ColorOpcion> CrearColores()
    {
        var colores = new ObservableCollection<ColorOpcion>(PaletaKeep.Todos.Select(c => new ColorOpcion(c)));
        colores[0].Seleccionado = true; // Predeterminado
        return colores;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mazoId", out var valor) && Guid.TryParse(Convert.ToString(valor), out var id))
        {
            mazoId = id;
            EsEdicion = true;
            _ = CargarAsync(id);
        }
    }

    private async Task CargarAsync(Guid id)
    {
        var mazo = await servicio.ObtenerAsync(id);
        if (mazo is null) return;
        Nombre = mazo.Nombre;
        Seleccionar(mazo.Color);
    }

    private void Seleccionar(ColorMazo color)
    {
        foreach (var opcion in Colores) opcion.Seleccionado = opcion.Valor == color;
    }

    [RelayCommand]
    private void SeleccionarColor(ColorOpcion elegido) => Seleccionar(elegido.Valor);

    [RelayCommand]
    private async Task GuardarAsync()
    {
        try
        {
            if (mazoId is { } id)
            {
                await servicio.EditarMazoAsync(id, Nombre, ColorElegido);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var nuevoId = await servicio.CrearMazoAsync(Nombre, ColorElegido);
                await Shell.Current.GoToAsync("..");
                await Shell.Current.GoToAsync($"{nameof(MazoPage)}?id={nuevoId}");
            }
        }
        catch (ExcepcionDeDominio ex)
        {
            await Shell.Current.DisplayAlertAsync("Revisá el mazo", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private Task CancelarAsync() => Shell.Current.GoToAsync("..");
}
