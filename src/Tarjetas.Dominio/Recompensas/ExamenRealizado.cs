namespace Tarjetas.Dominio.Recompensas;

public readonly record struct ExamenRealizado(DateTime FechaUtc, int Total, int Aciertos, int Puntos);
