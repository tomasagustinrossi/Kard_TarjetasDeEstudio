#!/bin/bash
# Ejecutar UNA vez dentro de la carpeta Tarjetas (en tu Mac).
set -e

# 1) Workload de MAUI (si ya lo tenés instalado, no pasa nada)
dotnet workload install maui

# 2) Recursos de plantilla (íconos, splash, Platforms de iOS/MacCatalyst) sin pisar mis archivos
tmp=$(mktemp -d)
dotnet new maui -n Tarjetas.App -o "$tmp/Tarjetas.App"
cp -Rn "$tmp/Tarjetas.App/." src/Tarjetas.App/
rm -rf src/Tarjetas.App/Platforms/Android src/Tarjetas.App/Platforms/Windows src/Tarjetas.App/Platforms/Tizen
rm -f src/Tarjetas.App/MainPage.xaml src/Tarjetas.App/MainPage.xaml.cs
rm -rf "$tmp"

# 3) Solución
dotnet new sln -n Tarjetas
dotnet sln add src/Tarjetas.Dominio src/Tarjetas.Aplicacion src/Tarjetas.Infraestructura src/Tarjetas.App tests/Tarjetas.Pruebas

# 4) Pruebas del dominio
dotnet test tests/Tarjetas.Pruebas

echo "Listo. Ejecutá:  dotnet build src/Tarjetas.App -f net10.0-maccatalyst -t:Run"
