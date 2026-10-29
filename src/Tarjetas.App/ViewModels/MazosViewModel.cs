using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.Aplicacion;

namespace Tarjetas.App.ViewModels;

public partial class MazosViewModel(MazosServicio servicio) : ObservableObject
{
    private IReadOnlyList<MazoResumen> todos = [];

    public ObservableCollection<MazoItem> Mazos { get; } = [];

    [ObservableProperty] private string busqueda = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonEdicion))]
    private bool modoEdicion;

    public string TextoBotonEdicion => ModoEdicion ? "✓" : "✏️";

    partial void OnBusquedaChanged(string value) => Refrescar();

    partial void OnModoEdicionChanged(bool value)
    {
        foreach (var mazo in Mazos) mazo.EnEdicion = value;
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        todos = await servicio.ListarAsync();
        Refrescar();
    }

    [RelayCommand]
    private void AlternarEdicion() => ModoEdicion = !ModoEdicion;

    /// <summary>Tocar un mazo empieza el repaso; en modo edición abre sus tarjetas. Un mazo vacío abre siempre sus tarjetas.</summary>
    [RelayCommand]
    private Task TocarMazoAsync(MazoItem mazo) =>
        ModoEdicion || mazo.Total == 0
            ? Shell.Current.GoToAsync($"{nameof(MazoPage)}?id={mazo.Id}")
            : Shell.Current.GoToAsync($"{nameof(RepasoPage)}?mazoId={mazo.Id}");

    [RelayCommand]
    private Task EditarMazoAsync(Guid id) => Shell.Current.GoToAsync($"{nameof(MazoEditorPage)}?mazoId={id}");

    [RelayCommand]
    private Task NuevoMazoAsync() => Shell.Current.GoToAsync(nameof(MazoEditorPage));

    [RelayCommand]
    private Task AbrirRecompensasAsync() => Shell.Current.GoToAsync(nameof(RecompensasPage));

    [RelayCommand]
    private Task AbrirImportarAsync() => Shell.Current.GoToAsync(nameof(ImportarPage));

    public void Refrescar()
    {
        var filtro = Busqueda.Trim();
        var visibles = filtro.Length == 0
            ? todos
            : todos.Where(m => m.Nombre.Contains(filtro, StringComparison.CurrentCultureIgnoreCase)).ToList();

        Mazos.Clear();
        foreach (var mazo in visibles) Mazos.Add(MazoItem.De(mazo, ModoEdicion));
    }
}
