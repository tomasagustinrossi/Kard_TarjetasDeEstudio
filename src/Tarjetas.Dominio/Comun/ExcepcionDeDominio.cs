namespace Tarjetas.Dominio.Comun;

/// <summary>Se lanza cuando se viola una regla de negocio. El mensaje es apto para mostrar al usuario.</summary>
public sealed class ExcepcionDeDominio(string mensaje) : Exception(mensaje);
