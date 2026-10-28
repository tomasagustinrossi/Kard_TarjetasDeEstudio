using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.Pruebas;

public class MazoPruebas
{
    private static readonly DateTime Ahora = new(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoy = new(2026, 10, 28);

    [Fact]
    public void El_mazo_necesita_nombre()
        => Assert.Throws<ExcepcionDeDominio>(() => Mazo.Crear("  ", ColorMazo.Predeterminado, Ahora));

    [Fact]
    public void Las_tarjetas_nuevas_estan_pendientes_y_las_recordadas_dejan_de_estarlo()
    {
        var mazo = Mazo.Crear("Historia", ColorMazo.Menta, Ahora);
        var tarjeta = Tarjeta.CrearEscrita("¿Año de la Revolución de Mayo?", "1810", Ahora);
        mazo.AgregarTarjetas([tarjeta]);

        Assert.Single(mazo.Pendientes(Hoy));
        tarjeta.MarcarComoRecordada(Hoy);
        Assert.Empty(mazo.Pendientes(Hoy));
    }

    [Fact]
    public void Quitar_una_tarjeta_inexistente_falla()
    {
        var mazo = Mazo.Crear("Historia", ColorMazo.Menta, Ahora);
        Assert.Throws<ExcepcionDeDominio>(() => mazo.QuitarTarjeta(Guid.NewGuid()));
    }
}
