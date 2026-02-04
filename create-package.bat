@echo off
REM ========================================
REM   Création du package de distribution
REM ========================================

title WSL Manager - Package Creator

setlocal enabledelayedexpansion

echo.
echo ========================================
echo    WSL Manager - Package Creator
echo ========================================
echo.

REM Définir la version
set /p VERSION="Entrez le numero de version (ex: 1.0.0): "
if "%VERSION%"=="" set VERSION=1.0.0

echo.
echo Version: %VERSION%
echo.

REM Dossiers
set PUBLISH_DIR=bin\Release\net8.0-windows10.0.19041.0\win-x64\publish
set PACKAGE_DIR=WSL-Manager-v%VERSION%
set ZIP_NAME=WSL-Manager-v%VERSION%.zip

echo [1/5] Verification du dossier de publication...
if not exist "%PUBLISH_DIR%\WSL Manager.exe" (
    echo.
    echo [ERREUR] Le dossier de publication n'existe pas !
    echo Veuillez d'abord publier le projet avec build.bat
    pause
    exit /b 1
)

echo [2/5] Creation du dossier de package...
if exist "%PACKAGE_DIR%" rmdir /S /Q "%PACKAGE_DIR%"
mkdir "%PACKAGE_DIR%"

echo [3/5] Copie des fichiers...
xcopy "%PUBLISH_DIR%\*" "%PACKAGE_DIR%\" /E /I /H /Y > nul

echo [4/5] Ajout des fichiers de documentation...
copy README.md "%PACKAGE_DIR%\" > nul
copy LICENSE "%PACKAGE_DIR%\" > nul

REM Créer un fichier README.txt pour Windows
echo ========================================> "%PACKAGE_DIR%\LISEZMOI.txt"
echo    WSL Manager v%VERSION%>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo ========================================>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo.>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo Pour lancer l'application :>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo.>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo 1. Double-cliquez sur "WSL Manager.exe">> "%PACKAGE_DIR%\LISEZMOI.txt"
echo 2. Si Windows SmartScreen s'affiche :>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo    - Cliquez sur "Informations complementaires">> "%PACKAGE_DIR%\LISEZMOI.txt"
echo    - Puis sur "Executer quand meme">> "%PACKAGE_DIR%\LISEZMOI.txt"
echo.>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo Configuration requise :>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo - Windows 10 (19041+) ou Windows 11>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo - WSL installe (wsl --install)>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo.>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo Pour plus d'informations, consultez README.md>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo.>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo ========================================>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo    (c) 2026 Lucas - Licence MIT>> "%PACKAGE_DIR%\LISEZMOI.txt"
echo ========================================>> "%PACKAGE_DIR%\LISEZMOI.txt"

echo [5/5] Creation de l'archive ZIP...
if exist "%ZIP_NAME%" del "%ZIP_NAME%"

powershell -Command "Compress-Archive -Path '%PACKAGE_DIR%\*' -DestinationPath '%ZIP_NAME%' -CompressionLevel Optimal"

if %errorlevel% neq 0 (
    echo.
    echo [ERREUR] La creation du ZIP a echoue!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    Package cree avec succes !
echo ========================================
echo.
echo Fichier: %ZIP_NAME%
echo Taille:
for %%A in ("%ZIP_NAME%") do echo %%~zA octets
echo.
echo Contenu du package:
dir /B "%PACKAGE_DIR%"
echo.
echo Le package est pret a etre distribue !
echo.

REM Demander si on veut ouvrir le dossier
set /p OPEN="Voulez-vous ouvrir le dossier ? (O/N): "
if /i "%OPEN%"=="O" explorer .

pause
