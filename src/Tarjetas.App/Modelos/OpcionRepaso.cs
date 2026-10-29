using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tarjetas.App.Modelos;

/// <summary>Una opción durante el repaso: se elige y, tras comprobar, muestra si era correcta y su justificación.</summary>
public partial class OpcionRepaso : ObservableObject
{
    private readonly Action<OpcionRepaso> alElegir;

    public OpcionRepaso(int indice, string texto, bool esCorrecta, string justificacion, Action<OpcionRepaso> alElegir)
    {
        Indice = indice;
        Texto = texto;
        EsCorrecta = esCorrecta;
        Justificacion = justificacion;
        this.alElegir = alElegir;
        fondo = PaletaKeep.Superficie;
    }

    public int Indice { get; }
    public string Texto { get; }
    public bool EsCorrecta { get; }
    public string Justificacion { get; }

    [ObservableProperty] private bool elegida;
    [ObservableProperty] private Color fondo;
    [ObservableProperty] private string marca = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarJustificacion))]
    private bool bloqueada;

    public bool MostrarJustificacion => Bloqueada && Justificacion.Length > 0;

    partial void OnElegidaChanged(bool value)
    {
        if (value) alElegir(this);
    }

    [RelayCommand]
    private void Alternar()
    {
        if (!Bloqueada) Elegida = !Elegida;
    }

    public void MostrarResultado()
    {
        Bloqueada = true;
        if (EsCorrecta)
        {
            Fondo = PaletaKeep.FondoAcierto;
            Marca = "✓";
        }
        else if (Elegida)
        {
            Fondo = PaletaKeep.FondoError;
            Marca = "✗";
        }
    }
}
