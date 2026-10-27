@echo off
REM ============================================================
REM  Instalacion y registro del servidor COM ServidorComSat
REM  (ejecutar como ADMINISTRADOR)
REM  Uso: registrar.bat [Debug|Release]   (por defecto Release)
REM
REM  Copia ServidorComSat.dll a una carpeta fija y registra desde ahi,
REM  de modo que borrar o recompilar ServidorComSat\bin no afecta al
REM  componente registrado.
REM ============================================================
setlocal
set CFG=%~1
if "%CFG%"=="" set CFG=Release

net session >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Ejecute este script desde un CMD con permisos de Administrador.
    exit /b 1
)

set REGASM=%windir%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe
set ORIGEN=%~dp0ServidorComSat\bin\%CFG%\ServidorComSat.dll
set INSTALACION=%ProgramW6432%\ComSat
set DLL=%INSTALACION%\ServidorComSat.dll

if not exist "%ORIGEN%" (
    echo [ERROR] No existe %ORIGEN%
    echo Compile primero la solucion en configuracion %CFG%.
    exit /b 1
)

if not exist "%INSTALACION%" mkdir "%INSTALACION%"

echo Copiando componente a %INSTALACION% ...
copy /y "%ORIGEN%" "%DLL%" >nul
if errorlevel 1 (
    echo [ERROR] No se pudo copiar la DLL. Cierre ClienteComSat.exe si esta abierto.
    exit /b 1
)

echo Registrando %DLL% ...
"%REGASM%" "%DLL%" /codebase /tlb
if errorlevel 1 (
    echo [ERROR] Fallo el registro.
    exit /b 1
)

echo.
echo Verificacion en el Registro de Windows:
reg query "HKCR\ServidorComSat.ValidadorSAT" /s
reg query "HKCR\CLSID\{796143f6-dba5-42cf-8230-f17df181b3e0}\InprocServer32" /s
echo.
echo Registro completado. DLL y .tlb instalados en %INSTALACION%
endlocal
