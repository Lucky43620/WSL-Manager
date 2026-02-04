using System;
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

            // Liste des distributions populaires
            var distributions = new[]
            {
                "Ubuntu",
                "Ubuntu-22.04",
                "Ubuntu-20.04",
                "Ubuntu-24.04",
                "Debian",
                "kali-linux",
                "openSUSE-Leap-15.5",
                "SUSE-Linux-Enterprise-15-SP5",
                "OracleLinux_8_5",
                "OracleLinux_9_1"
            };

            foreach (var distro in distributions)
            {
                comboBox.Items.Add(distro);
            }
            comboBox.SelectedIndex = 0; // Ubuntu par défaut

            stackPanel.Children.Add(comboLabel);
            stackPanel.Children.Add(comboBox);

            // TextBox pour nom personnalisé (optionnel)
            var nameLabel = new TextBlock
            {
                Text = "Nom personnalisé (optionnel) :",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                Margin = new Thickness(0, 8, 0, 0)
            };

            var nameTextBox = new TextBox
            {
                PlaceholderText = "Ex: MonUbuntu, DevDebian... (laissez vide pour nom par défaut)",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            stackPanel.Children.Add(nameLabel);
            stackPanel.Children.Add(nameTextBox);

            // Ajouter note informative
            var infoText = new TextBlock
            {
                Text = "💡 La distribution sera téléchargée depuis le Microsoft Store. Vous devrez configurer un nom d'utilisateur et mot de passe au premier démarrage.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                FontSize = 12,
                Margin = new Thickness(0, 8, 0, 0)
            };
            stackPanel.Children.Add(infoText);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && comboBox.SelectedItem != null)
            {
                var selectedDistro = comboBox.SelectedItem.ToString();
                var customName = nameTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(selectedDistro))
                {
                    System.Diagnostics.Debug.WriteLine($"Installation de: {selectedDistro}, Nom: {customName}");

                    // Lancer l'installation avec progression
                    await InstallDistributionWithProgressAsync(selectedDistro, customName);
                }
            }
        }

        /// <summary>
        /// Installe une distribution avec barre de progression
        /// </summary>
        private async System.Threading.Tasks.Task InstallDistributionWithProgressAsync(string distroName, string customName)
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

                    // Si nom personnalisé, on doit d'abord installer puis renommer
                    // Pour l'instant on installe juste avec le nom par défaut
                    var success = await wslService.InstallDistributionAsync(distroName);
                    return success;
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur installation: {ex}");
                    return false;
                }
            });

            // Afficher le dialogue (non bloquant)
            var dialogTask = progressDialog.ShowAsync();

            // Attendre la fin de l'installation
            var success = await installTask;

            // Mettre à jour le dialogue
            DispatcherQueue.TryEnqueue(() =>
            {
                if (success)
                {
                    statusText.Text = $"✅ Installation de '{distroName}' terminée!";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;
                    detailText.Text = "Vous pouvez maintenant fermer cette fenêtre.";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowSuccess(
                        $"'{distroName}' installé! {(string.IsNullOrEmpty(customName) ? "" : $"Nom: {customName}")}",
                        "Installation réussie"
                    );
                }
                else
                {
                    statusText.Text = $"❌ L'installation a échoué";
                    progressBar.ShowError = true;
                    progressBar.IsIndeterminate = false;
                    detailText.Text = "La distribution est peut-être déjà installée. Vérifiez avec wsl --list";
                    progressDialog.PrimaryButtonText = "OK";
                    progressDialog.CloseButtonText = null;

                    notificationService.ShowWarning(
                        $"Installation de '{distroName}' échouée ou déjà installée",
                        "Installation"
                    );
                }
            });

            // Attendre que l'utilisateur ferme le dialogue
            await dialogTask;

            // Rafraîchir la liste
            await System.Threading.Tasks.Task.Delay(1000);
            ViewModel.RefreshCommand.Execute(null);
        }
    }
}
