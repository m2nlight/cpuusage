@echo off
pushd "%~dp0"
set csfile=cpuusage.cs
set PATH="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\Roslyn";"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\bin\Roslyn";"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\Roslyn";"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\BIN\amd64";"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\BIN";C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319;C:\WINDOWS\Microsoft.NET\Framework64\v3.5;C:\WINDOWS\Microsoft.NET\Framework64\v2.0;%PATH%
csc.exe /target:exe /unsafe+ %csfile%
if ERRORLEVEL 1 (echo ERROR: %ERRORLEVEL%) else (echo Build Success)
echo.
echo Press any key to EXIT...
pause>nul
popd