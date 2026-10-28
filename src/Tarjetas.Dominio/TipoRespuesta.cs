namespace Tarjetas.Dominio;

/// <summary>Cómo se responde una tarjeta. Las tarjetas "pregunta/respuesta" (p. ej. importadas de Gemini) son Escrita.</summary>
public enum TipoRespuesta : byte
{
    Escrita = 0,
    OpcionUnica = 1,
    OpcionMultiple = 2
}
