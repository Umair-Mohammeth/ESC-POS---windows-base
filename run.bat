@echo off
SETLOCAL
echo ============================================
echo ESC-POS Windows Forms Launcher
echo ============================================
echo Checking dependencies...

:: Check for MSBuild
where msbuild >nul 2>&1
IF ERRORLEVEL 1 (
  echo MSBuild not found. Please install Visual Studio or Build Tools to compile.
  echo Please install Visual Studio to build and run the Windows Forms app.
  goto end
)

:: Check for .NET Framework 4.8 (via registry Release key)
REG QUERY "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release >nul 2>&1
IF ERRORLEVEL 1 (
  echo .NET Framework 4.8 not detected. Please install .NET Framework 4.8.
  goto end
)

set "REL="
for /f "tokens=2,*" %%A in ('REG QUERY "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release ^| FINDSTR Release') do (
  set "REL=%%B"
)

if defined REL (
  set /a "R=%REL%"
  if %R% LSS 528040 (
    echo Detected .NET Framework Release %REL% ^(less than 528040^). Update to 4.8.
    goto end
  )
) else (
  echo Could not determine .NET Framework Release. Proceeding may fail.
)

:: Build the WinForms project
echo Building POSWinForms (Release)...
msbuild "POSWinForms.sln" /p:Configuration=Release /t:Rebuild
IF ERRORLEVEL 1 (
  echo Build failed. See the console output for details.
  goto end
)

:: Run the application if build succeeded
set "EXE_PATH=%~dp0POSWinForms\bin\Release\POSWinForms.exe"
IF EXIST "%EXE_PATH%" (
  echo Launching POSWinForms.exe...
  "%EXE_PATH%"
) ELSE (
  echo Executable not found at %EXE_PATH%
)
:end
ENDLOCAL
pause
