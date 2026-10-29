using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;
using Tarjetas.Dominio.Recompensas;

namespace Tarjetas.Aplicacion;

/// <summary>Casos de uso de mazos y tarjetas. Orquesta; las reglas viven en el dominio.</summary>
public sealed class MazosServicio(IMazoRepositorio repositorio, IRecompensasRepositorio recompensas, TimeProvider reloj)
{
    private DateOnly Hoy => DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
    private DateTime AhoraUtc => reloj.GetUtcNow().UtcDateTime;

    // ---------- Mazos ----------
    public async Task<IReadOnlyList<MazoResumen>> ListarAsync(CancellationToken ct = default)
    {
        var hoy = Hoy;
        var mazos = await repositorio.ListarAsync(ct);
        return mazos
            .Select(m => new MazoResumen(m.Id, m.Nombre, m.Color, m.Tarjetas.Count, m.Pendientes(hoy).Count))
            .ToList();
    }

    public async Task<MazoVista?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var mazo = await repositorio.ObtenerAsync(id, ct);
        return mazo is null ? null : MazoVista.De(mazo, Hoy);
    }

    public async Task<Guid> CrearMazoAsync(string nombre, ColorMazo color, CancellationToken ct = default)
    {
        var mazo = Mazo.Crear(nombre, color, AhoraUtc);
        await repositorio.GuardarAsync(mazo, ct);
        return mazo.Id;
    }

    public Task EditarMazoAsync(Guid id, string nombre, ColorMazo color, CancellationToken ct = default) =>
        ModificarAsync(id, m => m.Editar(nombre, color), ct);

    public async Task EliminarMazoAsync(Guid id, CancellationToken ct = default)
    {
        await repositorio.EliminarAsync(id, ct);
        await recompensas.EliminarAsync(id, ct);
    }

    // ---------- Tarjetas ----------
    public Task AgregarTarjetaAsync(Guid mazoId, TarjetaDatos datos, CancellationToken ct = default) =>
        AgregarTarjetasAsync(mazoId, [datos], ct);

    /// <summary>Valida todo y agrega todas o ninguna.</summary>
    public Task AgregarTarjetasAsync(Guid mazoId, IEnumerable<TarjetaDatos> datos, CancellationToken ct = default)
    {
        var ahora = AhoraUtc;
        return ModificarAsync(mazoId, mazo =>
        {
            var nuevas = datos
                .Select(d => Tarjeta.Crear(d.Tipo, d.Pregunta, d.Respuesta, d.ComoOpciones(), ahora))
                .ToList();
            mazo.AgregarTarjetas(nuevas);
        }, ct);
    }

    public Task EditarTarjetaAsync(Guid mazoId, Guid tarjetaId, TarjetaDatos datos, CancellationToken ct = default) =>
        ModificarAsync(mazoId, m => m.ObtenerTarjeta(tarjetaId).Editar(datos.Tipo, datos.Pregunta, datos.Respuesta, datos.ComoOpciones()), ct);

    public Task EliminarTarjetaAsync(Guid mazoId, Guid tarjetaId, CancellationToken ct = default) =>
        ModificarAsync(mazoId, m => m.QuitarTarjeta(tarjetaId), ct);

    // ---------- Repaso ----------
    public async Task<SesionDeRepaso?> IniciarRepasoAsync(Guid mazoId, CancellationToken ct = default)
    {
        var mazo = await repositorio.ObtenerAsync(mazoId, ct);
        if (mazo is null) return null;
        var hoy = Hoy;
        return new SesionDeRepaso(mazo.Id, mazo.Nombre, mazo.Color,
            mazo.Pendientes(hoy).Select(t => TarjetaVista.De(t, hoy)).ToList());
    }

    public async Task<bool> ComprobarAsync(Guid mazoId, Guid tarjetaId, IEnumerable<int> elegidas, CancellationToken ct = default)
    {
        var mazo = await ObtenerOFallarAsync(mazoId, ct);
        return mazo.ObtenerTarjeta(tarjetaId).EsRespuestaCorrecta(elegidas);
    }

    public async Task<bool> ComprobarEscritaAsync(Guid mazoId, Guid tarjetaId, string intento, CancellationToken ct = default)
    {
        var mazo = await ObtenerOFallarAsync(mazoId, ct);
        return mazo.ObtenerTarjeta(tarjetaId).EsRespuestaEscritaCorrecta(intento);
    }

    public Task RepasarTarjetaAsync(Guid mazoId, Guid tarjetaId, bool recordada, CancellationToken ct = default)
    {
        var hoy = Hoy;
        return ModificarAsync(mazoId, m =>
        {
            var tarjeta = m.ObtenerTarjeta(tarjetaId);
            if (recordada) tarjeta.MarcarComoRecordada(hoy);
            else tarjeta.MarcarComoOlvidada(hoy);
        }, ct);
    }

    // ---------- Internos ----------
    private async Task<Mazo> ObtenerOFallarAsync(Guid id, CancellationToken ct) =>
        await repositorio.ObtenerAsync(id, ct) ?? throw new ExcepcionDeDominio("El mazo ya no existe.");

    private async Task ModificarAsync(Guid id, Action<Mazo> cambio, CancellationToken ct)
    {
        var mazo = await ObtenerOFallarAsync(id, ct);
        cambio(mazo);
        await repositorio.GuardarAsync(mazo, ct);
    }
}
