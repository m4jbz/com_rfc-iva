# ComSat – Servidor COM local (DLL) y cliente de consola

Práctica 1, Tema 4 (COM/DCOM). Servidor COM In-Process en C# con `ValidarRFC` y `CalcularIva`, y un cliente de consola que lo consume con Early Binding y Late Binding.

## Requisitos

- Windows 10/11 de 64 bits
- Visual Studio 2019 con la carga de trabajo "Desarrollo de escritorio con .NET"
- .NET Framework 4.8 (Targeting Pack)
- Permisos de Administrador (para registrar el componente)

## Estructura

```
ComSat.sln                      Solución de Visual Studio 2019
ServidorComSat/                 Biblioteca de clases (DLL) – servidor COM
  ValidadorSAT.cs               Interfaz IValidadorSAT y clase ValidadorSAT
  Properties/AssemblyInfo.cs    ComVisible y GUID de la librería de tipos
ClienteComSat/                  Aplicación de consola – cliente
  Program.cs                    Módulos Early y Late Binding
  ServidorComSatInterop.cs      Llamadas por vtable (Early Binding)
registrar.bat                   Registra el componente (regasm)
desregistrar.bat                Elimina el registro
rutas_regedit.txt               Rutas de regedit para la inspección
```

## Datos del componente

| Elemento | Valor |
|---|---|
| ProgID | `ServidorComSat.ValidadorSAT` |
| CLSID | `{796143f6-dba5-42cf-8230-f17df181b3e0}` |
| IID (`IValidadorSAT`) | `{377c5c35-b4ca-4db6-a094-472be28b7079}` |
| LIBID (.tlb) | `{c5ad25b5-7f0f-4d87-a229-2f4b64fba08d}` |

## Solución (`ComSat.sln`)

Contiene dos proyectos. `ClienteComSat` depende de `ServidorComSat`, por lo que este se compila primero. Ambos usan .NET Framework 4.8, plataforma AnyCPU con `Prefer32Bit` desactivado en el cliente.

Al compilar `ServidorComSat` se genera además `ServidorComSat.tlb` junto a la DLL (paso post-compilación con `tlbexp`).

## Compilar

### Desde Visual Studio

1. Abrir `ComSat.sln`.
2. Seleccionar la configuración **Release**.
3. `Compilar > Compilar solución` (`Ctrl+Shift+B`).

### Desde la línea de comandos

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" ComSat.sln /p:Configuration=Release
```

Ajustar la ruta si la edición de Visual Studio es otra (Community o Professional).

### Salida

```
ServidorComSat\bin\Release\ServidorComSat.dll
ServidorComSat\bin\Release\ServidorComSat.tlb
ClienteComSat\bin\Release\ClienteComSat.exe
```

## Usar

1. Abrir un **CMD como Administrador** en la carpeta del proyecto.
2. Registrar el componente:
   ```
   registrar.bat
   ```
   Equivale a `regasm ServidorComSat.dll /codebase /tlb` con el regasm de 64 bits. Admite `Debug` como argumento (`registrar.bat Debug`); por defecto usa `Release`.
3. Ejecutar el cliente:
   ```
   ClienteComSat\bin\Release\ClienteComSat.exe
   ```
   Muestra los resultados de `ValidarRFC` y `CalcularIva` primero con Early Binding y luego con Late Binding.
4. Inspeccionar el registro con `regedit.exe` usando las rutas de `rutas_regedit.txt`.
5. Para eliminar el registro:
   ```
   desregistrar.bat
   ```

## Notas

- Si se mueve o recompila la DLL en otra carpeta, ejecutar de nuevo `registrar.bat`.
- Cerrar `ClienteComSat.exe` antes de recompilar; mientras está abierto bloquea `ServidorComSat.dll`.
- Si se cambian los GUID, actualizarlos también en `ClienteComSat/ServidorComSatInterop.cs`, y ejecutar `desregistrar.bat` antes de registrar la nueva versión.
