using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.App.ViewModels;

public partial class TarjetaEditorViewModel(MazosServicio servicio) : ObservableObject, IQueryAttributable
{
    private Guid mazoId;

    public ObservableCollection<OpcionEditable> Opciones { get; } = [];

    /// <summary>Se dispara tras "Guardar y crear otra" para que la página vuelva a enfocar la pregunta.</summary>
    public event Action? Reiniciado;

    [ObservableProperty] private string pregunta = "";
    [ObservableProperty] private string respuesta = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HayAviso))]
    private string aviso = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion), nameof(EsNueva), nameof(TituloPagina))]
    private Guid? tarjetaId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEscrita), nameof(EsUnica), nameof(EsMultiple), nameof(UsaOpciones),
        nameof(AyudaOpciones), nameof(RotuloRespuesta))]
    private TipoRespuesta tipo = TipoRespuesta.Escrita;

    public bool EsEscrita => Tipo == TipoRespuesta.Escrita;
    public bool EsUnica => Tipo == TipoRespuesta.OpcionUnica;
    public bool EsMultiple => Tipo == TipoRespuesta.OpcionMultiple;
    public bool UsaOpciones => !EsEscrita;
    public bool EsEdicion => TarjetaId is not null;
    public bool EsNueva => !EsEdicion;
    public bool HayAviso => !string.IsNullOrEmpty(Aviso);
    public string TituloPagina => EsEdicion ? "Editar tarjeta" : "Nueva tarjeta";
    public string RotuloRespuesta => EsEscrita ? "RESPUESTA" : "EXPLICACIÓN (OPCIONAL)";
    public string AyudaOpciones => EsUnica ? "Marcá la única respuesta correcta" : "Marcá todas las respuestas correctas";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        mazoId = Guid.Parse(Convert.ToString(query["mazoId"])!);
        if (query.TryGetValue("tarjetaId", out var valor) && Guid.TryParse(Convert.ToString(valor), out var id))
        {
            TarjetaId = id;
            _ = CargarAsync(id);
        }
    }

    private async Task CargarAsync(Guid id)
    {
        var mazo = await servicio.ObtenerAsync(mazoId);
        var tarjeta = mazo?.Tarjetas.FirstOrDefault(t => t.Id == id);
        if (tarjeta is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        Pregunta = tarjeta.Pregunta;
        Respuesta = tarjeta.Respuesta;
        Opciones.Clear();
        foreach (var o in tarjeta.Opciones) Opciones.Add(NuevaOpcion(o.Texto, o.EsCorrecta, o.Justificacion));
        Tipo = tarjeta.Tipo;
    }

    partial void OnTipoChanged(TipoRespuesta value)
    {
        if (value == TipoRespuesta.Escrita) return;
        while (Opciones.Count < Tarjeta.MinimoOpciones) Opciones.Add(NuevaOpcion());
        if (value == TipoRespuesta.OpcionUnica)
            foreach (var sobrante in Opciones.Where(o => o.EsCorrecta).Skip(1).ToList()) sobrante.EsCorrecta = false;
    }

    private OpcionEditable NuevaOpcion(string texto = "", bool correcta = false, string justificacion = "") =>
        new(texto, correcta, justificacion, AlMarcar);

    // En "opción única" marcar una desmarca las demás.
    private void AlMarcar(OpcionEditable marcada)
    {
        if (Tipo != TipoRespuesta.OpcionUnica) return;
        foreach (var otra in Opciones.Where(o => o != marcada && o.EsCorrecta).ToList()) otra.EsCorrecta = false;
    }

    [RelayCommand] private void ElegirEscrita() => Tipo = TipoRespuesta.Escrita;
    [RelayCommand] private void ElegirUnica() => Tipo = TipoRespuesta.OpcionUnica;
    [RelayCommand] private void ElegirMultiple() => Tipo = TipoRespuesta.OpcionMultiple;

    [RelayCommand]
    private void AgregarOpcion()
    {
        if (Opciones.Count < Tarjeta.MaximoOpciones) Opciones.Add(NuevaOpcion());
    }

    [RelayCommand]
    private void QuitarOpcion(OpcionEditable opcion)
    {
        if (Opciones.Count > Tarjeta.MinimoOpciones) Opciones.Remove(opcion);
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (await IntentarGuardarAsync()) await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GuardarYCrearOtraAsync()
    {
        if (!await IntentarGuardarAsync()) return;

        Pregunta = "";
        Respuesta = "";
        foreach (var o in Opciones)
        {
            o.Texto = "";
            o.EsCorrecta = false;
            o.Justificacion = "";
        }
        Reiniciado?.Invoke();
        _ = MostrarAvisoAsync("Tarjeta guardada ✓");
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (TarjetaId is not { } id) return;
        var confirmado = await Shell.Current.DisplayAlertAsync(
            "Eliminar tarjeta", "Esta acción no se puede deshacer.", "Eliminar", "Cancelar");
        if (!confirmado) return;
        await servicio.EliminarTarjetaAsync(mazoId, id);
        await Shell.Current.GoToAsync("..");
    }

    private async Task<bool> IntentarGuardarAsync()
    {
        try
        {
            var datos = ArmarDatos();
            if (TarjetaId is { } id) await servicio.EditarTarjetaAsync(mazoId, id, datos);
            else await servicio.AgregarTarjetaAsync(mazoId, datos);
            return true;
        }
        catch (ExcepcionDeDominio ex)
        {
            await Shell.Current.DisplayAlertAsync("Revisá la tarjeta", ex.Message, "OK");
            return false;
        }
    }

    private TarjetaDatos ArmarDatos()
    {
        var opciones = EsEscrita
            ? null
            : Opciones.Where(o => !string.IsNullOrWhiteSpace(o.Texto))
                      .Select(o => new OpcionDatos(o.Texto, o.EsCorrecta, o.Justificacion))
                      .ToList();
        return new TarjetaDatos(Tipo, Pregunta, Respuesta, opciones);
    }

    private async Task MostrarAvisoAsync(string texto)
    {
        Aviso = texto;
        await Task.Delay(1800);
        if (Aviso == texto) Aviso = "";
    }
}
