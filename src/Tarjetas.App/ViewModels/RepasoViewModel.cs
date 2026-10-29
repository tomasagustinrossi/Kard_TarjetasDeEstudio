using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.App.ViewModels;

public enum FaseRepaso { Cargando, Pregunta, Revelada, Comprobada, Terminado }

public partial class RepasoViewModel(MazosServicio servicio, RecompensasServicio recompensas)
    : ObservableObject, IQueryAttributable
{
    private Guid mazoId;
    private List<TarjetaVista> cola = [];
    private int indice;
    private int aciertos;
    private bool aciertoActual;

    public ObservableCollection<OpcionRepaso> Opciones { get; } = [];

    /// <summary>La página los usa para animar la entrada de cada tarjeta y la aparición de la corrección.</summary>
    public event Action? TarjetaMostrada;
    public event Action? RespuestaRevelada;

    [ObservableProperty] private string progreso = "";
    [ObservableProperty] private string pregunta = "";
    [ObservableProperty] private string respuesta = "";
    [ObservableProperty] private string respuestaEscrita = "";
    [ObservableProperty] private string resultado = "";
    [ObservableProperty] private string textoCorreccion = "";
    [ObservableProperty] private string resumenFinal = "";
    [ObservableProperty] private string puntosTexto = "";
    [ObservableProperty] private bool sinPendientes;
    [ObservableProperty] private bool esEscrita = true;
    [ObservableProperty] private Color fondoTarjeta = Colors.Transparent;
    [ObservableProperty] private Color colorResultado = Colors.Gray;
    [ObservableProperty] private FaseRepaso fase = FaseRepaso.Cargando;

    // Cualquier cambio de fase refresca todas las propiedades calculadas de abajo.
    partial void OnFaseChanged(FaseRepaso value) => OnPropertyChanged(string.Empty);

    public bool EnCurso => Fase is FaseRepaso.Pregunta or FaseRepaso.Revelada or FaseRepaso.Comprobada;
    public bool Terminado => Fase == FaseRepaso.Terminado;
    public bool TieneVolver => Terminado && !SinPendientes;
    public bool MostrarEntradaEscrita => Fase == FaseRepaso.Pregunta && EsEscrita;
    public bool MostrarVerRespuesta => Fase == FaseRepaso.Pregunta && EsEscrita;
    public bool MostrarRespuestaEscrita => Fase == FaseRepaso.Revelada && EsEscrita;
    public bool MostrarOpciones => EnCurso && !EsEscrita;
    public bool MostrarComprobar => Fase == FaseRepaso.Pregunta;
    public bool MostrarContinuar => Fase == FaseRepaso.Comprobada;
    public bool MostrarForzar => Fase == FaseRepaso.Comprobada && EsEscrita && !aciertoActual;
    public bool MostrarCorreccion => Fase == FaseRepaso.Comprobada && !string.IsNullOrEmpty(TextoCorreccion);
    public bool MostrarPuntos => Terminado && !string.IsNullOrEmpty(PuntosTexto);

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
        await MostrarAsync();
    }

    private async Task MostrarAsync()
    {
        if (indice >= cola.Count)
        {
            await TerminarAsync();
            return;
        }

        var tarjeta = cola[indice];
        EsEscrita = tarjeta.Tipo == TipoRespuesta.Escrita;
        Pregunta = tarjeta.Pregunta;
        Respuesta = tarjeta.Respuesta;
        RespuestaEscrita = "";
        TextoCorreccion = "";
        Resultado = "";
        aciertoActual = false;
        Progreso = $"{indice + 1} / {cola.Count}";

        Opciones.Clear();
        for (var i = 0; i < tarjeta.Opciones.Count; i++)
        {
            var o = tarjeta.Opciones[i];
            Opciones.Add(new OpcionRepaso(i, o.Texto, o.EsCorrecta, o.Justificacion, AlElegir));
        }

        Fase = FaseRepaso.Pregunta;
        OnPropertyChanged(string.Empty);
        TarjetaMostrada?.Invoke();
    }

    private async Task TerminarAsync()
    {
        if (cola.Count == 0)
        {
            SinPendientes = true;
            ResumenFinal = "No tenés tarjetas para repasar hoy.";
            PuntosTexto = "";
        }
        else
        {
            var puntos = await recompensas.RegistrarExamenAsync(mazoId, cola.Count, aciertos);
            ResumenFinal = $"Repasaste {cola.Count} {(cola.Count == 1 ? "tarjeta" : "tarjetas")} · " +
                           $"{aciertos} {(aciertos == 1 ? "acierto" : "aciertos")}";
            PuntosTexto = puntos.Puntos > 0
                ? $"🏆 +{puntos.Puntos} puntos para este mazo"
                : "Con un 70 % de aciertos o más ganás puntos";
        }
        Fase = FaseRepaso.Terminado;
        OnPropertyChanged(string.Empty);
    }

    // En "opción única" elegir una desmarca las demás.
    private void AlElegir(OpcionRepaso elegida)
    {
        if (cola[indice].Tipo != TipoRespuesta.OpcionUnica) return;
        foreach (var otra in Opciones.Where(o => o != elegida && o.Elegida).ToList()) otra.Elegida = false;
    }

    /// <summary>Escrita: autoevaluación (ver la respuesta y decir si la recordaste).</summary>
    [RelayCommand]
    private void MostrarRespuesta()
    {
        Fase = FaseRepaso.Revelada;
        RespuestaRevelada?.Invoke();
    }

    [RelayCommand]
    private async Task ComprobarAsync()
    {
        var tarjeta = cola[indice];
        if (EsEscrita)
        {
            if (string.IsNullOrWhiteSpace(RespuestaEscrita)) return;
            aciertoActual = await servicio.ComprobarEscritaAsync(mazoId, tarjeta.Id, RespuestaEscrita);
            TextoCorreccion = aciertoActual ? "" : $"Respuesta correcta: {tarjeta.Respuesta.Replace("|", " / ")}";
        }
        else
        {
            var elegidas = Opciones.Where(o => o.Elegida).Select(o => o.Indice).ToList();
            aciertoActual = await servicio.ComprobarAsync(mazoId, tarjeta.Id, elegidas);
            foreach (var opcion in Opciones) opcion.MostrarResultado();
            TextoCorreccion = tarjeta.Respuesta; // explicación opcional de la tarjeta
        }

        MostrarVeredicto();
        Fase = FaseRepaso.Comprobada;
        OnPropertyChanged(string.Empty);
        RespuestaRevelada?.Invoke();
    }

    /// <summary>Por si el usuario considera correcta una respuesta escrita que la app marcó mal (sinónimos, etc.).</summary>
    [RelayCommand]
    private void ContarComoCorrecta()
    {
        aciertoActual = true;
        TextoCorreccion = "";
        MostrarVeredicto();
        OnPropertyChanged(string.Empty);
    }

    private void MostrarVeredicto()
    {
        Resultado = aciertoActual ? "¡Correcto!" : "Incorrecto";
        ColorResultado = aciertoActual ? PaletaKeep.TextoAcierto : PaletaKeep.TextoError;
    }

    [RelayCommand] private Task RecordadaAsync() => AvanzarAsync(recordada: true);
    [RelayCommand] private Task OlvidadaAsync() => AvanzarAsync(recordada: false);
    [RelayCommand] private Task ContinuarAsync() => AvanzarAsync(aciertoActual);

    private async Task AvanzarAsync(bool recordada)
    {
        await servicio.RepasarTarjetaAsync(mazoId, cola[indice].Id, recordada);
        if (recordada) aciertos++;
        indice++;
        await MostrarAsync();
    }

    [RelayCommand]
    private Task SalirAsync() => Shell.Current.GoToAsync("..");

    /// <summary>Si no había nada para repasar, deja ver y editar las tarjetas del mazo.</summary>
    [RelayCommand]
    private async Task VerTarjetasAsync()
    {
        await Shell.Current.GoToAsync("..");
        await Shell.Current.GoToAsync($"{nameof(MazoPage)}?id={mazoId}");
    }
}
