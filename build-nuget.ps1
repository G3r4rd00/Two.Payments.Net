# Script para crear el paquete NuGet de Two.Payments

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Two.Payments - NuGet Package Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Limpiar builds anteriores
Write-Host "Limpiando builds anteriores..." -ForegroundColor Yellow
dotnet clean src/Two.Payments/Two.Payments.csproj -c Release
Remove-Item -Path "src/Two.Payments/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Two.Payments/obj" -Recurse -Force -ErrorAction SilentlyContinue

# Restaurar dependencias
Write-Host ""
Write-Host "Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore src/Two.Payments/Two.Payments.csproj

# Compilar el proyecto
Write-Host ""
Write-Host "Compilando el proyecto..." -ForegroundColor Yellow
dotnet build src/Two.Payments/Two.Payments.csproj -c Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}

# Ejecutar tests (opcional)
Write-Host ""
Write-Host "Ejecutando tests..." -ForegroundColor Yellow
dotnet test tests/Two.Payments.Tests/Two.Payments.Tests.csproj -c Release --no-build --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Advertencia: Algunos tests fallaron" -ForegroundColor Yellow
    $continue = Read-Host "¿Desea continuar con la creación del paquete? (S/N)"
    if ($continue -ne "S" -and $continue -ne "s") {
        exit 1
    }
}

# Crear el paquete NuGet
Write-Host ""
Write-Host "Creando paquete NuGet..." -ForegroundColor Yellow
dotnet pack src/Two.Payments/Two.Payments.csproj -c Release --no-build --output ./nupkgs

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "  ¡Paquete creado exitosamente!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Los paquetes se encuentran en: ./nupkgs" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Para publicar en NuGet.org:" -ForegroundColor Yellow
    Write-Host "  dotnet nuget push ./nupkgs/Two.Payments.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "Error al crear el paquete NuGet" -ForegroundColor Red
    exit 1
}
