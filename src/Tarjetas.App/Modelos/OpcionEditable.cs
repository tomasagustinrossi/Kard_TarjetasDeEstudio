using CommunityToolkit.Mvvm.ComponentModel;

namespace Tarjetas.App.Modelos;

/// <summary>Una fila de opción en el creador de tarjetas (texto + si es correcta).</summary>
public partial class OpcionEditable : ObservableObject
{
    private readonly Action<OpcionEditable> alMarcar;

    public OpcionEditable(string textoInicial, bool correctaInicial, Action<OpcionEditable> alMarcar)
    {
        texto = textoInicial;
        esCorrecta = correctaInicial;
        this.alMarcar = alMarcar;
    }

    [ObservableProperty] private string texto;
    [ObservableProperty] private bool esCorrecta;

    partial void OnEsCorrectaChanged(bool value)
    {
        if (value) alMarcar(this);
    }
}
