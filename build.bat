@echo off
echo.
echo ========================================
echo  GhostMod Build Script
echo ========================================
echo.

dotnet build -c Release
set RESULT=%ERRORLEVEL%

echo.
echo ========================================
if %RESULT%==0 (
    echo  BUILD SUCCESSFUL
    echo ========================================
    echo.
    echo  Output: bin\Release\GhostMod.dll
    echo.
    copy /y "bin\Release\GhostMod.dll" "C:\Program Files (x86)\Steam\steamapps\common\Initial Drift Online\BepInEx\plugins\GhostMod\" >nul 2>nul
    if %ERRORLEVEL%==0 (
        echo  Installed to BepInEx\plugins\GhostMod\
    ) else (
        echo  Auto-install failed - copy DLL manually
    )
) else (
    echo  BUILD FAILED
    echo ========================================
)
echo.
echo Press any key to close...
pause >nul