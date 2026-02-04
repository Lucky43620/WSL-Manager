# 🐧 WSL Manager

> Application Windows moderne et professionnelle pour gérer vos distributions WSL (Windows Subsystem for Linux) en toute simplicité.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![WinUI](https://img.shields.io/badge/WinUI-3-brightgreen)](https://microsoft.github.io/microsoft-ui-xaml/)
[![Platform](https://img.shields.io/badge/platform-Windows%2011-blue)](https://www.microsoft.com/windows)

---

## ✨ Aperçu

WSL Manager est une application native Windows qui vous permet de gérer toutes vos distributions Linux depuis une interface graphique élégante, sans jamais toucher à la ligne de commande.

**Pourquoi WSL Manager ?**
- 🎨 Interface ultra moderne avec effet Mica (Windows 11)
- ⚡ Actions rapides et intuitives
- 🔔 Notifications pour chaque opération
- 🎯 Design professionnel et friendly
- 📊 Vue d'ensemble claire de toutes vos distributions

---

## 🎯 Fonctionnalités

### 📋 Gestion des Distributions

- ✅ **Liste complète** de toutes vos distributions WSL
- ✅ **État en temps réel** : Running (🟢) ou Stopped (⚪)
- ✅ **Badge "Par défaut"** pour la distribution principale
- ✅ **Version WSL** affichée (WSL 1 ou WSL 2)
- ✅ **Rafraîchissement** automatique de la liste

### ⚡ Actions Rapides

#### Actions Principales
- **▶️ Démarrer** : Lance une distribution arrêtée
- **⏹️ Arrêter** : Arrête proprement une distribution
- **🔄 Redémarrer** : Redémarre une distribution en cours
- **📟 Terminal** : Ouvre Windows Terminal (ou cmd) dans la distribution
- **📁 Explorateur** : Ouvre l'explorateur Windows (`\\wsl$\nom`)

#### Actions Avancées (Menu ⋯)
- **⭐ Définir par défaut** : Change la distribution par défaut WSL
- **ℹ️ Informations** : Affiche les détails système (uname -a)

### 🔔 Notifications Intelligentes

Chaque action déclenche une notification claire :

- **🟢 Succès** : "La distribution 'Ubuntu' a été démarrée avec succès"
- **🔴 Erreur** : "Impossible de démarrer la distribution"
- **🔵 Info** : "Redémarrage en cours..."
- **🟡 Avertissement** : Messages contextuels

### 🎨 Interface Moderne

- **Cartes élégantes** pour chaque distribution
- **Animations fluides** au survol (effet hover subtil)
- **Ombres portées** pour un effet de profondeur
- **État vide friendly** quand aucune distribution n'est trouvée
- **Effet Mica** : Transparence moderne Windows 11
- **Mode clair/sombre** : Suit automatiquement le thème système

---

## 🏗️ Architecture

Ce projet utilise le **pattern MVVM** (Model-View-ViewModel) pour une architecture propre et maintenable.

```
WSL Manager/
├── Models/              → Classes de données
│   └── WslDistribution.cs          # Représente une distribution WSL
│
├── ViewModels/          → Logique métier
│   ├── ViewModelBase.cs            # Classe de base avec INotifyPropertyChanged
│   └── MainViewModel.cs            # ViewModel principal (8 commandes)
│
├── Services/            → Services métier
│   ├── WslService.cs               # Interaction avec WSL (wsl.exe)
│   └── NotificationService.cs      # Système de notifications centralisé
│
├── Helpers/             → Classes utilitaires
│   └── RelayCommand.cs             # Implémentation ICommand pour MVVM
│
├── Converters/          → Convertisseurs XAML
│   ├── BoolToVisibilityConverter.cs     # bool → Visibility
│   └── StateToColorConverter.cs         # bool → Couleur (vert/gris)
│
├── MainWindow.xaml      → Interface utilisateur
└── App.xaml            → Configuration application
```

### 🎯 Pattern MVVM

**Model** → Données brutes (WslDistribution)
```csharp
public class WslDistribution
{
    public string Name { get; set; }
    public string State { get; set; }
    public int Version { get; set; }
    public bool IsDefault { get; set; }
    public bool IsRunning => State == "Running";
}
```

**View** → Interface XAML avec binding
```xaml
<ListView ItemsSource="{Binding Distributions}">
```

**ViewModel** → Logique métier
```csharp
public class MainViewModel
{
    public ObservableCollection<WslDistribution> Distributions { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    // ... 6 autres commandes
}
```

---

## 🚀 Installation & Utilisation

### Prérequis

- **Windows 10** (version 19041+) ou **Windows 11**
- **WSL** installé (`wsl --install`)
- **.NET 8 SDK** ([Télécharger](https://dotnet.microsoft.com/download))
- **Visual Studio 2022** (recommandé) ou VS Code

### Compiler le Projet

```bash
# Cloner le repository (si git)
git clone https://github.com/votre-repo/wsl-manager.git
cd wsl-manager

# Compiler avec .NET CLI
dotnet build "WSL Manager.csproj" -p:Platform=x64

# Ou ouvrir dans Visual Studio
# WSL Manager.slnx → Sélectionner x64 → F5
```

### Exécuter l'Application

```bash
# Via .NET CLI
cd "bin/x64/Debug/net8.0-windows10.0.19041.0/win-x64/"
./WSL Manager.exe

# Ou via Visual Studio
# Appuyez sur F5 (Démarrer)
```

---

## 📖 Guide d'Utilisation

### Démarrer une Distribution

1. Trouvez la carte de votre distribution (ex: Ubuntu)
2. Cliquez sur le bouton **▶️ Démarrer**
3. Une notification confirme le démarrage
4. L'indicateur passe au 🟢 vert

### Ouvrir un Terminal

1. Sur la carte de la distribution souhaitée
2. Cliquez sur **📟 Terminal**
3. Windows Terminal s'ouvre automatiquement
   - Si Windows Terminal n'est pas installé, cmd.exe est utilisé

### Accéder aux Fichiers Linux

1. Sur la carte de la distribution
2. Cliquez sur **📁 Explorateur**
3. L'Explorateur Windows s'ouvre sur `\\wsl$\nom-distribution`
4. Vous pouvez glisser-déposer des fichiers entre Windows et Linux !

### Changer la Distribution par Défaut

1. Cliquez sur **⋯ Plus d'options**
2. Sélectionnez **"Définir par défaut"**
3. Le badge "Par défaut" se déplace sur cette distribution
4. La commande `wsl` utilisera maintenant cette distribution

---

## 🔧 Configuration

### Structure du Code

#### WslService.cs

Service principal pour interagir avec WSL :

```csharp
// Liste les distributions
Task<List<WslDistribution>> GetDistributionsAsync()

// Démarre une distribution
Task<bool> StartDistributionAsync(string name)

// Arrête une distribution
Task<bool> StopDistributionAsync(string name)

// Redémarre (arrêt + démarrage)
Task<bool> RestartDistributionAsync(string name)

// Ouvre un terminal
void OpenTerminal(string name)

// Ouvre l'explorateur
void OpenFileExplorer(string name)

// Définit par défaut
Task<bool> SetDefaultDistributionAsync(string name)

// Récupère les infos système
Task<string> GetDistributionInfoAsync(string name)
```

#### NotificationService.cs

Service de notifications singleton :

```csharp
// Instance unique
NotificationService.Instance

// Afficher une notification
ShowInfo(string message, string? title = null)
ShowSuccess(string message, string? title = null)
ShowWarning(string message, string? title = null)
ShowError(string message, string? title = null)
```

#### MainViewModel.cs

ViewModel principal avec toutes les commandes :

```csharp
// Propriétés
ObservableCollection<WslDistribution> Distributions
bool IsLoading
bool IsEmptyState
bool HasDistributions

// Commandes
ICommand RefreshCommand          // Rafraîchir la liste
ICommand StartCommand            // Démarrer
ICommand StopCommand             // Arrêter
ICommand RestartCommand          // Redémarrer
ICommand OpenTerminalCommand     // Terminal
ICommand OpenExplorerCommand     // Explorateur
ICommand SetDefaultCommand       // Définir par défaut
ICommand ShowInfoCommand         // Informations
```

---

## 📝 Changelog

### Version 2.0 (Phase 2) - Interface Ultra Pro ✨

**🎨 Design Complet Redesigné**
- Interface refaite avec cartes modernes et ombres
- Animations au survol pour un effet premium
- État vide avec message friendly et lien documentation
- Effet Mica pour transparence Windows 11
- Indicateurs d'état avec points colorés et ombres

**🔔 Système de Notifications**
- Service de notifications centralisé (singleton)
- InfoBar intégrée en haut de l'application
- 4 types : Info, Succès, Warning, Erreur
- Feedback immédiat pour toutes les opérations
- Messages clairs et contextuels

**⚡ Nouvelles Fonctionnalités**
- 🔄 Redémarrage de distributions
- 📟 Ouverture de terminal (Windows Terminal + fallback cmd)
- 📁 Ouverture de l'explorateur Windows (`\\wsl$\`)
- ⭐ Définir distribution par défaut
- ℹ️ Affichage informations système (uname)
- Menu "Plus d'options" avec actions avancées

**🏗️ Architecture Améliorée**
- NotificationService.cs ajouté
- WslService.cs étendu (8 méthodes)
- MainViewModel.cs : 8 commandes au total
- Commentaires XML exhaustifs sur toutes les méthodes
- Gestion d'erreurs complète avec try-catch
- Null safety : vérifications systématiques

**🐛 Corrections**
- Animation au survol corrigée (opacité au lieu de scale)
- Gestion des erreurs améliorée
- Thread UI : DispatcherQueue pour notifications

### Version 1.0 (Phase 1) - Foundation

**✅ Fonctionnalités de Base**
- Liste des distributions WSL
- Affichage de l'état (Running/Stopped)
- Version WSL (1 ou 2)
- Badge "Par défaut"
- Démarrer une distribution
- Arrêter une distribution
- Rafraîchir la liste

**🏗️ Architecture MVVM**
- Models : WslDistribution
- ViewModels : ViewModelBase, MainViewModel
- Services : WslService
- Helpers : RelayCommand
- Converters : BoolToVisibility, StateToColor

---

## 🎓 Technologies Utilisées

| Technologie | Version | Usage |
|-------------|---------|-------|
| **.NET** | 8.0 | Framework principal |
| **WinUI 3** | 1.8 | Interface utilisateur moderne |
| **Windows App SDK** | 1.8 | APIs Windows natives |
| **C#** | 12.0 | Langage de programmation |
| **XAML** | - | Markup pour l'interface |

### Packages NuGet

```xml
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.26100.7463" />
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260101001" />
```

---

## 🔍 Résolution de Problèmes

### "Aucune distribution WSL trouvée"

**Problème** : WSL n'est pas installé ou aucune distribution installée

**Solution** :
```powershell
# Installer WSL avec Ubuntu par défaut
wsl --install

# Ou installer une distribution spécifique
wsl --install -d Debian
wsl --install -d Ubuntu-22.04
```

### Le Terminal ne s'ouvre pas

**Problème** : Windows Terminal n'est pas installé

**Solution** :
- L'application utilise automatiquement `cmd.exe` en fallback
- Ou installez Windows Terminal depuis le [Microsoft Store](https://aka.ms/terminal)

### Erreur de Compilation

**Problème** : `error : Packaged .NET applications ... cannot be ProcessorArchitecture neutral`

**Solution** :
```bash
# Compiler avec une plateforme spécifique
dotnet build -p:Platform=x64
# Ou x86, ARM64
```

### La Distribution ne Démarre pas

**Problème** : Distribution corrompue ou erreur WSL

**Solution** :
1. Essayez le bouton **🔄 Redémarrer**
2. Redémarrez Windows
3. En dernier recours :
```powershell
# Arrêter WSL complètement
wsl --shutdown

# Puis redémarrer la distribution
wsl -d Ubuntu
```

---

## 🎨 Personnalisation

### Modifier les Couleurs

Les couleurs sont définies via les ressources XAML :

```xaml
<!-- Dans App.xaml -->
<SolidColorBrush x:Key="CustomAccentColor" Color="#0078D4"/>
```

### Ajouter une Nouvelle Action

1. **Ajouter la méthode dans WslService.cs** :
```csharp
public async Task<bool> NouvelleFonction(string name)
{
    // Votre logique
}
```

2. **Ajouter la commande dans MainViewModel.cs** :
```csharp
public ICommand NouvelleCommande { get; }

// Dans le constructeur
NouvelleCommande = new RelayCommand<WslDistribution>(async (dist) =>
    await ExecuterNouvelleFonction(dist));
```

3. **Ajouter le bouton dans MainWindow.xaml** :
```xaml
<Button Command="{Binding NouvelleCommande}"
        CommandParameter="{Binding}">
    <SymbolIcon Symbol="VotreIcone"/>
</Button>
```

---

## 🚀 Roadmap (Phase 3 - À venir)

### Gestion Avancée
- [ ] Import de distributions (.tar, .tar.gz)
- [ ] Export de distributions
- [ ] Suppression de distributions (avec confirmation)
- [ ] Conversion WSL 1 ↔ WSL 2
- [ ] Configuration mémoire/CPU par distribution

### Monitoring
- [ ] Utilisation mémoire en temps réel
- [ ] Utilisation CPU
- [ ] Espace disque utilisé
- [ ] Graphiques de performance
- [ ] Historique d'utilisation

### Configuration
- [ ] Page de paramètres
- [ ] Thème personnalisable (clair/sombre/auto)
- [ ] Choix du terminal par défaut
- [ ] Raccourcis clavier personnalisables
- [ ] Auto-démarrage avec Windows

### Fonctionnalités Pro
- [ ] Scripts de démarrage automatiques
- [ ] Snapshots/Backups de distributions
- [ ] Profils de configuration
- [ ] Multi-sélection et actions groupées
- [ ] Recherche/Filtrage de distributions

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Si vous souhaitez améliorer WSL Manager :

1. Forkez le projet
2. Créez une branche (`git checkout -b feature/NouvelleFonctionnalité`)
3. Committez vos changements (`git commit -m 'Ajout nouvelle fonctionnalité'`)
4. Pushez vers la branche (`git push origin feature/NouvelleFonctionnalité`)
5. Ouvrez une Pull Request

### Standards de Code

- **Commentaires XML** sur toutes les méthodes publiques
- **Gestion d'erreurs** complète avec try-catch
- **Notifications** pour feedback utilisateur
- **Null safety** : vérifications systématiques
- **Nommage** : Conventions C# standards

---

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

---

## 📚 Ressources

### Documentation Officielle

- [Documentation WSL](https://learn.microsoft.com/windows/wsl/)
- [WinUI 3 Docs](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [Pattern MVVM](https://learn.microsoft.com/dotnet/architecture/maui/mvvm)
- [C# Documentation](https://learn.microsoft.com/dotnet/csharp/)

### Commandes WSL Utiles

```powershell
# Lister les distributions
wsl --list --verbose
wsl -l -v

# Démarrer une distribution
wsl -d Ubuntu

# Arrêter une distribution
wsl --terminate Ubuntu

# Arrêter toutes les distributions
wsl --shutdown

# Définir par défaut
wsl --set-default Ubuntu

# Mettre à jour WSL
wsl --update

# Voir la version WSL
wsl --version

# Importer une distribution
wsl --import <Nom> <Emplacement> <Fichier.tar>

# Exporter une distribution
wsl --export <Nom> <Fichier.tar>

# Désinstaller une distribution
wsl --unregister Ubuntu
```

### Communauté

- [Reddit r/bashonubuntuonwindows](https://reddit.com/r/bashonubuntuonwindows)
- [WSL GitHub Issues](https://github.com/microsoft/WSL/issues)
- [Stack Overflow - WSL](https://stackoverflow.com/questions/tagged/wsl)

---

## 👨‍💻 Auteur

Développé avec ❤️ pour la communauté WSL

**Technologies** : .NET 8, WinUI 3, C# 12, XAML
**Pattern** : MVVM (Model-View-ViewModel)
**Compatibilité** : Windows 10 (19041+) / Windows 11

---

## 🌟 Remerciements

- **Microsoft** pour WSL et WinUI 3
- **Communauté .NET** pour les ressources et support
- **Vous** pour utiliser WSL Manager ! 🎉

---

<div align="center">

**[⬆ Retour en haut](#-wsl-manager)**

*WSL Manager - Gérez vos distributions Linux avec style* ✨🐧

</div>
