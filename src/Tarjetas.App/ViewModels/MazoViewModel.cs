using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.App.ViewModels;

public partial class MazoViewModel(MazosServicio servicio, IntercambioServicio intercambio)
    : ObservableObject, IQueryAttributable
{
    private Guid mazoId;
    private MazoVista? mazo;

    public ObservableCollection<TarjetaItem> Tarjetas { get; } = [];

    [ObservableProperty] private string nombre = "";
    [ObservableProperty] private string resumen = "";
    [ObservableProperty] private string textoRepasar = "";
    [ObservableProperty] private bool hayPendientes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonEdicion), nameof(TextoAyuda))]
    private bool modoEdicion;

    public string TextoBotonEdicion => ModoEdicion ? "✓" : "✏️";
    public string TextoAyuda => ModoEdicion
        ? "Modo edición: tocá una tarjeta para editarla"
        : "Tocá una tarjeta para ver su respuesta";

    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        mazoId = Guid.Parse(Convert.ToString(query["id"])!);

    partial void OnModoEdicionChanged(bool value)
    {
        foreach (var tarjeta in Tarjetas) tarjeta.EnEdicion = value;
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        mazo = await servicio.ObtenerAsync(mazoId);
        if (mazo is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }
        Refrescar();
    }

    public void Refrescar()
    {
        if (mazo is null) return;
        Nombre = mazo.Nombre;
        HayPendientes = mazo.Pendientes > 0;
        TextoRepasar = HayPendientes ? $"Repasar · {mazo.Pendientes}" : "Al día ✓";
        Resumen = mazo.Tarjetas.Count == 1 ? "1 tarjeta" : $"{mazo.Tarjetas.Count} tarjetas";

        Tarjetas.Clear();
        foreach (var tarjeta in mazo.Tarjetas) Tarjetas.Add(TarjetaItem.De(tarjeta, mazo.Color, ModoEdicion));
    }

    [RelayCommand]
    private void AlternarEdicion() => ModoEdicion = !ModoEdicion;

    /// <summary>Normal: muestra/oculta la respuesta. En modo edición: abre la tarjeta para editarla.</summary>
    [RelayCommand]
    private async Task TocarTarjetaAsync(TarjetaItem tarjeta)
    {
        if (ModoEdicion)
            await Shell.Current.GoToAsync($"{nameof(TarjetaEditorPage)}?mazoId={mazoId}&tarjetaId={tarjeta.Id}");
        else
            tarjeta.Revelada = !tarjeta.Revelada;
    }

    [RelayCommand]
    private Task NuevaTarjetaAsync() => Shell.Current.GoToAsync($"{nameof(TarjetaEditorPage)}?mazoId={mazoId}");

    [RelayCommand]
    private Task RepasarAsync() => Shell.Current.GoToAsync($"{nameof(RepasoPage)}?mazoId={mazoId}");

    [RelayCommand]
    private Task EditarMazoAsync() => Shell.Current.GoToAsync($"{nameof(MazoEditorPage)}?mazoId={mazoId}");

    [RelayCommand]
    private async Task ExportarAsync()
    {
        try
        {
            var formatos = intercambio.FormatosDeExportacion;
            var eleccion = await Shell.Current.DisplayActionSheetAsync(
                "Exportar mazo", "Cancelar", null, formatos.Select(f => f.NombreVisible).ToArray());
            var formato = formatos.FirstOrDefault(f => f.NombreVisible == eleccion);
            if (formato is null) return;

            var archivo = await intercambio.ExportarAsync(mazoId, formato.Id);
            var ruta = Path.Combine(FileSystem.CacheDirectory, archivo.NombreArchivo);
            await File.WriteAllTextAsync(ruta, archivo.Contenido, new UTF8Encoding(encoderShouldEmitUTF8Identifier: formato.Id == "csv"));
            await Share.Default.RequestAsync(new ShareFileRequest { Title = "Exportar mazo", File = new ShareFile(ruta) });
        }
        catch (Exception ex) when (ex is ExcepcionDeDominio or ExcepcionDeImportacion or IOException)
        {
            await Shell.Current.DisplayAlertAsync("No se pudo exportar", ex.Message, "OK");
        }
    }
}
