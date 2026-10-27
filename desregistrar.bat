@echo off
REM ============================================================
REM  Desregistro y desinstalacion del servidor COM ServidorComSat
REM  (ejecutar como ADMINISTRADOR)
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
set INSTALACION=%ProgramW6432%\ComSat
set DLL=%INSTALACION%\ServidorComSat.dll

REM Si no esta instalada, se intenta con la copia de la carpeta bin.
if not exist "%DLL%" set DLL=%~dp0ServidorComSat\bin\%CFG%\ServidorComSat.dll

if not exist "%DLL%" (
    echo [ERROR] No se encontro ServidorComSat.dll ni en %INSTALACION% ni en bin\%CFG%.
    exit /b 1
)

echo Desregistrando %DLL% ...
"%REGASM%" "%DLL%" /unregister /tlb
if errorlevel 1 (
    echo [ERROR] Fallo el desregistro.
    exit /b 1
)

if exist "%INSTALACION%" (
    echo Eliminando %INSTALACION% ...
    rmdir /s /q "%INSTALACION%"
)

echo.
echo Comprobando que la clave ya no existe:
reg query "HKCR\ServidorComSat.ValidadorSAT" >nul 2>&1 && (echo La clave AUN existe.) || (echo Clave eliminada correctamente.)
endlocal
