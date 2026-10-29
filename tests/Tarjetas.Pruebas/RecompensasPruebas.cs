using Tarjetas.Dominio.Comun;
using Tarjetas.Dominio.Recompensas;

namespace Tarjetas.Pruebas;

public class RecompensasPruebas
{
    private static readonly DateTime Ahora = new(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Un_examen_con_70_por_ciento_o_mas_es_exitoso()
        => Assert.True(new ResultadoDeExamen(10, 7).EsExitoso);

    [Fact]
    public void Un_examen_no_exitoso_no_da_puntos()
        => Assert.Equal(0, new ResultadoDeExamen(10, 6).Puntos);

    [Fact]
    public void Un_examen_perfecto_suma_bonus()
        => Assert.Equal(10 * 10 + 50, new ResultadoDeExamen(10, 10).Puntos);

    [Fact]
    public void Los_puntos_se_acumulan_y_suben_el_rango()
    {
        var recompensas = RecompensasDelMazo.Nuevas(Guid.NewGuid());
        recompensas.RegistrarExamen(new ResultadoDeExamen(10, 10), Ahora);
        recompensas.RegistrarExamen(new ResultadoDeExamen(10, 10), Ahora);

        Assert.Equal(300, recompensas.Puntos);
        Assert.Equal(2, recompensas.ExamenesExitosos);
        Assert.Equal(RangoDeMazo.Aprendiz, recompensas.Rango);
    }

    [Fact]
    public void Un_resultado_invalido_falla()
        => Assert.Throws<ExcepcionDeDominio>(() => new ResultadoDeExamen(0, 0));
}
