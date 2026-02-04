# Script pour redimensionner le logo en différentes tailles pour l'application
# Utilise System.Drawing pour manipuler les images

Add-Type -AssemblyName System.Drawing

$sourceImage = "Assets\AppLogo.png"
$assetsFolder = "Assets"

# Vérifie que le fichier source existe
if (-not (Test-Path $sourceImage)) {
    Write-Error "Le fichier source $sourceImage n'existe pas!"
    exit 1
}

# Fonction pour redimensionner une image
function Resize-Image {
    param(
        [string]$InputPath,
        [string]$OutputPath,
        [int]$Width,
        [int]$Height
    )

    Write-Host "Création de $OutputPath ($Width x $Height)"

    # Charge l'image source
    $image = [System.Drawing.Image]::FromFile((Resolve-Path $InputPath).Path)

    # Crée une nouvelle bitmap avec la taille cible
    $newImage = New-Object System.Drawing.Bitmap $Width, $Height

    # Crée un objet Graphics pour dessiner
    $graphics = [System.Drawing.Graphics]::FromImage($newImage)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    # Dessine l'image redimensionnée
    $graphics.DrawImage($image, 0, 0, $Width, $Height)

    # Sauvegarde l'image
    $newImage.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Libère les ressources
    $graphics.Dispose()
    $newImage.Dispose()
    $image.Dispose()
}

# Crée les différentes tailles d'images
Write-Host "Redimensionnement du logo en cours..."

# Square44x44Logo - Icône de la barre des tâches
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\Square44x44Logo.scale-200.png" -Width 88 -Height 88

# Square150x150Logo - Tuile moyenne
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\Square150x150Logo.scale-200.png" -Width 300 -Height 300

# Wide310x150Logo - Tuile large
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\Wide310x150Logo.scale-200.png" -Width 620 -Height 300

# StoreLogo - Logo du store
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\StoreLogo.png" -Width 50 -Height 50

# SplashScreen - Écran de démarrage
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\SplashScreen.scale-200.png" -Width 620 -Height 300

# Square44x44Logo.targetsize-24 - Petite icône
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\Square44x44Logo.targetsize-24_altform-unplated.png" -Width 24 -Height 24

# LockScreenLogo - Logo de l'écran de verrouillage
Resize-Image -InputPath $sourceImage -OutputPath "$assetsFolder\LockScreenLogo.scale-200.png" -Width 48 -Height 48

Write-Host "Redimensionnement terminé!"
Write-Host "Les images ont été créées dans le dossier $assetsFolder"
