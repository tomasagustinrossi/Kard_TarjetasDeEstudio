namespace Tarjetas.Dominio;

/// <summary>Objeto de valor: una opción de una tarjeta de elección, con una justificación opcional.</summary>
public readonly record struct Opcion(string Texto, bool EsCorrecta, string Justificacion = "");
