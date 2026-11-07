@echo off
chcp 65001 >nul
cd /d "%~dp0"

if not exist "bin\Release\net48\Dolunay2Launcher.exe" (
    echo Once build.bat dosyasini calistirin.
    pause
    exit /b 1
)

cd /d "bin\Release\net48"
echo Server=http://localhost:3002> patcher.ini
start "" Dolunay2Launcher.exe
