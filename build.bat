@echo off
REM ========================================
REM   Script de compilation WSL Manager
REM ========================================

title WSL Manager - Compilation

echo.
echo ========================================
echo    WSL Manager - Build Script
echo ========================================
echo.

:menu
echo Choisissez une option :
echo.
echo [1] Compiler en Debug
echo [2] Compiler en Release
echo [3] Publier (exécutable final)
echo [4] Nettoyer les fichiers de build
echo [5] Quitter
echo.
set /p choice="Votre choix (1-5): "

if "%choice%"=="1" goto debug
if "%choice%"=="2" goto release
if "%choice%"=="3" goto publish
if "%choice%"=="4" goto clean
if "%choice%"=="5" goto end
goto menu

:debug
echo.
echo ========================================
echo   Compilation en mode Debug...
echo ========================================
echo.
dotnet build -c Debug -p:Platform=x64
if %errorlevel% neq 0 (
    echo.
    echo [ERREUR] La compilation a echoue!
    pause
    goto menu
)
echo.
echo [OK] Compilation Debug reussie!
echo Executable: bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\WSL Manager.exe
echo.
pause
goto menu

:release
echo.
echo ========================================
echo   Compilation en mode Release...
echo ========================================
echo.
dotnet build -c Release -p:Platform=x64
if %errorlevel% neq 0 (
    echo.
    echo [ERREUR] La compilation a echoue!
    pause
    goto menu
)
echo.
echo [OK] Compilation Release reussie!
echo Executable: bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\WSL Manager.exe
echo.
pause
goto menu

:publish
echo.
echo ========================================
echo   Publication (self-contained)...
echo ========================================
echo.
echo [1/3] Restauration des dependances...
dotnet restore

echo.
echo [2/3] Compilation Release...
dotnet build -c Release -p:Platform=x64

echo.
echo [3/3] Publication avec ReadyToRun...
dotnet publish -c Release -r win-x64 -p:Platform=x64 --self-contained true -p:PublishReadyToRun=true -p:PublishSingleFile=false

if %errorlevel% neq 0 (
    echo.
    echo [ERREUR] La publication a echoue!
    pause
    goto menu
)

echo.
echo ========================================
echo   Publication terminee avec succes !
echo ========================================
echo.
echo Dossier: bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\
echo.
echo Fichiers crees:
dir /B "bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\WSL Manager.exe" 2>nul
echo.
echo Pour creer un ZIP de distribution :
echo   Compressez le dossier 'publish' en ZIP
echo.
pause
goto menu

:clean
echo.
echo ========================================
echo   Nettoyage des fichiers de build...
echo ========================================
echo.
if exist bin rmdir /S /Q bin
if exist obj rmdir /S /Q obj
echo [OK] Nettoyage termine !
echo.
pause
goto menu

:end
echo.
echo Au revoir !
echo.
exit /b 0
