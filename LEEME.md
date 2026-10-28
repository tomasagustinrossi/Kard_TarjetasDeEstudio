# Tarjetas — app de estudio para Mac y iOS (.NET MAUI)

## Puesta en marcha (Mac con Xcode instalado)
1. `cd Tarjetas && ./preparar.sh`
2. `dotnet build src/Tarjetas.App -f net10.0-maccatalyst -t:Run`

Si usás .NET 9, cambiá `net10.0` por `net9.0` en los `.csproj` y reemplazá `DisplayAlertAsync` por `DisplayAlert`.

## Capas (dependen hacia adentro)
- **Tarjetas.Dominio**: `Tarjeta` (raíz de agregado), `ProgramaDeRepaso` (objeto de valor con toda la lógica del repaso y el tope de deuda), `ITarjetaRepositorio`.
- **Tarjetas.Aplicacion**: `TarjetasServicio` (casos de uso) y `TarjetaVista`.
- **Tarjetas.Infraestructura**: repositorio JSON atómico en memoria.
- **Tarjetas.App**: MAUI + MVVM (CommunityToolkit.Mvvm), estilo iOS con modo claro/oscuro.
- **Tarjetas.Pruebas**: xUnit sobre la regla de deuda.

## Regla de deuda
`límite = intervalo×2 − intervalo/2` (70 días → 105). Con atraso entre 1 y el límite se muestra "Debes -N d";
pasado el límite la etiqueta desaparece, pero la tarjeta sigue apareciendo "Para hoy".
Escala de intervalos: 1, 3, 7, 14, 30, 70, 140 días. "La recordé" sube un nivel; "Me olvidé" vuelve al nivel 0.

## Próximos días
- Importar/exportar tarjetas (formatos de Gemini)
- Búsqueda y mazos/etiquetas
- Más animaciones, atajos de teclado y menú de Mac
- Pruebas de `TarjetasServicio` y del repositorio
