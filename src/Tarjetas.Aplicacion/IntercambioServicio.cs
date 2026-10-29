using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.Aplicacion;

/// <summary>Importar y exportar mazos. Lo que se importa pasa siempre por las reglas del dominio.</summary>
public sealed class IntercambioServicio(
    IEnumerable<IFormatoDeIntercambio> formatos, IMazoRepositorio repositorio, TimeProvider reloj)
{
    public IReadOnlyList<IFormatoDeIntercambio> FormatosDeExportacion => formatos.Where(f => f.PuedeEscribir).ToList();

    /// <summary>Lee el contenido, descarta las tarjetas que el dominio no acepta y devuelve una vista previa.</summary>
    public Previsualizacion Previsualizar(string contenido, string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new ExcepcionDeImportacion("El contenido está vacío.");

        var formato = formatos.FirstOrDefault(f => f.PuedeLeer(nombreArchivo, contenido))
                      ?? throw new ExcepcionDeImportacion("No reconocí el formato de ese contenido.");

        MazoDeIntercambio leido;
        try
        {
            leido = formato.Leer(contenido, NombreSugerido(nombreArchivo));
        }
        catch (ExcepcionDeImportacion)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ExcepcionDeImportacion("No pude leer ese contenido: " + ex.Message);
        }

        var ahora = reloj.GetUtcNow().UtcDateTime;
        var validas = new List<TarjetaDatos>();
        var descartadas = 0;
        foreach (var datos in leido.Tarjetas)
        {
            try
            {
                _ = Tarjeta.Crear(datos.Tipo, datos.Pregunta, datos.Respuesta, datos.ComoOpciones(), ahora);
                validas.Add(datos);
            }
            catch (ExcepcionDeDominio)
            {
                descartadas++;
            }
        }

        if (validas.Count == 0)
            throw new ExcepcionDeImportacion("No encontré tarjetas en ese contenido.");

        return new Previsualizacion(leido with { Tarjetas = validas }, descartadas);
    }

    public async Task<Guid> ImportarAsync(MazoDeIntercambio mazo, string nombre, ColorMazo color, CancellationToken ct = default)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var nuevo = Mazo.Crear(nombre, color, ahora);
        nuevo.AgregarTarjetas(mazo.Tarjetas
            .Select(d => Tarjeta.Crear(d.Tipo, d.Pregunta, d.Respuesta, d.ComoOpciones(), ahora))
            .ToList());
        await repositorio.GuardarAsync(nuevo, ct);
        return nuevo.Id;
    }

    public async Task<ArchivoExportado> ExportarAsync(Guid mazoId, string formatoId, CancellationToken ct = default)
    {
        var mazo = await repositorio.ObtenerAsync(mazoId, ct) ?? throw new ExcepcionDeDominio("El mazo ya no existe.");
        var formato = formatos.FirstOrDefault(f => f.Id == formatoId && f.PuedeEscribir)
                      ?? throw new ExcepcionDeImportacion("Ese formato no permite exportar.");

        var datos = new MazoDeIntercambio(mazo.Nombre, mazo.Color, mazo.Tarjetas
            .Select(t => new TarjetaDatos(t.Tipo, t.Pregunta, t.Respuesta,
                t.Opciones.Count == 0 ? null : t.Opciones.Select(o => new OpcionDatos(o.Texto, o.EsCorrecta, o.Justificacion)).ToList()))
            .ToList());

        return new ArchivoExportado(NombreSeguro(mazo.Nombre) + formato.Extension, formato.Escribir(datos));
    }

    private static string NombreSugerido(string archivo)
    {
        var nombre = Path.GetFileNameWithoutExtension(archivo ?? "");
        if (nombre.EndsWith(".tarjetas", StringComparison.OrdinalIgnoreCase)) nombre = nombre[..^".tarjetas".Length];
        nombre = nombre.Replace('_', ' ').Trim();
        if (nombre.Length > Mazo.LargoMaximoNombre) nombre = nombre[..Mazo.LargoMaximoNombre];
        return nombre.Length == 0 ? "Mazo importado" : nombre;
    }

    private static string NombreSeguro(string nombre)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var limpio = string.Concat(nombre.Select(c => invalidos.Contains(c) ? '_' : c)).Trim();
        return limpio.Length == 0 ? "mazo" : limpio;
    }
}
