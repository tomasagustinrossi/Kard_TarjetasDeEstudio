using Tarjetas.Dominio.Comun;
using Tarjetas.Dominio.Repasos;

namespace Tarjetas.Dominio;

/// <summary>
/// Entidad dentro del agregado <see cref="Mazo"/>. Una pregunta con su respuesta (escrita u opciones)
/// y su programa de repaso. Protege sus propias reglas.
/// </summary>
public sealed class Tarjeta : Entidad
{
    public const int LargoMaximoPregunta = 1000;
    public const int LargoMaximoRespuesta = 2000;
    public const int LargoMaximoOpcion = 300;
    public const int MinimoOpciones = 2;
    public const int MaximoOpciones = 6;

    public TipoRespuesta Tipo { get; private set; }
    public string Pregunta { get; private set; }

    /// <summary>En tarjetas escritas es la respuesta; en las de elección es una explicación opcional.</summary>
    public string Respuesta { get; private set; }

    public IReadOnlyList<Opcion> Opciones { get; private set; }
    public DateTime CreadaUtc { get; }
    public ProgramaDeRepaso Repaso { get; private set; }

    private Tarjeta(Guid id, TipoRespuesta tipo, string pregunta, string respuesta,
        IReadOnlyList<Opcion> opciones, DateTime creadaUtc, ProgramaDeRepaso repaso)
    {
        Id = id;
        Tipo = tipo;
        Pregunta = pregunta;
        Respuesta = respuesta;
        Opciones = opciones;
        CreadaUtc = creadaUtc;
        Repaso = repaso;
    }

    public static Tarjeta Crear(TipoRespuesta tipo, string pregunta, string respuesta,
        IEnumerable<Opcion>? opciones, DateTime ahoraUtc)
    {
        var c = Normalizar(tipo, pregunta, respuesta, opciones);
        return new Tarjeta(Guid.NewGuid(), tipo, c.Pregunta, c.Respuesta, c.Opciones, ahoraUtc, ProgramaDeRepaso.Nuevo);
    }

    /// <summary>Atajo para el caso más común (y el de Gemini): pregunta + respuesta escrita.</summary>
    public static Tarjeta CrearEscrita(string pregunta, string respuesta, DateTime ahoraUtc) =>
        Crear(TipoRespuesta.Escrita, pregunta, respuesta, null, ahoraUtc);

    /// <summary>Reconstruye una tarjeta ya guardada (la usa la infraestructura; no revalida).</summary>
    public static Tarjeta Rehidratar(Guid id, TipoRespuesta tipo, string pregunta, string respuesta,
        IReadOnlyList<Opcion> opciones, DateTime creadaUtc, ProgramaDeRepaso repaso) =>
        new(id, tipo, pregunta, respuesta, opciones, creadaUtc, repaso);

    public void Editar(TipoRespuesta tipo, string pregunta, string respuesta, IEnumerable<Opcion>? opciones)
    {
        var c = Normalizar(tipo, pregunta, respuesta, opciones); // valida todo antes de tocar nada
        Tipo = tipo;
        Pregunta = c.Pregunta;
        Respuesta = c.Respuesta;
        Opciones = c.Opciones;
    }

    public void MarcarComoRecordada(DateOnly hoy) => Repaso = Repaso.Recordada(hoy);
    public void MarcarComoOlvidada(DateOnly hoy) => Repaso = Repaso.Olvidada(hoy);

    /// <summary>Compara el conjunto de opciones elegidas (por posición) con el de las correctas.</summary>
    public bool EsRespuestaCorrecta(IEnumerable<int> elegidas)
    {
        if (Tipo == TipoRespuesta.Escrita)
            throw new ExcepcionDeDominio("Esta tarjeta no tiene opciones para comprobar.");

        var correctas = Opciones.Select((o, i) => (o, i)).Where(x => x.o.EsCorrecta).Select(x => x.i);
        return correctas.ToHashSet().SetEquals(elegidas);
    }

    public bool EsNueva => Repaso.EsNueva;
    public bool DebeRepasarse(DateOnly hoy) => Repaso.DebeRepasarse(hoy);
    public int? DeudaVisible(DateOnly hoy) => Repaso.DeudaVisible(hoy);
    public DateOnly ProximoRepaso(DateOnly hoy) => Repaso.ProximoRepaso(hoy);

    private static (string Pregunta, string Respuesta, Opcion[] Opciones) Normalizar( // Revisa que todo este correcto.
        TipoRespuesta tipo, string? pregunta, string? respuesta, IEnumerable<Opcion>? opciones)
    {
        var p = Limpiar(pregunta, "La pregunta", LargoMaximoPregunta, obligatorio: true);
        var r = Limpiar(respuesta, "La respuesta", LargoMaximoRespuesta, obligatorio: tipo == TipoRespuesta.Escrita);
        if (tipo == TipoRespuesta.Escrita) return (p, r, []);

        var lista = (opciones ?? []).Select(o => new Opcion(o.Texto?.Trim() ?? "", o.EsCorrecta)).ToArray();
        ValidarOpciones(tipo, lista);
        return (p, r, lista);
    }

    private static void ValidarOpciones(TipoRespuesta tipo, Opcion[] lista)
    {
        if (lista.Length < MinimoOpciones || lista.Length > MaximoOpciones)
            throw new ExcepcionDeDominio($"Agregá entre {MinimoOpciones} y {MaximoOpciones} opciones con texto.");
        if (lista.Any(o => o.Texto.Length == 0 || o.Texto.Length > LargoMaximoOpcion))
            throw new ExcepcionDeDominio($"Cada opción necesita texto (máximo {LargoMaximoOpcion} caracteres).");
        if (lista.Select(o => o.Texto).Distinct(StringComparer.OrdinalIgnoreCase).Count() != lista.Length)
            throw new ExcepcionDeDominio("Las opciones no pueden repetirse.");

        var correctas = lista.Count(o => o.EsCorrecta);
        if (tipo == TipoRespuesta.OpcionUnica && correctas != 1)
            throw new ExcepcionDeDominio("Marcá exactamente una opción como correcta.");
        if (tipo == TipoRespuesta.OpcionMultiple && correctas == 0)
            throw new ExcepcionDeDominio("Marcá al menos una opción como correcta.");
    }

    private static string Limpiar(string? texto, string nombre, int maximo, bool obligatorio)
    {
        var t = texto?.Trim() ?? "";
        if (obligatorio && t.Length == 0) throw new ExcepcionDeDominio($"{nombre} no puede estar vacía.");
        if (t.Length > maximo) throw new ExcepcionDeDominio($"{nombre} no puede superar los {maximo} caracteres.");
        return t;
    }
}
