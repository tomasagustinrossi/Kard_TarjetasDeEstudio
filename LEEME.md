# Tarjetas — mazos de estudio para Mac y iOS (.NET MAUI)

## Puesta en marcha (Mac con Xcode)
1. `./preparar.sh` (solo la primera vez en un proyecto nuevo)
2. `xattr -dr com.apple.quarantine .` (si descargaste el zip con Safari)
3. `dotnet build src/Tarjetas.App -f net10.0-maccatalyst`
4. `open src/Tarjetas.App/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/Tarjetas.app`

## Cómo se usa
- **Mazos**: tocar un mazo empieza el repaso. El lápiz de abajo (✏️) activa el *modo edición*: los mazos tiemblan y al tocar uno se abren sus tarjetas. El ✎ junto al nombre edita nombre y color.
- **Dentro de un mazo**: cuadrados de colores Keep con la respuesta pixelada (▒▒▒); tocar la tarjeta la revela. Con el modo edición, tocar abre el editor.
- **Tipos de tarjeta**: Escrita, Opción única y Múltiple (con justificación opcional por opción).
- **Repaso**: en las escritas podés *escribir* la respuesta (la app dice correcto/incorrecto) o pedir verla y autoevaluarte.
- **Recompensas**: un repaso con ≥ 70 % de aciertos da 10 puntos por acierto (+50 si es perfecto). Se ven en 🏆 Recompensas.
- **Importar/Exportar**: ⬇ Importar (archivo, texto pegado o plugin de Gemini); en un mazo, «Exportar».

## Capas (las dependencias apuntan hacia adentro)
- **Dominio**: `Mazo` → `Tarjeta` → `Opcion`; `ProgramaDeRepaso`; `RecompensasDelMazo` (agregado aparte) y `ResultadoDeExamen`.
- **Aplicación**: `MazosServicio`, `RecompensasServicio`, `IntercambioServicio` y el puerto `IFormatoDeIntercambio`.
- **Infraestructura**: repositorios JSON y los formatos de intercambio (nativo, CSV, texto de Gemini).
- **App**: MAUI + MVVM.

## Formatos
**En disco** (`mazos/{id}.json`, una línea, campos de una letra, lo que vale 0/vacío no se escribe):
`{"i":"…","n":"Historia","c":4,"f":1761600000,"t":[{"i":"…","p":"¿Año de Mayo?","r":"1810","f":1761600000},{"i":"…","p":"¿Capital?","k":1,"o":["Roma","Milán"],"a":[0],"j":["Desde 1871",""],"f":1761600000,"v":2,"u":20400}]}`
(k = tipo 0/1/2, o = opciones, a = índices correctos, j = justificaciones, v = nivel de repaso, u = último repaso en días.)

**Para compartir** (`.tarjetas.json`, legible):
`{"formato":"tarjetas","version":1,"nombre":"Geografía","color":"Menta","tarjetas":[{"tipo":"escrita","pregunta":"¿Capital de Chile?","respuesta":"Santiago"},{"tipo":"unica","pregunta":"¿Mayor océano?","opciones":[{"texto":"Pacífico","correcta":true,"justificacion":"Cubre un tercio del planeta"},{"texto":"Atlántico","correcta":false}]}]}`

**CSV**: `pregunta,respuesta`. **Texto de Gemini**: tablas, «Pregunta:/Respuesta:», «Q:/A:», opciones A) B) C) con «Respuesta correcta: B».

## Plugin de Gemini (extension-gemini/)
Funciona en local: solo corre en gemini.google.com, sin permisos ni conexiones; guarda el texto seleccionado como `.txt` en Descargas.
Instalación: chrome://extensions → Modo desarrollador → Cargar descomprimida → carpeta `extension-gemini`.

## Regla de deuda
`límite = intervalo×2 − intervalo/2` (70 días → 105). Pasado el límite la etiqueta «Debes -N d» desaparece, pero la tarjeta sigue como «Para hoy».
