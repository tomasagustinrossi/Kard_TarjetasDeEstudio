using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.Aplicacion;

namespace Tarjetas.App.ViewModels;

public partial class RecompensasViewModel(RecompensasServicio servicio) : ObservableObject
{
    public ObservableCollection<RecompensaItem> Mazos { get; } = [];

    [ObservableProperty] private string total = "";

    [RelayCommand]
    private async Task CargarAsync()
    {
        var lista = await servicio.ListarAsync();
        Mazos.Clear();
        foreach (var recompensa in lista) Mazos.Add(RecompensaItem.De(recompensa));
        Total = $"{lista.Sum(r => r.Puntos)} puntos en total";
    }
}
