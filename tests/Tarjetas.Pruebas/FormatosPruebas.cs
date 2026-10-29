using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Infraestructura.Intercambio;

namespace Tarjetas.Pruebas;

public class FormatosPruebas
{
    [Fact]
    public void Lee_una_tabla_markdown_de_Gemini()
    {
        const string texto = "| Pregunta | Respuesta |\n|---|---|\n| ¿Capital de Francia? | París |\n| 2 + 2 | 4 |";
        var mazo = new FormatoTexto().Leer(texto, "Gemini");

        Assert.Equal(2, mazo.Tarjetas.Count);
        Assert.Equal("París", mazo.Tarjetas[0].Respuesta);
    }

    [Fact]
    public void Lee_bloques_pregunta_respuesta_con_negritas()
    {
        const string texto =
            "**Pregunta 1:** ¿Año de la Revolución de Mayo?\n**Respuesta:** 1810\n\n" +
            "**Pregunta 2:** ¿Quién cruzó los Andes?\n**Respuesta:** San Martín";
        var mazo = new FormatoTexto().Leer(texto, "Gemini");

        Assert.Equal(2, mazo.Tarjetas.Count);
        Assert.Equal("1810", mazo.Tarjetas[0].Respuesta);
    }

    [Fact]
    public void Reconoce_una_opcion_unica_con_su_explicacion()
    {
        const string texto =
            "Pregunta: ¿Capital de Italia?\nA) Roma\nB) Milán\nC) Turín\nRespuesta correcta: A\nExplicación: Lo es desde 1871.";
        var tarjeta = Assert.Single(new FormatoTexto().Leer(texto, "Quiz").Tarjetas);

        Assert.Equal(TipoRespuesta.OpcionUnica, tarjeta.Tipo);
        Assert.Equal(3, tarjeta.Opciones!.Count);
        Assert.True(tarjeta.Opciones[0].EsCorrecta);
        Assert.Equal("Lo es desde 1871.", tarjeta.Respuesta);
    }

    [Fact]
    public void Reconoce_una_opcion_multiple()
    {
        const string texto = "Pregunta: ¿Cuáles son primos?\nA) 2\nB) 4\nC) 5\nRespuesta: A y C";
        var tarjeta = Assert.Single(new FormatoTexto().Leer(texto, "Quiz").Tarjetas);

        Assert.Equal(TipoRespuesta.OpcionMultiple, tarjeta.Tipo);
        Assert.Equal(2, tarjeta.Opciones!.Count(o => o.EsCorrecta));
    }

    [Fact]
    public void Lee_lineas_separadas_por_tabulador_y_omite_el_encabezado()
    {
        var mazo = new FormatoTexto().Leer("Pregunta\tRespuesta\nHola\tHello", "Copiado");
        var tarjeta = Assert.Single(mazo.Tarjetas);
        Assert.Equal("Hello", tarjeta.Respuesta);
    }

    [Fact]
    public void El_csv_respeta_comillas_y_detecta_el_punto_y_coma()
    {
        const string csv = "pregunta;respuesta\n\"¿Cómo; por qué?\";\"Porque sí\"\n";
        var tarjeta = Assert.Single(new FormatoCsv().Leer(csv, "x").Tarjetas);
        Assert.Equal("¿Cómo; por qué?", tarjeta.Pregunta);
    }

    [Fact]
    public void El_formato_nativo_ida_y_vuelta_conserva_tipos_y_justificaciones()
    {
        var original = new MazoDeIntercambio("Geografía", ColorMazo.Menta,
        [
            TarjetaDatos.Escrita("¿Capital de Chile?", "Santiago"),
            new TarjetaDatos(TipoRespuesta.OpcionUnica, "¿Mayor océano?", "",
                [new OpcionDatos("Pacífico", true, "Cubre un tercio del planeta"), new OpcionDatos("Atlántico", false)])
        ]);

        var formato = new FormatoNativo();
        var json = formato.Escribir(original);

        Assert.True(formato.PuedeLeer("x.json", json));
        var leido = formato.Leer(json, "x");
        Assert.Equal("Geografía", leido.Nombre);
        Assert.Equal(ColorMazo.Menta, leido.Color);
        Assert.Equal("Cubre un tercio del planeta", leido.Tarjetas[1].Opciones![0].Justificacion);
    }
}
