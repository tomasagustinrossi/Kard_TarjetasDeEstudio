using System.Text;
using System.Text.RegularExpressions;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.Infraestructura.Intercambio;

/// <summary>
/// Interpreta texto libre probando, en orden: tablas (| a | b |), bloques etiquetados (Pregunta:/Respuesta:)
/// y líneas separadas por tabulador, "::", "=>" o "→". Las preguntas con opciones A) B) C) y una respuesta
/// que apunta a una letra se convierten en tarjetas de opción única o múltiple.
/// </summary>
internal static class ParserDeTexto
{
    private const RegexOptions Opc = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private static readonly Regex EtiquetaPregunta = new(
        @"^(?:[-*•]\s*)?(?:\d+\s*[.)]\s*)?(?:(?:pregunta|question|frente|anverso|front)\s*\d*\s*[:.)\-–]\s*|[pq]\s*\d*\s*:\s*)(?<t>.*)$", Opc);
    private static readonly Regex EtiquetaRespuesta = new(
        @"^(?:[-*•]\s*)?(?:(?:respuesta(?:\s+correcta)?|answer|reverso|dorso|back|correcta)\s*[:.)\-–]\s*|[ra]\s*:\s*)(?<t>.*)$", Opc);
    private static readonly Regex EtiquetaExplicacion = new(
        @"^(?:[-*•]\s*)?(?:explicaci[oó]n|justificaci[oó]n|explanation|porque|por qu[eé])\s*[:\-–]\s*(?<t>.*)$", Opc);
    private static readonly Regex LineaOpcion = new(@"^(?:[-*•]\s*)?\(?(?<l>[A-Fa-f])[).]\s+(?<t>.+)$", Opc);
    private static readonly Regex LineaNumerada = new(@"^\d+\s*[.)]\s+(?<t>.+)$", Opc);
    private static readonly Regex LineaSeparada = new(@"^(?<p>.+?)\s*(?:\t+|::|=>|→)\s*(?<r>.+)$", Opc);
    private static readonly Regex Decoracion = new(@"(\*\*|__|`)", Opc);
    private static readonly Regex FilaSeparadora = new(@"^:?-{2,}:?$", Opc);
    private static readonly Regex TokenLetra = new(@"^\(?(?<l>[A-Fa-f])(?:[).]|$)", Opc);
    private static readonly Regex SeparadorDeLetras = new(@"[,;/&]|\s+y\s+|\s+and\s+|\s+e\s+", Opc);

    public static List<TarjetaDatos> Parsear(string contenido)
    {
        var lineas = contenido.Split('\n').Select(Limpiar).ToList();

        var tarjetas = ParsearTablas(lineas);
        if (tarjetas.Count == 0) tarjetas = ParsearBloques(lineas);
        if (tarjetas.Count == 0) tarjetas = ParsearLineasSeparadas(lineas);
        return tarjetas;
    }

    private static string Limpiar(string linea)
    {
        var l = Decoracion.Replace(linea.TrimEnd('\r'), "").Trim();
        return Regex.Replace(l, @"^#+\s*", "");
    }

    // ---------- Tablas ----------
    private static List<TarjetaDatos> ParsearTablas(List<string> lineas)
    {
        var tarjetas = new List<TarjetaDatos>();
        foreach (var linea in lineas.Where(l => l.StartsWith('|')))
        {
            var celdas = linea.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
            if (celdas.All(FilaSeparadora.IsMatch)) continue;
            if (celdas.Length < 2 || celdas[0].Length == 0 || celdas[1].Length == 0) continue;
            if (EncabezadosConocidos.Es(celdas[0], celdas[1])) continue;
            tarjetas.Add(TarjetaDatos.Escrita(celdas[0], celdas[1]));
        }
        return tarjetas;
    }

    // ---------- Bloques "Pregunta: … Respuesta: …" ----------
    private enum Parte { Pregunta, Respuesta, Explicacion }

    private sealed class Borrador
    {
        public StringBuilder Pregunta { get; } = new();
        public StringBuilder Respuesta { get; } = new();
        public StringBuilder Explicacion { get; } = new();
        public List<(char Letra, string Texto)> Opciones { get; } = [];
        public Parte ParteActual { get; set; } = Parte.Pregunta;

        public void Agregar(string texto)
        {
            var destino = ParteActual switch
            {
                Parte.Respuesta => Respuesta,
                Parte.Explicacion => Explicacion,
                _ => Pregunta
            };
            if (destino.Length > 0 && texto.Length > 0) destino.Append(' ');
            destino.Append(texto);
        }
    }

    private static List<TarjetaDatos> ParsearBloques(List<string> lineas)
    {
        var usaEtiquetaPregunta = lineas.Any(EtiquetaPregunta.IsMatch);
        var tarjetas = new List<TarjetaDatos>();
        Borrador? actual = null;

        void Cerrar()
        {
            if (actual is not null && Construir(actual) is { } tarjeta) tarjetas.Add(tarjeta);
            actual = null;
        }

        foreach (var linea in lineas)
        {
            if (linea.Length == 0) continue;

            if (EtiquetaPregunta.Match(linea) is { Success: true } pregunta)
            {
                Cerrar();
                actual = new Borrador();
                actual.Agregar(pregunta.Groups["t"].Value.Trim());
            }
            else if (!usaEtiquetaPregunta
                     && LineaNumerada.Match(linea) is { Success: true } numerada
                     && (actual is null || actual.Respuesta.Length > 0))
            {
                Cerrar();
                actual = new Borrador();
                actual.Agregar(numerada.Groups["t"].Value.Trim());
            }
            else if (actual is null)
            {
                continue;
            }
            else if (EtiquetaExplicacion.Match(linea) is { Success: true } explicacion)
            {
                actual.ParteActual = Parte.Explicacion;
                actual.Agregar(explicacion.Groups["t"].Value.Trim());
            }
            else if (EtiquetaRespuesta.Match(linea) is { Success: true } respuesta)
            {
                actual.ParteActual = Parte.Respuesta;
                actual.Agregar(respuesta.Groups["t"].Value.Trim());
            }
            else if (actual.ParteActual == Parte.Pregunta && LineaOpcion.Match(linea) is { Success: true } opcion)
            {
                actual.Opciones.Add((char.ToUpperInvariant(opcion.Groups["l"].Value[0]), opcion.Groups["t"].Value.Trim()));
            }
            else
            {
                actual.Agregar(linea);
            }
        }
        Cerrar();
        return tarjetas;
    }

    private static TarjetaDatos? Construir(Borrador b)
    {
        var pregunta = b.Pregunta.ToString().Trim();
        var respuesta = b.Respuesta.ToString().Trim();
        var explicacion = b.Explicacion.ToString().Trim();
        if (pregunta.Length == 0 || respuesta.Length == 0) return null;

        if (b.Opciones.Count >= 2)
        {
            var correctas = ResolverCorrectas(b.Opciones, respuesta);
            if (correctas.Count > 0)
            {
                var opciones = b.Opciones.Select(o => new OpcionDatos(o.Texto, correctas.Contains(o.Letra))).ToList();
                var tipo = correctas.Count > 1 ? TipoRespuesta.OpcionMultiple : TipoRespuesta.OpcionUnica;
                return new TarjetaDatos(tipo, pregunta, explicacion, opciones);
            }
        }

        return TarjetaDatos.Escrita(pregunta, explicacion.Length > 0 ? $"{respuesta}\n{explicacion}" : respuesta);
    }

    /// <summary>Entiende "B", "B) Milán", "A y C", "B, D" o, si no hay letras, el texto de la opción correcta.</summary>
    private static HashSet<char> ResolverCorrectas(List<(char Letra, string Texto)> opciones, string respuesta)
    {
        var validas = opciones.Select(o => o.Letra).ToHashSet();
        var letras = SeparadorDeLetras.Split(respuesta)
            .Select(token => TokenLetra.Match(token.Trim()))
            .Where(m => m.Success)
            .Select(m => char.ToUpperInvariant(m.Groups["l"].Value[0]))
            .Where(validas.Contains)
            .ToHashSet();
        if (letras.Count > 0) return letras;

        var normalizada = TextoNormalizado.De(respuesta);
        return opciones
            .Where(o => { var t = TextoNormalizado.De(o.Texto); return t.Length > 0 && normalizada.Contains(t); })
            .Select(o => o.Letra)
            .ToHashSet();
    }

    // ---------- Líneas "pregunta<TAB>respuesta" ----------
    private static List<TarjetaDatos> ParsearLineasSeparadas(List<string> lineas)
    {
        var tarjetas = new List<TarjetaDatos>();
        foreach (var linea in lineas)
        {
            var m = LineaSeparada.Match(linea);
            if (!m.Success) continue;
            var pregunta = m.Groups["p"].Value.Trim();
            var respuesta = m.Groups["r"].Value.Trim();
            if (EncabezadosConocidos.Es(pregunta, respuesta)) continue;
            tarjetas.Add(TarjetaDatos.Escrita(pregunta, respuesta));
        }
        return tarjetas;
    }
}
