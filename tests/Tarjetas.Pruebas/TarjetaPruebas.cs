using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.Pruebas;

public class TarjetaPruebas
{
    private static readonly DateTime Ahora = new(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc);

    private static Opcion[] Opciones(params (string, bool)[] datos) =>
        datos.Select(d => new Opcion(d.Item1, d.Item2)).ToArray();

    [Fact]
    public void Una_pregunta_respuesta_simple_de_Gemini_se_crea_como_escrita()
    {
        var t = Tarjeta.CrearEscrita("¿Capital de Francia?", "París", Ahora);
        Assert.Equal(TipoRespuesta.Escrita, t.Tipo);
        Assert.Empty(t.Opciones);
    }

    [Fact]
    public void La_escrita_exige_respuesta()
        => Assert.Throws<ExcepcionDeDominio>(() => Tarjeta.CrearEscrita("¿Capital?", "  ", Ahora));

    [Fact]
    public void La_opcion_unica_exige_exactamente_una_correcta()
    {
        var dos = Opciones(("A", true), ("B", true));
        Assert.Throws<ExcepcionDeDominio>(() => Tarjeta.Crear(TipoRespuesta.OpcionUnica, "¿?", "", dos, Ahora));
    }

    [Fact]
    public void La_multiple_compara_el_conjunto_completo()
    {
        var t = Tarjeta.Crear(TipoRespuesta.OpcionMultiple, "¿Primos?", "",
            Opciones(("2", true), ("4", false), ("5", true)), Ahora);
        Assert.True(t.EsRespuestaCorrecta([2, 0]));
        Assert.False(t.EsRespuestaCorrecta([0]));
        Assert.False(t.EsRespuestaCorrecta([0, 1, 2]));
    }

    [Fact]
    public void La_escrita_ignora_tildes_mayusculas_y_acepta_alternativas()
    {
        var t = Tarjeta.CrearEscrita("¿Capital?", "París | Paris Francia", Ahora);
        Assert.True(t.EsRespuestaEscritaCorrecta("  paris "));
        Assert.True(t.EsRespuestaEscritaCorrecta("PARÍS."));
        Assert.False(t.EsRespuestaEscritaCorrecta("Roma"));
        Assert.False(t.EsRespuestaEscritaCorrecta(""));
    }

    [Fact]
    public void Las_opciones_conservan_su_justificacion()
    {
        var t = Tarjeta.Crear(TipoRespuesta.OpcionUnica, "¿?", "",
            [new Opcion("A", true, "  porque sí "), new Opcion("B", false)], Ahora);
        Assert.Equal("porque sí", t.Opciones[0].Justificacion);
    }

    [Fact]
    public void Rechaza_opciones_repetidas()
    {
        var repetidas = Opciones(("A", true), ("a", false));
        Assert.Throws<ExcepcionDeDominio>(() => Tarjeta.Crear(TipoRespuesta.OpcionUnica, "¿?", "", repetidas, Ahora));
    }

    [Fact]
    public void Editar_con_datos_invalidos_no_modifica_la_tarjeta()
    {
        var t = Tarjeta.CrearEscrita("P", "R", Ahora);
        Assert.Throws<ExcepcionDeDominio>(() => t.Editar(TipoRespuesta.Escrita, "", "R2", null));
        Assert.Equal("P", t.Pregunta);
        Assert.Equal("R", t.Respuesta);
    }
}
