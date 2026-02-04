using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WSL_Manager.Models;
using WSL_Manager.Services;
using WSL_Manager.ViewModels;

namespace WSL_Manager
{
    /// <summary>
    /// Fenêtre principale de l'application WSL Manager
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// ViewModel de la fenêtre principale
        /// </summary>
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();

            // Définit le DataContext pour le binding
            RootGrid.DataContext = ViewModel;

            // Configure la fenêtre
            Title = "WSL Manager";
            ExtendsContentIntoTitleBar = true;

            // Définit l'icône de la fenêtre
            SetWindowIcon();

            // S'abonne aux notifications
            NotificationService.Instance.NotificationRequested += OnNotificationRequested;
        }

        /// <summary>
        /// Définit l'icône de la fenêtre
        /// </summary>
        private void SetWindowIcon()
        {
            try
            {
                var iconPath = System.IO.Path.Combine(
                    Windows.ApplicationModel.Package.Current.InstalledLocation.Path,
                    "Assets",
                    "Square44x44Logo.targetsize-24_altform-unplated.png"
                );

                if (System.IO.File.Exists(iconPath))
                {
                    AppWindow.SetIcon(iconPath);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la définition de l'icône: {ex.Message}");
            }
        }

        /// <summary>
        /// Gère l'affichage des notifications dans l'InfoBar
        /// </summary>
        private void OnNotificationRequested(object? sender, NotificationEventArgs e)
        {
            // Doit être appelé sur le thread UI
            DispatcherQueue.TryEnqueue(() =>
            {
                NotificationBar.Title = e.Title ?? string.Empty;
                NotificationBar.Message = e.Message;
                NotificationBar.Severity = e.Type switch
                {
                    NotificationType.Success => InfoBarSeverity.Success,
                    NotificationType.Warning => InfoBarSeverity.Warning,
                    NotificationType.Error => InfoBarSeverity.Error,
                    _ => InfoBarSeverity.Informational
                };
                NotificationBar.IsOpen = true;
            });
        }

        /// <summary>
        /// Animation au survol de la carte - applique un effet visuel subtil
        /// Change l'apparence de la bordure et l'opacité pour un effet d'élévation
        /// </summary>
        private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // Effet d'élévation visuel via opacité et épaisseur de bordure
                var storyboard = new Storyboard();

                // Animation d'opacité (léger highlight)
                var opacityAnimation = new DoubleAnimation
                {
                    To = 0.95,
                    Duration = TimeSpan.FromMilliseconds(150),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(opacityAnimation, border);
                Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
                storyboard.Children.Add(opacityAnimation);

                storyboard.Begin();
            }
        }

        /// <summary>
        /// Animation quand le curseur quitte la carte - retour à l'état normal
        /// </summary>
        private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                var storyboard = new Storyboard();

                // Retour à l'opacité normale
                var opacityAnimation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(150),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(opacityAnimation, border);
                Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
                storyboard.Children.Add(opacityAnimation);

                storyboard.Begin();
            }
        }

        /// <summary>
        /// Event handler pour "Voir les informations"
        /// </summary>
        private void ShowInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"ShowInfo_Click pour {distribution.Name}");
                ViewModel.ShowInfoCommand.Execute(distribution);
            }
        }

        /// <summary>
        /// Event handler pour "Définir par défaut"
        /// </summary>
        private void SetDefault_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"SetDefault_Click pour {distribution.Name}");
                ViewModel.SetDefaultCommand.Execute(distribution);
            }
        }

        /// <summary>
        /// Event handler pour "Convertir version"
        /// </summary>
        private void ConvertVersion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"ConvertVersion_Click pour {distribution.Name}");
                ViewModel.ConvertVersionCommand.Execute(distribution);
            }
        }

        /// <summary>
        /// Event handler pour "Mettre à jour l'instance"
        /// </summary>
        private void UpdateInstance_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateInstance_Click pour {distribution.Name}");
                ViewModel.UpdateInstanceCommand.Execute(distribution);
            }
        }

        /// <summary>
        /// Event handler pour "Exporter"
        /// </summary>
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"Export_Click pour {distribution.Name}");
                ViewModel.ExportCommand.Execute(distribution);
            }
        }

        /// <summary>
        /// Event handler pour "Supprimer"
        /// </summary>
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"Delete_Click pour {distribution.Name}");

                // Dialogue de confirmation
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Confirmer la suppression",
                    Content = $"Êtes-vous sûr de vouloir supprimer '{distribution.Name}' ?\n\nCette action est IRRÉVERSIBLE et supprimera toutes les données de la distribution.",
                    PrimaryButtonText = "Supprimer",
                    CloseButtonText = "Annuler",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.DeleteCommand.Execute(distribution);
                }
            }
        }

        /// <summary>
        /// Event handler pour installer une nouvelle distribution
        /// </summary>
        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Install_Click");

            // Créer le dialogue avec choix de distribution
            var dialog = new ContentDialog
            {
                Title = "Installer une nouvelle distribution",
                PrimaryButtonText = "Installer",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // Créer le contenu du dialogue
            var stackPanel = new StackPanel { Spacing = 16 };

            // Charger les distributions dynamiquement
            var loadingText = new TextBlock
            {
                Text = "Chargement des distributions disponibles...",
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            stackPanel.Children.Add(loadingText);

            var loadingProgress = new ProgressRing { IsActive = true, Width = 30, Height = 30 };
            stackPanel.Children.Add(loadingProgress);

            dialog.Content = stackPanel;

            // Afficher le dialogue pendant le chargement (non bloquant)
            var _ = dialog.ShowAsync();

            // Charger les distributions en arrière-plan
            var wslService = new Services.WslService();
            var availableDistros = await wslService.GetAvailableDistributionsAsync();

            // Si échec, utiliser la liste hardcodée
            if (availableDistros == null || availableDistros.Count == 0)
            {
                availableDistros = Constants.WslConstants.PopularDistributions.ToList();
            }

            // Mettre à jour le dialogue avec les distributions
            DispatcherQueue.TryEnqueue(() =>
            {
                stackPanel.Children.Clear();

                // ComboBox pour choisir la distribution
                var comboLabel = new TextBlock
                {
                    Text = "Choisissez une distribution :",
                    FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
                };

                var comboBox = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    PlaceholderText = "Sélectionnez une distribution..."
                };

                foreach (var distro in availableDistros)
                {
                    comboBox.Items.Add(distro);
                }
                comboBox.SelectedIndex = 0; // Premier par défaut

                stackPanel.Children.Add(comboLabel);
                stackPanel.Children.Add(comboBox);

                // TextBox pour nom personnalisé
                var nameLabel = new TextBlock
                {
                    Text = "Nom personnalisé (optionnel) :",
                    FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                    Margin = new Thickness(0, 8, 0, 0)
                };
                var nameTextBox = new TextBox
                {
                    PlaceholderText = "Laissez vide pour utiliser le nom par défaut",
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                stackPanel.Children.Add(nameLabel);
                stackPanel.Children.Add(nameTextBox);

                // Expander pour options avancées
                var advancedExpander = new Expander
                {
                    Header = "Options avancées",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var advancedPanel = new StackPanel { Spacing = 12 };

                // Web download
                var webDownloadCheckBox = new CheckBox
                {
                    Content = "Télécharger depuis le web (au lieu du Store)",
                    IsChecked = false
                };
                advancedPanel.Children.Add(webDownloadCheckBox);

                // No launch
                var noLaunchCheckBox = new CheckBox
                {
                    Content = "Ne pas lancer après installation",
                    IsChecked = false
                };
                advancedPanel.Children.Add(noLaunchCheckBox);

                // Emplacement personnalisé
                var locationLabel = new TextBlock
                {
                    Text = "Emplacement personnalisé (optionnel) :",
                    FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                    Margin = new Thickness(0, 8, 0, 0)
                };
                var locationTextBox = new TextBox
                {
                    PlaceholderText = "Laissez vide pour emplacement par défaut",
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                advancedPanel.Children.Add(locationLabel);
                advancedPanel.Children.Add(locationTextBox);

                advancedExpander.Content = advancedPanel;
                stackPanel.Children.Add(advancedExpander);

                // Ajouter note informative
                var infoText = new TextBlock
                {
                    Text = "💡 La distribution sera téléchargée et installée. Vous devrez configurer un nom d'utilisateur et mot de passe au premier démarrage.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    FontSize = 12,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                stackPanel.Children.Add(infoText);

                // Gérer le clic sur Installer
                dialog.PrimaryButtonClick += async (s, args) =>
                {
                    args.Cancel = true; // Empêcher la fermeture automatique

                    if (comboBox.SelectedItem != null)
                    {
                        var selectedDistro = comboBox.SelectedItem.ToString();
                        var customName = nameTextBox.Text.Trim();
                        var webDownload = webDownloadCheckBox.IsChecked == true;
                        var noLaunch = noLaunchCheckBox.IsChecked == true;
                        var location = locationTextBox.Text.Trim();

                        System.Diagnostics.Debug.WriteLine($"Installation de: {selectedDistro}, Nom: {customName}, Web: {webDownload}, NoLaunch: {noLaunch}, Location: {location}");

                        dialog.Hide();

                        // Lancer l'installation avec progression
                        await InstallDistributionWithProgressAsync(
                            selectedDistro,
                            customName,
                            webDownload,
                            noLaunch,
                            string.IsNullOrEmpty(location) ? null : location
                        );
                    }
                };
            });
        }

        /// <summary>
        /// Installe une distribution avec barre de progression
        /// </summary>
        private async System.Threading.Tasks.Task InstallDistributionWithProgressAsync(
            string distroName,
            string? customName = null,
            bool webDownload = false,
            bool noLaunch = false,
            string? installLocation = null)
        {
            var notificationService = NotificationService.Instance;

            // Créer un dialogue de progression
            var progressDialog = new ContentDialog
            {
                Title = "Installation en cours",
                CloseButtonText = "Fermer en arrière-plan",
                XamlRoot = this.Content.XamlRoot
            };

            var progressPanel = new StackPanel { Spacing = 16, MinWidth = 400 };

            var statusText = new TextBlock
            {
                Text = $"Téléchargement et installation de '{distroName}'...",
                TextWrapping = TextWrapping.Wrap
            };

            var progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                ShowError = false,
                ShowPaused = false
            };

            var detailText = new TextBlock
            {
                Text = "Cela peut prendre plusieurs minutes selon votre connexion internet.\nNe fermez pas l'application.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12
            };

            progressPanel.Children.Add(statusText);
            progressPanel.Children.Add(progressBar);
            progressPanel.Children.Add(detailText);

            progressDialog.Content = progressPanel;

            // Lancer l'installation en arrière-plan
            var installTask = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var wslService = new Services.WslService();
                    return await wslService.InstallDistributionAsync(distroName, webDownload, noLaunch, installLocation);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur installation: {ex}");
                    return (false, ex.Message);
                }
            });

            // Afficher le dialogue (non bloquant)
            var dialogTask = progressDialog.ShowAsync();

            // Attendre la fin de l'installation
            var (success, error) = await installTask;

            // Mettre à jour le dialogue
            DispatcherQueue.TryEnqueue(() =>
            {
                if (success)
                {
                    var displayName = string.IsNullOrEmpty(customName) ? distroName : customName;
                    statusText.Text = $"✅ Installation de '{displayName}' terminée!";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;
                    detailText.Text = noLaunch
                        ? "Installation terminée. Lancez la distribution pour la configurer."
                        : "Vous pouvez maintenant fermer cette fenêtre.";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowSuccess(
                        $"'{displayName}' installé avec succès!" + (string.IsNullOrEmpty(customName) ? "" : $" (basé sur {distroName})"),
                        Constants.Messages.TitleInstallation
                    );
                }
                else
                {
                    statusText.Text = "❌ L'installation a échoué";
                    progressBar.ShowError = true;
                    progressBar.IsIndeterminate = false;
                    detailText.Text = error ?? "La distribution est peut-être déjà installée.";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowWarning(
                        string.Format(Constants.Messages.InstallationWarning, distroName),
                        Constants.Messages.TitleInstallation
                    );
                }
            });

            // Attendre que l'utilisateur ferme le dialogue
            await dialogTask;

            // Rafraîchir la liste
            await System.Threading.Tasks.Task.Delay(1000);
            ViewModel.RefreshCommand.Execute(null);
        }

        #region Nouveaux dialogues

        /// <summary>
        /// Event handler pour importer une distribution
        /// </summary>
        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Import_Click");

            // Créer le dialogue d'import
            var dialog = new ContentDialog
            {
                Title = "Importer une distribution",
                PrimaryButtonText = "Importer",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 16 };

            // Nom de la distribution
            var nameLabel = new TextBlock
            {
                Text = "Nom de la distribution :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
            };
            var nameTextBox = new TextBox
            {
                PlaceholderText = "Ex: MyUbuntu, ImportedDebian...",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            stackPanel.Children.Add(nameLabel);
            stackPanel.Children.Add(nameTextBox);

            // Chemin du fichier
            var fileLabel = new TextBlock
            {
                Text = "Fichier source (TAR ou VHD) :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };
            var filePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var fileTextBox = new TextBox
            {
                PlaceholderText = "Sélectionnez un fichier...",
                IsReadOnly = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 300
            };
            var browseButton = new Button { Content = "Parcourir" };
            browseButton.Click += async (s, args) =>
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                // Ajouter les extensions de fichiers supportées
                picker.FileTypeFilter.Add("*"); // Tous les fichiers
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    fileTextBox.Text = file.Path;
                }
            };
            filePanel.Children.Add(fileTextBox);
            filePanel.Children.Add(browseButton);
            stackPanel.Children.Add(fileLabel);
            stackPanel.Children.Add(filePanel);

            // Emplacement d'installation
            var locationLabel = new TextBlock
            {
                Text = "Emplacement d'installation :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };
            var locationTextBox = new TextBox
            {
                PlaceholderText = Constants.WslConstants.DefaultImportLocation,
                Text = Constants.WslConstants.DefaultImportLocation,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            stackPanel.Children.Add(locationLabel);
            stackPanel.Children.Add(locationTextBox);

            // Version WSL
            var versionLabel = new TextBlock
            {
                Text = "Version WSL :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };
            var versionCombo = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = 1
            };
            versionCombo.Items.Add("WSL 1");
            versionCombo.Items.Add("WSL 2");
            stackPanel.Children.Add(versionLabel);
            stackPanel.Children.Add(versionCombo);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var name = nameTextBox.Text.Trim();
                var filePath = fileTextBox.Text.Trim();
                var location = locationTextBox.Text.Trim();
                var version = versionCombo.SelectedIndex == 0 ? 1 : 2;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(filePath))
                {
                    NotificationService.Instance.ShowError(
                        "Veuillez remplir tous les champs requis.",
                        "Champs manquants"
                    );
                    return;
                }

                await ImportWithProgressAsync(name, filePath, location, version);
            }
        }

        /// <summary>
        /// Importe une distribution avec barre de progression
        /// </summary>
        private async System.Threading.Tasks.Task ImportWithProgressAsync(string name, string filePath, string location, int version)
        {
            var notificationService = NotificationService.Instance;

            var progressDialog = new ContentDialog
            {
                Title = "Import en cours",
                CloseButtonText = "Fermer en arrière-plan",
                XamlRoot = this.Content.XamlRoot
            };

            var progressPanel = new StackPanel { Spacing = 16, MinWidth = 400 };
            var statusText = new TextBlock
            {
                Text = $"Import de '{name}' en cours...",
                TextWrapping = TextWrapping.Wrap
            };
            var progressBar = new ProgressBar { IsIndeterminate = true };
            var detailText = new TextBlock
            {
                Text = "Cela peut prendre plusieurs minutes selon la taille du fichier.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12
            };

            progressPanel.Children.Add(statusText);
            progressPanel.Children.Add(progressBar);
            progressPanel.Children.Add(detailText);
            progressDialog.Content = progressPanel;

            var importTask = System.Threading.Tasks.Task.Run(async () =>
            {
                var wslService = new Services.WslService();
                var isVhd = filePath.EndsWith(".vhd", StringComparison.OrdinalIgnoreCase) ||
                           filePath.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase);
                return await wslService.ImportDistributionAsync(name, location, filePath, isVhd, version);
            });

            var dialogTask = progressDialog.ShowAsync();
            var (success, error) = await importTask;

            DispatcherQueue.TryEnqueue(() =>
            {
                if (success)
                {
                    statusText.Text = $"✅ Import de '{name}' terminé!";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;
                    detailText.Text = "Vous pouvez maintenant fermer cette fenêtre.";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowSuccess(
                        string.Format(Constants.Messages.ImportSuccess, name),
                        Constants.Messages.TitleImport
                    );
                }
                else
                {
                    statusText.Text = "❌ L'import a échoué";
                    progressBar.ShowError = true;
                    progressBar.IsIndeterminate = false;
                    detailText.Text = error ?? "Erreur inconnue";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowError(
                        string.Format(Constants.Messages.ImportError, error ?? "Erreur inconnue"),
                        Constants.Messages.TitleError
                    );
                }
            });

            await dialogTask;
            await System.Threading.Tasks.Task.Delay(1000);
            ViewModel.RefreshCommand.Execute(null);
        }

        /// <summary>
        /// Event handler pour exporter avec options avancées
        /// </summary>
        private async void ExportAdvanced_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"ExportAdvanced_Click pour {distribution.Name}");

                var dialog = new ContentDialog
                {
                    Title = "Exporter la distribution",
                    PrimaryButtonText = "Exporter",
                    CloseButtonText = "Annuler",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                var stackPanel = new StackPanel { Spacing = 16 };

                // Format d'export
                var formatLabel = new TextBlock
                {
                    Text = "Format d'export :",
                    FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
                };
                var formatCombo = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    SelectedIndex = 0
                };
                formatCombo.Items.Add("TAR (Archive compressée)");
                formatCombo.Items.Add("VHD (Disque virtuel)");
                stackPanel.Children.Add(formatLabel);
                stackPanel.Children.Add(formatCombo);

                // Chemin du fichier
                var fileLabel = new TextBlock
                {
                    Text = "Fichier de destination :",
                    FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                    Margin = new Thickness(0, 8, 0, 0)
                };
                var filePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                var fileTextBox = new TextBox
                {
                    PlaceholderText = "Sélectionnez un emplacement...",
                    IsReadOnly = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Width = 300
                };
                var browseButton = new Button { Content = "Parcourir" };
                browseButton.Click += async (s, args) =>
                {
                    var picker = new Windows.Storage.Pickers.FileSavePicker();
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    picker.SuggestedFileName = $"{distribution.Name}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (formatCombo.SelectedIndex == 0)
                    {
                        picker.FileTypeChoices.Add("TAR Archive", new List<string> { ".tar" });
                    }
                    else
                    {
                        picker.FileTypeChoices.Add("Virtual Hard Disk", new List<string> { ".vhd" });
                    }

                    var file = await picker.PickSaveFileAsync();
                    if (file != null)
                    {
                        fileTextBox.Text = file.Path;
                    }
                };
                filePanel.Children.Add(fileTextBox);
                filePanel.Children.Add(browseButton);
                stackPanel.Children.Add(fileLabel);
                stackPanel.Children.Add(filePanel);

                dialog.Content = stackPanel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var filePath = fileTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        NotificationService.Instance.ShowError(
                            "Veuillez sélectionner un fichier de destination.",
                            "Fichier manquant"
                        );
                        return;
                    }

                    var asVhd = formatCombo.SelectedIndex == 1;
                    await ExportWithProgressAsync(distribution.Name, filePath, asVhd);
                }
            }
        }

        /// <summary>
        /// Exporte une distribution avec barre de progression
        /// </summary>
        private async System.Threading.Tasks.Task ExportWithProgressAsync(string name, string filePath, bool asVhd)
        {
            var notificationService = NotificationService.Instance;

            var progressDialog = new ContentDialog
            {
                Title = "Export en cours",
                CloseButtonText = "Fermer en arrière-plan",
                XamlRoot = this.Content.XamlRoot
            };

            var progressPanel = new StackPanel { Spacing = 16, MinWidth = 400 };
            var statusText = new TextBlock
            {
                Text = $"Export de '{name}' en cours...",
                TextWrapping = TextWrapping.Wrap
            };
            var progressBar = new ProgressBar { IsIndeterminate = true };
            var detailText = new TextBlock
            {
                Text = "Cela peut prendre plusieurs minutes selon la taille de la distribution.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12
            };

            progressPanel.Children.Add(statusText);
            progressPanel.Children.Add(progressBar);
            progressPanel.Children.Add(detailText);
            progressDialog.Content = progressPanel;

            var exportTask = System.Threading.Tasks.Task.Run(async () =>
            {
                var wslService = new Services.WslService();
                return await wslService.ExportDistributionAsync(name, filePath, asVhd);
            });

            var dialogTask = progressDialog.ShowAsync();
            var (success, error) = await exportTask;

            DispatcherQueue.TryEnqueue(() =>
            {
                if (success)
                {
                    statusText.Text = $"✅ Export de '{name}' terminé!";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;
                    detailText.Text = $"Fichier exporté : {filePath}";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowSuccess(
                        $"Exporté vers: {filePath}",
                        Constants.Messages.TitleExport
                    );
                }
                else
                {
                    statusText.Text = "❌ L'export a échoué";
                    progressBar.ShowError = true;
                    progressBar.IsIndeterminate = false;
                    detailText.Text = error ?? "Erreur inconnue";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowError(
                        error ?? "Erreur inconnue",
                        Constants.Messages.TitleError
                    );
                }
            });

            await dialogTask;
        }

        /// <summary>
        /// Event handler pour monter un disque
        /// </summary>
        private async void MountDisk_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MountDisk_Click");

            var dialog = new ContentDialog
            {
                Title = "Monter un disque ou VHD",
                PrimaryButtonText = "Monter",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 16 };

            // Chemin du disque
            var pathLabel = new TextBlock
            {
                Text = "Chemin du disque ou VHD :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
            };
            var pathPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var pathTextBox = new TextBox
            {
                PlaceholderText = @"Ex: \\.\PHYSICALDRIVE1 ou C:\disk.vhd",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 300
            };
            var browseButton = new Button { Content = "Parcourir VHD" };
            browseButton.Click += async (s, args) =>
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                picker.FileTypeFilter.Add("*"); // Tous les fichiers
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    pathTextBox.Text = file.Path;
                }
            };
            pathPanel.Children.Add(pathTextBox);
            pathPanel.Children.Add(browseButton);
            stackPanel.Children.Add(pathLabel);
            stackPanel.Children.Add(pathPanel);

            // Options
            var bareCheckBox = new CheckBox
            {
                Content = "Mode bare (sans partition)",
                Margin = new Thickness(0, 8, 0, 0)
            };
            stackPanel.Children.Add(bareCheckBox);

            // Partition
            var partitionLabel = new TextBlock
            {
                Text = "Numéro de partition (optionnel) :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };
            var partitionTextBox = new TextBox
            {
                PlaceholderText = "Ex: 1, 2...",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            stackPanel.Children.Add(partitionLabel);
            stackPanel.Children.Add(partitionTextBox);

            // Type de système de fichiers
            var typeLabel = new TextBlock
            {
                Text = "Type de système de fichiers (optionnel) :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };
            var typeCombo = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                PlaceholderText = "Sélectionnez..."
            };
            foreach (var fs in Constants.WslConstants.FileSystemTypes)
            {
                typeCombo.Items.Add(fs);
            }
            stackPanel.Children.Add(typeLabel);
            stackPanel.Children.Add(typeCombo);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var diskPath = pathTextBox.Text.Trim();
                if (string.IsNullOrEmpty(diskPath))
                {
                    NotificationService.Instance.ShowError(
                        "Veuillez spécifier un chemin de disque.",
                        "Chemin manquant"
                    );
                    return;
                }

                await MountWithProgressAsync(
                    diskPath,
                    bareCheckBox.IsChecked == true,
                    partitionTextBox.Text.Trim(),
                    typeCombo.SelectedItem?.ToString()
                );
            }
        }

        /// <summary>
        /// Monte un disque avec barre de progression
        /// </summary>
        private async System.Threading.Tasks.Task MountWithProgressAsync(string diskPath, bool bare, string? partition, string? type)
        {
            var notificationService = NotificationService.Instance;

            var progressDialog = new ContentDialog
            {
                Title = "Montage en cours",
                CloseButtonText = "Fermer en arrière-plan",
                XamlRoot = this.Content.XamlRoot
            };

            var progressPanel = new StackPanel { Spacing = 16, MinWidth = 400 };
            var statusText = new TextBlock
            {
                Text = "Montage du disque en cours...",
                TextWrapping = TextWrapping.Wrap
            };
            var progressBar = new ProgressBar { IsIndeterminate = true };

            progressPanel.Children.Add(statusText);
            progressPanel.Children.Add(progressBar);
            progressDialog.Content = progressPanel;

            var mountTask = System.Threading.Tasks.Task.Run(async () =>
            {
                var wslService = new Services.WslService();
                return await wslService.MountDiskAsync(diskPath, bare, partition, type);
            });

            var dialogTask = progressDialog.ShowAsync();
            var (success, error) = await mountTask;

            DispatcherQueue.TryEnqueue(() =>
            {
                if (success)
                {
                    statusText.Text = "✅ Disque monté avec succès!";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowSuccess(
                        Constants.Messages.MountSuccess,
                        Constants.Messages.TitleMount
                    );
                }
                else
                {
                    statusText.Text = "❌ Le montage a échoué";
                    progressBar.ShowError = true;
                    progressBar.IsIndeterminate = false;
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowError(
                        string.Format(Constants.Messages.MountError, error ?? "Erreur inconnue"),
                        Constants.Messages.TitleError
                    );
                }
            });

            await dialogTask;
        }

        /// <summary>
        /// Event handler pour démonter un disque
        /// </summary>
        private async void UnmountDisk_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("UnmountDisk_Click");

            var dialog = new ContentDialog
            {
                Title = "Démonter un disque",
                PrimaryButtonText = "Démonter",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 16 };

            var pathLabel = new TextBlock
            {
                Text = "Chemin du disque à démonter :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
            };
            var pathTextBox = new TextBox
            {
                PlaceholderText = @"Ex: \\.\PHYSICALDRIVE1",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            stackPanel.Children.Add(pathLabel);
            stackPanel.Children.Add(pathTextBox);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var diskPath = pathTextBox.Text.Trim();
                if (string.IsNullOrEmpty(diskPath))
                {
                    NotificationService.Instance.ShowError(
                        "Veuillez spécifier un chemin de disque.",
                        "Chemin manquant"
                    );
                    return;
                }

                var wslService = new Services.WslService();
                var (success, error) = await wslService.UnmountDiskAsync(diskPath);

                if (success)
                {
                    NotificationService.Instance.ShowSuccess(
                        Constants.Messages.UnmountSuccess,
                        Constants.Messages.TitleUnmount
                    );
                }
                else
                {
                    NotificationService.Instance.ShowError(
                        string.Format(Constants.Messages.UnmountError, error ?? "Erreur inconnue"),
                        Constants.Messages.TitleError
                    );
                }
            }
        }

        /// <summary>
        /// Event handler pour définir la version WSL par défaut
        /// </summary>
        private async void SetDefaultVersion_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("SetDefaultVersion_Click");

            var dialog = new ContentDialog
            {
                Title = "Définir la version WSL par défaut",
                PrimaryButtonText = "Définir",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 16 };

            var infoText = new TextBlock
            {
                Text = "Cette version sera utilisée pour les nouvelles distributions installées.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12
            };

            var versionLabel = new TextBlock
            {
                Text = "Choisissez la version par défaut :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };

            var versionCombo = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = 1
            };
            versionCombo.Items.Add("WSL 1 (Compatibilité maximale)");
            versionCombo.Items.Add("WSL 2 (Recommandé - Meilleures performances)");

            stackPanel.Children.Add(infoText);
            stackPanel.Children.Add(versionLabel);
            stackPanel.Children.Add(versionCombo);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var version = versionCombo.SelectedIndex == 0 ? 1 : 2;
                var wslService = new Services.WslService();
                var (success, error) = await wslService.SetDefaultWslVersionAsync(version);

                if (success)
                {
                    NotificationService.Instance.ShowSuccess(
                        string.Format(Constants.Messages.DefaultVersionSet, version),
                        Constants.Messages.TitleDefaultVersion
                    );
                }
                else
                {
                    NotificationService.Instance.ShowError(
                        string.Format(Constants.Messages.DefaultVersionError, error ?? "Erreur inconnue"),
                        Constants.Messages.TitleError
                    );
                }
            }
        }

        /// <summary>
        /// Event handler pour ouvrir terminal avec utilisateur spécifique
        /// </summary>
        private async void OpenTerminalAsUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is WslDistribution distribution)
            {
                System.Diagnostics.Debug.WriteLine($"OpenTerminalAsUser_Click pour {distribution.Name}");

                var dialog = new ContentDialog
                {
                    Title = $"Ouvrir terminal - {distribution.Name}",
                    PrimaryButtonText = "Ouvrir",
                    CloseButtonText = "Annuler",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                var stackPanel = new StackPanel { Spacing = 16 };

                var userLabel = new TextBlock
                {
                    Text = "Nom d'utilisateur (laissez vide pour l'utilisateur par défaut) :",
                    FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
                };

                var userTextBox = new TextBox
                {
                    PlaceholderText = "Ex: root, utilisateur...",
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                var infoText = new TextBlock
                {
                    Text = "💡 Si vous laissez vide, le terminal s'ouvrira avec l'utilisateur par défaut de la distribution.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    FontSize = 12,
                    Margin = new Thickness(0, 8, 0, 0)
                };

                stackPanel.Children.Add(userLabel);
                stackPanel.Children.Add(userTextBox);
                stackPanel.Children.Add(infoText);

                dialog.Content = stackPanel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        var userName = userTextBox.Text.Trim();
                        var wslService = new Services.WslService();
                        wslService.OpenTerminalAsUser(distribution.Name, string.IsNullOrEmpty(userName) ? null : userName);

                        NotificationService.Instance.ShowSuccess(
                            $"Terminal ouvert pour '{distribution.Name}'" + (string.IsNullOrEmpty(userName) ? "" : $" en tant que {userName}"),
                            Constants.Messages.TitleTerminal
                        );
                    }
                    catch (Exception ex)
                    {
                        NotificationService.Instance.ShowError(
                            $"Impossible d'ouvrir le terminal : {ex.Message}",
                            Constants.Messages.TitleError
                        );
                    }
                }
            }
        }

        #endregion
    }
}
