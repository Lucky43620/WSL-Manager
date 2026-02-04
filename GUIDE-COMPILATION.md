# 🔨 Guide de Compilation - WSL Manager

Guide rapide pour compiler et distribuer WSL Manager.

---

## 📋 Table des matières

1. [Compilation Simple](#-compilation-simple)
2. [Création du Package](#-création-du-package)
3. [Distribution](#-distribution)
4. [Résolution de problèmes](#-résolution-de-problèmes)

---

## ⚡ Compilation Simple

### Option 1 : Script automatique (Recommandé)

```bash
# Double-cliquez sur build.bat
# OU lancez depuis CMD/PowerShell :
build.bat
```

Menu interactif avec les options :
- `[1]` Compiler en Debug
- `[2]` Compiler en Release
- `[3]` Publier (exécutable final)
- `[4]` Nettoyer
- `[5]` Quitter

**Choisissez l'option 3** pour créer l'exécutable final.

### Option 2 : Ligne de commande manuelle

```bash
# Compilation Release + Publication
dotnet publish -c Release -r win-x64 -p:Platform=x64 --self-contained true -p:PublishReadyToRun=true
```

L'exécutable sera dans :
```
bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/WSL Manager.exe
```

---

## 📦 Création du Package

### Étape 1 : Compiler

Utilisez `build.bat` option 3 pour publier l'application.

### Étape 2 : Créer le package ZIP

```bash
# Double-cliquez sur create-package.bat
# OU lancez :
create-package.bat
```

Le script vous demandera :
1. **Numéro de version** (ex: 1.0.0, 2.1.3)
2. **Ouvrir le dossier ?** (O/N)

### Ce que fait le script :

1. ✅ Vérifie que la compilation est faite
2. ✅ Crée un dossier `WSL-Manager-vX.X.X`
3. ✅ Copie tous les fichiers nécessaires
4. ✅ Ajoute README.md et LICENSE
5. ✅ Crée un fichier LISEZMOI.txt pour Windows
6. ✅ Compresse en ZIP

### Résultat :

```
WSL-Manager-v1.0.0.zip
├── WSL Manager.exe
├── *.dll (toutes les dépendances)
├── README.md
├── LICENSE
└── LISEZMOI.txt
```

---

## 🌐 Distribution

### Méthode 1 : GitHub Releases

1. Allez dans votre dépôt GitHub
2. Cliquez sur **Releases** → **Create a new release**
3. Entrez le tag (ex: `v1.0.0`)
4. Uploadez le fichier `WSL-Manager-v1.0.0.zip`
5. Ajoutez les notes de version
6. Cliquez sur **Publish release**

### Méthode 2 : Partage direct

Envoyez le fichier ZIP directement :
- Email
- Cloud storage (OneDrive, Google Drive)
- Site web

### Méthode 3 : Microsoft Store (Avancé)

Pour publier sur le Microsoft Store :

1. Créer un compte développeur Microsoft
2. Créer un package MSIX :
   ```bash
   dotnet build -c Release -p:GenerateAppxPackageOnBuild=true
   ```
3. Uploader le package sur le [Partner Center](https://partner.microsoft.com/)

---

## 🐛 Résolution de problèmes

### Erreur : "Le fichier projet n'existe pas"

**Problème** : Vous n'êtes pas dans le bon dossier

**Solution** :
```bash
cd "C:\Users\user4\Documents\Lucas\WSL Manager"
```

### Erreur : "Platform et RuntimeIdentifier incompatibles"

**Problème** : Plateforme non spécifiée

**Solution** :
```bash
# Ajoutez toujours -p:Platform=x64
dotnet build -c Release -p:Platform=x64
```

### Warning : "nullable reference"

Ce sont juste des warnings, pas des erreurs. L'application compile quand même.

### Le ZIP ne se crée pas

**Problème** : PowerShell non disponible ou erreur de permissions

**Solution** :
```powershell
# Créer manuellement avec PowerShell :
Compress-Archive -Path "WSL-Manager-v1.0.0\*" -DestinationPath "WSL-Manager-v1.0.0.zip"
```

### L'exécutable ne démarre pas sur un autre PC

**Problèmes possibles** :
1. Windows 10 version trop ancienne → Nécessite 19041+
2. .NET Runtime manquant → Solution : Utiliser `--self-contained true` (déjà fait)
3. WSL non installé → L'utilisateur doit installer WSL

---

## 🎯 Commandes Rapides

### Workflow complet

```bash
# 1. Nettoyer
dotnet clean

# 2. Restaurer
dotnet restore

# 3. Compiler
dotnet build -c Release -p:Platform=x64

# 4. Publier
dotnet publish -c Release -r win-x64 -p:Platform=x64 --self-contained true

# 5. Créer le package
create-package.bat
```

### OU simplement :

```bash
# Compilation
build.bat
# Choisir option 3

# Package
create-package.bat
# Entrer la version
```

---

## 📊 Tailles des fichiers

Tailles approximatives :

- **Compilation Debug** : ~30 MB
- **Compilation Release** : ~25 MB
- **Publication self-contained** : ~28 MB
- **ZIP final** : ~10-15 MB (compressé)

---

## ✅ Checklist avant distribution

- [ ] Version mise à jour dans le nom du fichier
- [ ] README.md à jour avec les nouvelles fonctionnalités
- [ ] Toutes les fonctionnalités testées
- [ ] Pas d'erreurs de compilation
- [ ] Build en Release (pas Debug)
- [ ] Self-contained (inclut .NET Runtime)
- [ ] LICENSE inclus dans le ZIP
- [ ] Notes de version écrites

---

## 🚀 Plateformes supplémentaires

Pour compiler pour ARM64 (Surface Pro X, etc.) :

```bash
dotnet publish -c Release -r win-arm64 -p:Platform=ARM64 --self-contained true
```

Pour compiler pour x86 (32-bit, rare) :

```bash
dotnet publish -c Release -r win-x86 -p:Platform=x86 --self-contained true
```

---

## 📝 Notes

- Le flag `--self-contained true` inclut .NET Runtime → **Fichier plus gros** mais **aucune dépendance à installer**
- Le flag `-p:PublishReadyToRun=true` → **Démarrage plus rapide** mais **fichier un peu plus gros**
- Le flag `-p:PublishSingleFile=true` crée un seul .exe mais peut causer des problèmes avec WinUI 3 → **Non recommandé**

---

## 🆘 Support

Si vous rencontrez des problèmes :

1. Vérifiez que vous avez **.NET SDK 8.0** installé
2. Vérifiez que vous êtes sur **Windows 10 19041+** ou **Windows 11**
3. Essayez de nettoyer : `dotnet clean` puis recompiler
4. Vérifiez les logs de compilation pour les erreurs détaillées

---

<div align="center">

**Bon build ! 🚀**

[⬆ Retour au README](README.md)

</div>
