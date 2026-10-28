using Tarjetas.Dominio.Repasos;

namespace Tarjetas.Pruebas;

public class ProgramaDeRepasoPruebas
{
    private static readonly DateOnly Hoy = new(2026, 10, 27);

    [Fact]
    public void El_limite_de_deuda_para_70_dias_es_105()
        => Assert.Equal(105, new ProgramaDeRepaso(5, Hoy).LimiteDeDeuda); // nivel 5 = 70 días

    [Fact]
    public void Muestra_la_deuda_mientras_no_pase_el_limite()
    {
        var programa = new ProgramaDeRepaso(5, Hoy.AddDays(-(70 + 10)));
        Assert.Equal(10, programa.DeudaVisible(Hoy));
    }

    [Fact]
    public void La_deuda_desaparece_pasado_el_limite_pero_sigue_pendiente()
    {
        var programa = new ProgramaDeRepaso(5, Hoy.AddDays(-(70 + 106)));
        Assert.Null(programa.DeudaVisible(Hoy));
        Assert.True(programa.DebeRepasarse(Hoy));
    }

    [Fact]
    public void Recordar_sube_de_nivel_y_olvidar_reinicia()
    {
        var programa = new ProgramaDeRepaso(2, Hoy.AddDays(-7));
        Assert.Equal(3, programa.Recordada(Hoy).Nivel);
        Assert.Equal(0, programa.Olvidada(Hoy).Nivel);
    }

    [Fact]
    public void La_primera_vez_que_se_recuerda_queda_en_nivel_0()
        => Assert.Equal(0, ProgramaDeRepaso.Nuevo.Recordada(Hoy).Nivel);
}
