# Práctica 1 – Servidor COM local (DLL) con C# y .NET Framework 4.8

Estudiante 1: ____________________  Estudiante 2: ____________________

## 1. Pasos de ejecución

1. Abrir `ComSat.sln` en Visual Studio 2019 y compilar en **Release** (`Ctrl+Shift+B`).
   Se generan `ServidorComSat.dll` y `ServidorComSat.tlb` en `ServidorComSat\bin\Release`.
2. Abrir **CMD como Administrador** en la carpeta de la solución y ejecutar `registrar.bat`
   (equivale a `regasm ServidorComSat.dll /codebase /tlb`).
3. Ejecutar `ClienteComSat\bin\Release\ClienteComSat.exe` (muestra Early y Late Binding).
4. Para limpiar: `desregistrar.bat` (como Administrador).

## 2. Inspección en regedit.exe (capturas a incluir)

| Ruta | Qué verificar |
|------|---------------|
| `HKEY_CLASSES_ROOT\ServidorComSat.ValidadorSAT\CLSID` | (Default) = `{796143f6-dba5-42cf-8230-f17df181b3e0}` |
| `HKEY_CLASSES_ROOT\CLSID\{796143f6-dba5-42cf-8230-f17df181b3e0}\InprocServer32` | (Default) = `mscoree.dll`; `Assembly`, `Class`, `RuntimeVersion` y **`CodeBase`** = ruta física de `ServidorComSat.dll` |
| `HKEY_CLASSES_ROOT\CLSID\{796143f6-...}\ProgId` | `ServidorComSat.ValidadorSAT` |
| `HKEY_CLASSES_ROOT\Interface\{377c5c35-b4ca-4db6-a094-472be28b7079}` | Interfaz `IValidadorSAT`, con `TypeLib` apuntando a `{c5ad25b5-7f0f-4d87-a229-2f4b64fba08d}` |
| `HKEY_CLASSES_ROOT\TypeLib\{c5ad25b5-7f0f-4d87-a229-2f4b64fba08d}` | Ruta del `.tlb` |

Nota: `InprocServer32` apunta a `mscoree.dll` (el *shim* del CLR) y la DLL física queda en el valor `CodeBase`.

## 3. Preguntas de análisis

**1. ¿Qué función cumple IUnknown y cómo se abstrae en .NET?**
`IUnknown` es la interfaz base de todo objeto COM. Aporta `QueryInterface` (pedir a un objeto otra
interfaz que implemente), `AddRef` y `Release` (conteo de referencias que decide cuándo se libera el objeto).
En .NET lo oculta el *Runtime Callable Wrapper* (RCW) para clientes administrados y el *COM Callable Wrapper* (CCW)
para servidores administrados. El CCW implementa `IUnknown`/`IDispatch` por nosotros; un cast entre interfaces
ejecuta `QueryInterface`, y el recolector de basura sustituye al conteo manual. `Marshal.ReleaseComObject`
libera la referencia del RCW de forma determinista.

**2. `regsvr32` (DLL C/C++) vs `regasm` (ensamblado .NET)**
`regsvr32` carga la DLL nativa y llama a su función exportada `DllRegisterServer`, que escribe sus propias claves;
`InprocServer32` apunta a la DLL misma. Un ensamblado .NET no exporta `DllRegisterServer`, por lo que `regasm`
lee los metadatos (atributos `Guid`, `ProgId`, `ComVisible`) y escribe las claves; `InprocServer32` apunta a
`mscoree.dll`, que carga el CLR y el ensamblado. Además `regasm` puede generar el `.tlb` (`/tlb`) y registrar la
ruta del ensamblado (`/codebase`) cuando no está en el GAC.

**3. Early Binding vs Late Binding**
*Early*: ventajas: comprobación de tipos en compilación, IntelliSense, mejor rendimiento (llamada directa por vtable)
y código más legible. Desventajas: requiere el `.tlb`/definición de tipos en compilación, acopla el cliente a la
versión de la interfaz (si cambia, hay que recompilar).
*Late*: ventajas: no necesita referencias, solo el ProgID; tolera cambios de versión y sirve para lenguajes de
script. Desventajas: errores de nombre o tipo solo se detectan en ejecución, es más lento (resolución por
`IDispatch`/reflexión en cada llamada) y no hay IntelliSense.

## 4. Conclusiones

(Redactar de forma personal: experiencia, dificultades —por ejemplo, la limitación de `tlbimp` con un `.tlb`
exportado desde .NET, o la necesidad de permisos de Administrador— y aprendizaje.)
