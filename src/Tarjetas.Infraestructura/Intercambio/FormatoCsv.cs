using System.Text;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.Infraestructura.Intercambio;

/// <summary>
/// CSV/TSV de dos columnas (pregunta, respuesta): lo que entienden Excel, Google Sheets y Anki.
/// Al exportar, las tarjetas con opciones se aplanan (la respuesta son las opciones correctas).
/// </summary>
public sealed class FormatoCsv : IFormatoDeIntercambio
{
    public string Id => "csv";
    public string NombreVisible => "CSV (Excel, Sheets, Anki)";
    public string Extension => ".csv";
    public bool PuedeEscribir => true;

    public bool PuedeLeer(string nombreArchivo, string contenido) =>
        nombreArchivo.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
        || nombreArchivo.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase);

    public MazoDeIntercambio Leer(string contenido, string nombreSugerido)
    {
        var filas = CsvLector.Leer(contenido, CsvLector.DetectarDelimitador(contenido));
        var tarjetas = new List<TarjetaDatos>();
        for (var i = 0; i < filas.Count; i++)
        {
            var fila = filas[i];
            if (fila.Length < 2) continue;
            if (i == 0 && EncabezadosConocidos.Es(fila[0], fila[1])) continue;
            tarjetas.Add(TarjetaDatos.Escrita(fila[0].Trim(), fila[1].Trim()));
        }
        return new MazoDeIntercambio(nombreSugerido, ColorMazo.Predeterminado, tarjetas);
    }

    public string Escribir(MazoDeIntercambio mazo)
    {
        var sb = new StringBuilder("pregunta,respuesta\n");
        foreach (var t in mazo.Tarjetas)
            sb.Append(Campo(t.Pregunta)).Append(',').Append(Campo(RespuestaPlana(t))).Append('\n');
        return sb.ToString();
    }

    private static string RespuestaPlana(TarjetaDatos t) => t.Tipo == TipoRespuesta.Escrita
        ? t.Respuesta
        : string.Join(" | ", (t.Opciones ?? []).Where(o => o.EsCorrecta).Select(o => o.Texto));

    private static string Campo(string valor) =>
        valor.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{valor.Replace("\"", "\"\"")}\"" : valor;
}

internal static class CsvLector
{
    public static char DetectarDelimitador(string contenido)
    {
        var primera = contenido.Split('\n')[0];
        var mejor = new[] { '\t', ';', ',' }.OrderByDescending(d => primera.Count(c => c == d)).First();
        return primera.Contains(mejor) ? mejor : ',';
    }

    public static List<string[]> Leer(string contenido, char delimitador)
    {
        var filas = new List<string[]>();
        var fila = new List<string>();
        var campo = new StringBuilder();
        var enComillas = false;

        void CerrarFila()
        {
            fila.Add(campo.ToString());
            campo.Clear();
            if (fila.Any(x => x.Length > 0)) filas.Add([.. fila]);
            fila.Clear();
        }

        for (var i = 0; i < contenido.Length; i++)
        {
            var c = contenido[i];
            if (enComillas)
            {
                if (c != '"') campo.Append(c);
                else if (i + 1 < contenido.Length && contenido[i + 1] == '"') { campo.Append('"'); i++; }
                else enComillas = false;
            }
            else if (c == '"' && campo.Length == 0) enComillas = true;
            else if (c == delimitador) { fila.Add(campo.ToString()); campo.Clear(); }
            else if (c is '\n' or '\r')
            {
                if (c == '\r' && i + 1 < contenido.Length && contenido[i + 1] == '\n') i++;
                CerrarFila();
            }
            else campo.Append(c);
        }
        if (campo.Length > 0 || fila.Count > 0) CerrarFila();
        return filas;
    }
}
