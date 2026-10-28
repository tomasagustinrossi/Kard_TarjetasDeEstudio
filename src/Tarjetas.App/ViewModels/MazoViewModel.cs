using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.Aplicacion;

namespace Tarjetas.App.ViewModels;

public partial class MazoViewModel(MazosServicio servicio) : ObservableObject, IQueryAttributable
{
    private Guid mazoId;
    private MazoVista? mazo;

    public ObservableCollection<TarjetaItem> Tarjetas { get; } = [];

    [ObservableProperty] private string nombre = "";
    [ObservableProperty] private string resumen = "";
    [ObservableProperty] private string textoRepasar = "";
    [ObservableProperty] private bool hayPendientes;

    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        mazoId = Guid.Parse(Convert.ToString(query["id"])!);

    [RelayCommand]
    private async Task CargarAsync()
    {
        mazo = await servicio.ObtenerAsync(mazoId);
        if (mazo is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }
        Refrescar();
    }

    public void Refrescar()
    {
        if (mazo is null) return;
        Nombre = mazo.Nombre;
        HayPendientes = mazo.Pendientes > 0;
        TextoRepasar = HayPendientes ? $"Repasar · {mazo.Pendientes}" : "Al día ✓";
        Resumen = mazo.Tarjetas.Count == 1 ? "1 tarjeta" : $"{mazo.Tarjetas.Count} tarjetas";

        Tarjetas.Clear();
        foreach (var tarjeta in mazo.Tarjetas) Tarjetas.Add(TarjetaItem.De(tarjeta, mazo.Color));
    }

    [RelayCommand]
    private Task NuevaTarjetaAsync() => Shell.Current.GoToAsync($"{nameof(TarjetaEditorPage)}?mazoId={mazoId}");

    [RelayCommand]
    private Task EditarTarjetaAsync(Guid id) =>
        Shell.Current.GoToAsync($"{nameof(TarjetaEditorPage)}?mazoId={mazoId}&tarjetaId={id}");

    [RelayCommand]
    private Task RepasarAsync() => Shell.Current.GoToAsync($"{nameof(RepasoPage)}?mazoId={mazoId}");

    [RelayCommand]
    private Task EditarMazoAsync() => Shell.Current.GoToAsync($"{nameof(MazoEditorPage)}?mazoId={mazoId}");

    [RelayCommand]
    private async Task EliminarMazoAsync()
    {
        var confirmado = await Shell.Current.DisplayAlertAsync(
            "Eliminar mazo", "Se borrarán el mazo y todas sus tarjetas. No se puede deshacer.", "Eliminar", "Cancelar");
        if (!confirmado) return;
        await servicio.EliminarMazoAsync(mazoId);
        await Shell.Current.GoToAsync("..");
    }
}
