@echo off
setlocal
rem The Update Catalog package requires the WebView2 app GUID.
rem Run as administrator on the target Win7 SP1 machine, not the build machine.
if not exist "%~dp0MicrosoftEdgeStandaloneInstallerX86.exe" (
    echo Missing MicrosoftEdgeStandaloneInstallerX86.exe
    exit /b 1
)
"%~dp0MicrosoftEdgeStandaloneInstallerX86.exe" /installsource windowsupdate /install "appguid={F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}&needsadmin=true"
set "result=%errorlevel%"
echo Installer exit code: %result%
echo Verify WebView2 Runtime 109.0.1518.140 in Programs and Features.
exit /b %result%
