@echo off

set "BAT_DIR=%~dp0"
cd /d "%BAT_DIR%"

rem create program
dotnet new console --force
del Program.cs

rem run program
dotnet run SampleUsage.cs

rem create program finish
cd ..
