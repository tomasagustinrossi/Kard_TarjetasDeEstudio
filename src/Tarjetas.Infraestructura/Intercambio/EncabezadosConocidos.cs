using Tarjetas.Dominio.Comun;

namespace Tarjetas.Infraestructura.Intercambio;

/// <summary>Reconoce filas de encabezado típicas ("Pregunta | Respuesta") para no importarlas como tarjeta.</summary>
internal static class EncabezadosConocidos
{
    private static readonly HashSet<string> Preguntas =
        ["pregunta", "preguntas", "question", "termino", "concepto", "frente", "anverso", "front", "tema", "palabra", "term"];

    private static readonly HashSet<string> Respuestas =
        ["respuesta", "respuestas", "answer", "definicion", "reverso", "dorso", "back", "significado", "definition", "explicacion"];

    public static bool Es(string primera, string segunda) =>
        Preguntas.Contains(TextoNormalizado.De(primera)) && Respuestas.Contains(TextoNormalizado.De(segunda));
}
