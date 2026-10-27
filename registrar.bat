@echo off
REM ============================================================
REM  Registro del servidor COM ServidorComSat (ejecutar como ADMINISTRADOR)
REM  Uso: registrar.bat [Debug|Release]   (por defecto Release)
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
set DLL=%~dp0ServidorComSat\bin\%CFG%\ServidorComSat.dll

if not exist "%DLL%" (
    echo [ERROR] No existe %DLL%
    echo Compile primero la solucion en configuracion %CFG%.
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
echo Registro completado. Se genero ServidorComSat.tlb junto a la DLL.
endlocal
