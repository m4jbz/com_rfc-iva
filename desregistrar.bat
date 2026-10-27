@echo off
REM ============================================================
REM  Desregistro del servidor COM ServidorComSat (ejecutar como ADMINISTRADOR)
REM  Uso: desregistrar.bat [Debug|Release]   (por defecto Release)
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
    exit /b 1
)

echo Desregistrando %DLL% ...
"%REGASM%" "%DLL%" /unregister /tlb
if errorlevel 1 (
    echo [ERROR] Fallo el desregistro.
    exit /b 1
)

echo.
echo Comprobando que la clave ya no existe:
reg query "HKCR\ServidorComSat.ValidadorSAT" >nul 2>&1 && (echo La clave AUN existe.) || (echo Clave eliminada correctamente.)
endlocal
