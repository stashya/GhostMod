@echo off
echo.
echo ========================================
echo  GhostMod Build Script
echo ========================================
echo.

set "GAME_PATH=C:\Program Files (x86)\Steam\steamapps\common\Initial Drift Online"
set "PLUGIN_PATH=%GAME_PATH%\BepInEx\plugins"

dotnet build -c Release
set RESULT=%ERRORLEVEL%

echo.
echo ========================================
if %RESULT% neq 0 (
    echo  BUILD FAILED
    echo ========================================
    echo.
    echo Press any key to close...
    pause >nul
    exit /b 1
)

echo  BUILD SUCCESSFUL
echo ========================================
echo.
echo  Output: bin\Release\GhostMod.dll
echo.

if not exist "%PLUGIN_PATH%" (
    echo  BepInEx plugins folder not found!
    echo  Copy the DLL manually to your BepInEx\plugins\ folder.
) else (
    copy /y "bin\Release\GhostMod.dll" "%PLUGIN_PATH%\" >nul
    if exist "%PLUGIN_PATH%\GhostMod.dll" (
        echo  Installed: %PLUGIN_PATH%\GhostMod.dll
    ) else (
        echo  Install failed - copy DLL manually
    )
)

echo.
echo Press any key to close...
pause >nul