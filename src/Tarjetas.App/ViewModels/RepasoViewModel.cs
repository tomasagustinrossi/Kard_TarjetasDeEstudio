using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.App.ViewModels;

public enum FaseRepaso { Cargando, Pregunta, Revelada, Comprobada, Terminado }

public partial class RepasoViewModel(MazosServicio servicio) : ObservableObject, IQueryAttributable
{
    private Guid mazoId;
    private List<TarjetaVista> cola = [];
    private int indice;
    private int aciertos;
    private bool aciertoActual;

    public ObservableCollection<OpcionRepaso> Opciones { get; } = [];

    /// <summary>La página los usa para animar la entrada de cada tarjeta y la aparición de la respuesta.</summary>
    public event Action? TarjetaMostrada;
    public event Action? RespuestaRevelada;

    [ObservableProperty] private string progreso = "";
    [ObservableProperty] private string pregunta = "";
    [ObservableProperty] private string respuesta = "";
    [ObservableProperty] private string resultado = "";
    [ObservableProperty] private string resumenFinal = "";
    [ObservableProperty] private bool esEscrita = true;
    [ObservableProperty] private Color fondoTarjeta = Colors.Transparent;
    [ObservableProperty] private Color colorResultado = Colors.Gray;
    [ObservableProperty] private FaseRepaso fase = FaseRepaso.Cargando;

    // Cualquier cambio de fase refresca todas las propiedades calculadas de abajo.
    partial void OnFaseChanged(FaseRepaso value) => OnPropertyChanged(string.Empty);

    public bool EnCurso => Fase is FaseRepaso.Pregunta or FaseRepaso.Revelada or FaseRepaso.Comprobada;
    public bool Terminado => Fase == FaseRepaso.Terminado;
    public bool MostrarTocar => Fase == FaseRepaso.Pregunta && EsEscrita;
    public bool MostrarRespuestaEscrita => Fase == FaseRepaso.Revelada && EsEscrita;
    public bool MostrarOpciones => EnCurso && !EsEscrita;
    public bool MostrarComprobar => Fase == FaseRepaso.Pregunta && !EsEscrita;
    public bool MostrarContinuar => Fase == FaseRepaso.Comprobada;
    public bool MostrarExplicacion => Fase == FaseRepaso.Comprobada && !string.IsNullOrEmpty(Respuesta);

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        mazoId = Guid.Parse(Convert.ToString(query["mazoId"])!);
        _ = IniciarAsync();
    }

    private async Task IniciarAsync()
    {
        var sesion = await servicio.IniciarRepasoAsync(mazoId);
        if (sesion is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }
        cola = [.. sesion.Tarjetas];
        FondoTarjeta = PaletaKeep.Fondo(sesion.Color);
        Mostrar();
    }

    private void Mostrar()
    {
        if (indice >= cola.Count)
        {
            ResumenFinal = cola.Count == 0
                ? "No tenés tarjetas para repasar hoy."
                : $"Repasaste {cola.Count} {(cola.Count == 1 ? "tarjeta" : "tarjetas")} · {aciertos} {(aciertos == 1 ? "acierto" : "aciertos")}";
            Fase = FaseRepaso.Terminado;
            return;
        }

        var tarjeta = cola[indice];
        EsEscrita = tarjeta.Tipo == TipoRespuesta.Escrita;
        Pregunta = tarjeta.Pregunta;
        Respuesta = tarjeta.Respuesta;
        Progreso = $"{indice + 1} / {cola.Count}";
        Resultado = "";

        Opciones.Clear();
        for (var i = 0; i < tarjeta.Opciones.Count; i++)
            Opciones.Add(new OpcionRepaso(i, tarjeta.Opciones[i].Texto, tarjeta.Opciones[i].EsCorrecta, AlElegir));

        Fase = FaseRepaso.Pregunta;
        OnPropertyChanged(string.Empty);
        TarjetaMostrada?.Invoke();
    }

    // En "opción única" elegir una desmarca las demás.
    private void AlElegir(OpcionRepaso elegida)
    {
        if (cola[indice].Tipo != TipoRespuesta.OpcionUnica) return;
        foreach (var otra in Opciones.Where(o => o != elegida && o.Elegida).ToList()) otra.Elegida = false;
    }

    [RelayCommand]
    private void MostrarRespuesta()
    {
        Fase = FaseRepaso.Revelada;
        RespuestaRevelada?.Invoke();
    }

    [RelayCommand]
    private async Task ComprobarAsync()
    {
        var elegidas = Opciones.Where(o => o.Elegida).Select(o => o.Indice).ToList();
        aciertoActual = await servicio.ComprobarAsync(mazoId, cola[indice].Id, elegidas);
        foreach (var opcion in Opciones) opcion.MostrarResultado();

        Resultado = aciertoActual ? "¡Correcto!" : "Incorrecto";
        ColorResultado = aciertoActual ? PaletaKeep.TextoAcierto : PaletaKeep.TextoError;
        Fase = FaseRepaso.Comprobada;
        RespuestaRevelada?.Invoke();
    }

    [RelayCommand] private Task RecordadaAsync() => AvanzarAsync(recordada: true);
    [RelayCommand] private Task OlvidadaAsync() => AvanzarAsync(recordada: false);
    [RelayCommand] private Task ContinuarAsync() => AvanzarAsync(aciertoActual);

    [RelayCommand]
    private Task SalirAsync() => Shell.Current.GoToAsync("..");

    private async Task AvanzarAsync(bool recordada)
    {
        await servicio.RepasarTarjetaAsync(mazoId, cola[indice].Id, recordada);
        if (recordada) aciertos++;
        indice++;
        Mostrar();
    }
}
