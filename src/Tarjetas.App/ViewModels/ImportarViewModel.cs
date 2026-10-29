using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tarjetas.App.Modelos;
using Tarjetas.App.Paginas;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;
using Tarjetas.Dominio.Comun;

namespace Tarjetas.App.ViewModels;

public partial class ImportarViewModel(IntercambioServicio intercambio, RutasDeDatos rutas) : ObservableObject
{
    private MazoDeIntercambio? mazo;

    public ObservableCollection<string> Muestra { get; } = [];

    [ObservableProperty] private string nombre = "";
    [ObservableProperty] private string resumen = "";
    [ObservableProperty] private bool hayPrevia;
    [ObservableProperty] private bool guiaAbierta;

    public string TextoGuia => GuiaAbierta ? "Ocultar detalles" : "¿Por qué es seguro?";

    partial void OnGuiaAbiertaChanged(bool value) => OnPropertyChanged(nameof(TextoGuia));

    [RelayCommand]
    private void AlternarGuia() => GuiaAbierta = !GuiaAbierta;

    [RelayCommand]
    private async Task ElegirArchivoAsync()
    {
        var tipos = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.MacCatalyst] = ["public.data"],
            [DevicePlatform.iOS] = ["public.data"]
        });
        var archivo = await FilePicker.Default.PickAsync(
            new PickOptions { PickerTitle = "Elegí un archivo de tarjetas", FileTypes = tipos });
        if (archivo is null) return;

        await using var flujo = await archivo.OpenReadAsync();
        using var lector = new StreamReader(flujo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        await PrepararAsync(await lector.ReadToEndAsync(), archivo.FileName);
    }

    [RelayCommand]
    private async Task PegarAsync()
    {
        var texto = Clipboard.Default.HasText ? await Clipboard.Default.GetTextAsync() : null;
        if (string.IsNullOrWhiteSpace(texto))
        {
            await Shell.Current.DisplayAlertAsync("Portapapeles vacío", "Copiá primero el texto de Gemini y volvé a probar.", "OK");
            return;
        }
        await PrepararAsync(texto, "");
    }

    private async Task PrepararAsync(string contenido, string nombreArchivo)
    {
        try
        {
            var previa = intercambio.Previsualizar(contenido, nombreArchivo);
            mazo = previa.Mazo;
            Nombre = mazo.Nombre;

            Muestra.Clear();
            foreach (var tarjeta in mazo.Tarjetas.Take(5)) Muestra.Add($"• {tarjeta.Pregunta}");
            if (mazo.Tarjetas.Count > 5) Muestra.Add($"… y {mazo.Tarjetas.Count - 5} más");

            Resumen = (mazo.Tarjetas.Count == 1 ? "1 tarjeta encontrada" : $"{mazo.Tarjetas.Count} tarjetas encontradas")
                      + (previa.Descartadas > 0 ? $" · {previa.Descartadas} omitidas por no ser válidas" : "");
            HayPrevia = true;
        }
        catch (ExcepcionDeImportacion ex)
        {
            HayPrevia = false;
            await Shell.Current.DisplayAlertAsync("No se pudo importar", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ImportarAsync()
    {
        if (mazo is null) return;
        try
        {
            var id = await intercambio.ImportarAsync(mazo, Nombre, mazo.Color);
            await Shell.Current.GoToAsync("..");
            await Shell.Current.GoToAsync($"{nameof(MazoPage)}?id={id}");
        }
        catch (ExcepcionDeDominio ex)
        {
            await Shell.Current.DisplayAlertAsync("Revisá el mazo", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private Task MostrarUbicacionAsync() => Shell.Current.DisplayAlertAsync(
        "Dónde se guardan tus datos",
        $"{rutas.Carpeta}\n\nLos mazos están en la subcarpeta «mazos» (un archivo por mazo) y las recompensas en «recompensas.json».",
        "OK");
}
