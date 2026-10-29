using CommunityToolkit.Mvvm.ComponentModel;

namespace Tarjetas.App.Modelos;

/// <summary>Una fila de opción en el creador de tarjetas: texto, si es correcta y una justificación opcional.</summary>
public partial class OpcionEditable : ObservableObject
{
    private readonly Action<OpcionEditable> alMarcar;

    public OpcionEditable(string textoInicial, bool correctaInicial, string justificacionInicial, Action<OpcionEditable> alMarcar)
    {
        texto = textoInicial;
        esCorrecta = correctaInicial;
        justificacion = justificacionInicial;
        this.alMarcar = alMarcar;
    }

    [ObservableProperty] private string texto;
    [ObservableProperty] private bool esCorrecta;
    [ObservableProperty] private string justificacion;

    partial void OnEsCorrectaChanged(bool value)
    {
        if (value) alMarcar(this);
    }
}
