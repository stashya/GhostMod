@echo off
setlocal

:: ============================================
:: GhostMod Build Script
:: ============================================
:: 
:: Usage:
::   build.bat           - Build Release and install to game
::   build.bat debug     - Build Debug version
::   build.bat release   - Build Release version  
::   build.bat install   - Build Release and install to game
::
:: If your game is not in the default Steam location, edit
:: the GamePath in GhostMod.csproj
:: ============================================

set CONFIG=Release
set INSTALL=0

:: Parse arguments
if /i "%1"=="debug" set CONFIG=Debug
if /i "%1"=="release" set CONFIG=Release
if /i "%1"=="install" (
    set CONFIG=Release
    set INSTALL=1
)
if "%1"=="" set INSTALL=1

:: Default game path (must match .csproj)
set "GAME_PATH=C:\Program Files (x86)\Steam\steamapps\common\Initial Drift Online"
set "PLUGIN_PATH=%GAME_PATH%\BepInEx\plugins\GhostMod"

echo.
echo ========================================
echo  GhostMod Build Script
echo ========================================
echo  Configuration: %CONFIG%
echo ========================================
echo.

:: Check if dotnet is available
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: dotnet SDK not found!
    echo Please install .NET SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

:: Build
echo Building GhostMod...
dotnet build -c %CONFIG%

if %ERRORLEVEL% neq 0 (
    echo.
    echo BUILD FAILED!
    echo.
    echo Common issues:
    echo  - Game not installed at default Steam path
    echo  - BepInEx not installed in game folder
    echo  - Edit GamePath in GhostMod.csproj if game is elsewhere
    pause
    exit /b 1
)

echo.
echo BUILD SUCCESSFUL!
echo Output: bin\%CONFIG%\GhostMod.dll

:: Install if requested
if %INSTALL%==1 (
    echo.
    echo Installing to game...
    
    if not exist "%GAME_PATH%" (
        echo.
        echo WARNING: Game folder not found at:
        echo %GAME_PATH%
        echo.
        echo The DLL was built but not installed.
        echo Manually copy bin\%CONFIG%\GhostMod.dll to your BepInEx\plugins folder.
        pause
        exit /b 0
    )
    
    if not exist "%PLUGIN_PATH%" mkdir "%PLUGIN_PATH%"
    copy /y "bin\%CONFIG%\GhostMod.dll" "%PLUGIN_PATH%\" >nul
    
    echo.
    echo Installed to: %PLUGIN_PATH%\GhostMod.dll
)

echo.
echo Done!
pause
