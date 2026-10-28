namespace Tarjetas.App.Paginas;

internal static class Rejilla
{
    /// <summary>Más ancho de ventana (Mac) = más columnas, como en Google Keep.</summary>
    public static void Ajustar(CollectionView lista, double anchoDisponible, double anchoMinimoDeCuadro = 220)
    {
        if (lista.ItemsLayout is GridItemsLayout rejilla)
            rejilla.Span = Math.Max(1, (int)(anchoDisponible / anchoMinimoDeCuadro));
    }
}
