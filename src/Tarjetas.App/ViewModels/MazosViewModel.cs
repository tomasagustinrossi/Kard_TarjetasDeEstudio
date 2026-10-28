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

    partial void OnBusquedaChanged(string value) => Refrescar();

    [RelayCommand]
    private async Task CargarAsync()
    {
        todos = await servicio.ListarAsync();
        Refrescar();
    }

    [RelayCommand]
    private Task NuevoMazoAsync() => Shell.Current.GoToAsync(nameof(MazoEditorPage));

    [RelayCommand]
    private Task AbrirAsync(Guid id) => Shell.Current.GoToAsync($"{nameof(MazoPage)}?id={id}");

    public void Refrescar()
    {
        var filtro = Busqueda.Trim();
        var visibles = filtro.Length == 0
            ? todos
            : todos.Where(m => m.Nombre.Contains(filtro, StringComparison.CurrentCultureIgnoreCase)).ToList();

        Mazos.Clear();
        foreach (var mazo in visibles) Mazos.Add(MazoItem.De(mazo));
    }
}
