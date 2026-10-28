using CommunityToolkit.Mvvm.ComponentModel;
using Tarjetas.Dominio;

namespace Tarjetas.App.Modelos;

/// <summary>Una muestra de color en el selector de colores del mazo.</summary>
public partial class ColorOpcion : ObservableObject
{
    public ColorOpcion(ColorMazo valor)
    {
        Valor = valor;
        Fondo = PaletaKeep.Fondo(valor);
    }

    public ColorMazo Valor { get; }
    public Color Fondo { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Borde))]
    private bool seleccionado;

    public Brush Borde => PaletaKeep.Pincel(Seleccionado ? PaletaKeep.Seleccion : PaletaKeep.Borde(Valor));
}
