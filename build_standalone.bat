@echo off
SETLOCAL EnableDelayedExpansion

echo ============================================
echo ESC-POS Standalone (.exe) Builder
echo ============================================

:: Check for .NET Framework 4.8
REG QUERY "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release | findstr /I "release" >nul
if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] .NET Framework 4.8 not detected.
    echo Please install it from: https://dotnet.microsoft.com/download/dotnet-framework/net48
)

:: Find MSBuild
set "MSBUILD_PATH="
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if exist "!VSWHERE!" (
    for /f "usebackq tokens=*" %%i in (`"!VSWHERE!" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
      set "MSBUILD_PATH=%%i"
    )
)

IF NOT EXIST "!MSBUILD_PATH!" (
    echo [ERROR] MSBuild was not found. This is required to build the standalone EXE.
    echo.
    set /p "INSTALL=Would you like to download the Visual Studio Build Tools bootstrapper? (Y/N): "
    if /I "!INSTALL!"=="Y" (
        echo Downloading vs_buildtools.exe...
        powershell -Command "Invoke-WebRequest -Uri 'https://aka.ms/vs/17/release/vs_buildtools.exe' -OutFile 'vs_buildtools.exe'"
        if exist "vs_buildtools.exe" (
            echo Launching installer...
            echo Please ensure you select the '.NET desktop build tools' workload.
            start vs_buildtools.exe --passive --wait --add Microsoft.VisualStudio.Workload.MSBuildTools --add Microsoft.VisualStudio.Workload.NetCoreBuildTools
            echo After installation is complete, please run this script again.
        ) else (
            echo [ERROR] Download failed.
        )
    )
    goto end
)

echo Using MSBuild at: !MSBUILD_PATH!
echo Restoring packages and building standalone executable for Windows...
echo.

cd /d "%~dp0"

:: Restore Costura.Fody
"!MSBUILD_PATH!" "POSWinForms.sln" /t:Restore
IF ERRORLEVEL 1 (
    echo [ERROR] Package restore failed.
    goto end
)

:: Publish the application as a single executable using Costura.Fody
"!MSBUILD_PATH!" "POSWinForms.sln" /p:Configuration=Release /t:Rebuild
IF ERRORLEVEL 1 (
    echo.
    echo [ERROR] Build failed. Please check the output above.
    goto end
)

echo.
echo ============================================
echo Build Successful!
echo Copying standalone files to: %~dp0Standalone
echo ============================================

:: Create Standalone folder if it doesn't exist
if not exist "%~dp0Standalone" mkdir "%~dp0Standalone"

:: Copy the bundled .exe and necessary native Interop folders
copy "%~dp0POSWinForms\bin\Release\net48\POSWinForms.exe" "%~dp0Standalone\POSWinForms.exe" /Y
copy "%~dp0POSWinForms\bin\Release\net48\POSWinForms.exe.config" "%~dp0Standalone\POSWinForms.exe.config" /Y
xcopy "%~dp0POSWinForms\bin\Release\net48\x86" "%~dp0Standalone\x86\" /S /E /I /Y
xcopy "%~dp0POSWinForms\bin\Release\net48\x64" "%~dp0Standalone\x64\" /S /E /I /Y

echo.
echo Your standalone application is ready in: %~dp0Standalone\
echo ============================================

:end
ENDLOCAL
