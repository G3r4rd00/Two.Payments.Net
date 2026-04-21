# Guía de Publicación del Paquete NuGet - Two.Payments

## Prerrequisitos

1. **Cuenta en NuGet.org**
   - Crear una cuenta en https://www.nuget.org/
   - Verificar tu correo electrónico

2. **API Key de NuGet**
   - Ir a https://www.nuget.org/account/apikeys
   - Crear una nueva API Key con permisos de "Push new packages and package versions"
   - Guardar la key en un lugar seguro

## Opción 1: Usar el Script PowerShell (Recomendado)

```powershell
# Ejecutar el script desde la raíz del proyecto
.\build-nuget.ps1
```

El script automáticamente:
- Limpiará builds anteriores
- Restaurará las dependencias
- Compilará el proyecto en modo Release
- Ejecutará los tests
- Creará el paquete NuGet en la carpeta `./nupkgs`

## Opción 2: Comandos Manuales

### 1. Limpiar el proyecto
```powershell
dotnet clean src/Two.Payments/Two.Payments.csproj -c Release
```

### 2. Restaurar dependencias
```powershell
dotnet restore src/Two.Payments/Two.Payments.csproj
```

### 3. Compilar el proyecto
```powershell
dotnet build src/Two.Payments/Two.Payments.csproj -c Release
```

### 4. Ejecutar tests (opcional)
```powershell
dotnet test tests/Two.Payments.Tests/Two.Payments.Tests.csproj -c Release
```

### 5. Crear el paquete
```powershell
dotnet pack src/Two.Payments/Two.Payments.csproj -c Release --output ./nupkgs
```

## Publicar en NuGet.org

### Método 1: Línea de comandos
```powershell
dotnet nuget push ./nupkgs/Two.Payments.1.0.0.nupkg --api-key TU_API_KEY --source https://api.nuget.org/v3/index.json
```

### Método 2: Portal web de NuGet.org
1. Ir a https://www.nuget.org/packages/manage/upload
2. Subir el archivo `.nupkg` desde la carpeta `./nupkgs`
3. Verificar la información del paquete
4. Hacer clic en "Submit"

## Verificar el Paquete Localmente

Antes de publicar, puedes probar el paquete localmente:

```powershell
# Crear una carpeta local como fuente de NuGet
mkdir C:\local-nuget-feed

# Agregar la fuente local
dotnet nuget add source C:\local-nuget-feed --name LocalFeed

# Copiar el paquete a la fuente local
Copy-Item ./nupkgs/Two.Payments.*.nupkg C:\local-nuget-feed\

# Crear un proyecto de prueba
mkdir test-project
cd test-project
dotnet new console
dotnet add package Two.Payments --source C:\local-nuget-feed
```

## Actualizar Versiones

Para publicar una nueva versión:

1. Actualizar el número de versión en `src/Two.Payments/Two.Payments.csproj`:
   ```xml
   <Version>1.0.1</Version>
   ```

2. Actualizar las notas de la versión:
   ```xml
   <PackageReleaseNotes>Bug fixes and improvements.</PackageReleaseNotes>
   ```

3. Ejecutar el script de build nuevamente:
   ```powershell
   .\build-nuget.ps1
   ```

## Buenas Prácticas

1. **Versionado Semántico**: Usar formato `MAJOR.MINOR.PATCH`
   - MAJOR: Cambios incompatibles con versiones anteriores
   - MINOR: Nuevas funcionalidades compatibles
   - PATCH: Correcciones de bugs

2. **Release Notes**: Siempre actualizar las notas de versión

3. **Testing**: Asegurarse de que todos los tests pasen antes de publicar

4. **README**: Mantener el README.md actualizado con ejemplos de uso

5. **Licencia**: Verificar que la licencia MIT esté presente

## Información del Paquete Configurada

- **PackageId**: Two.Payments
- **Autor**: Gerardo Tous Vallespir
- **Licencia**: MIT
- **Repositorio**: https://github.com/G3r4rd00/Two.Payments.Net
- **Targets**: .NET Standard 2.0, .NET 6.0
- **Documentación XML**: Incluida
- **Símbolos de Debug**: Incluidos (.snupkg)

## Soporte

Para problemas o preguntas, crear un issue en:
https://github.com/G3r4rd00/Two.Payments.Net/issues
