@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo [1/3] Launcher derleniyor...
dotnet build -c Release
if errorlevel 1 (
    echo.
    echo DERLEME BASARISIZ.
    pause
    exit /b 1
)

echo [2/3] Panel klasorune kopyalaniyor...
copy /y "bin\Release\net48\Dolunay2Launcher.exe" "..\public\patcher\Dolunay2Launcher.exe" >nul
if errorlevel 1 (
    echo Kopyalama basarisiz.
    pause
    exit /b 1
)

echo [3/3] Tamamlandi.
echo Cikti : bin\Release\net48\Dolunay2Launcher.exe
echo Yayin : public\patcher\Dolunay2Launcher.exe
echo.
pause
