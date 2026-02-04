using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WSL_Manager.Constants;
using WSL_Manager.Helpers;
using WSL_Manager.Models;
using WSL_Manager.Services;

namespace WSL_Manager.ViewModels
{
    /// <summary>
    /// ViewModel pour la fenêtre principale
    /// Gère la liste des distributions et toutes les actions disponibles
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly WslService _wslService;
        private readonly NotificationService _notificationService;
        private bool _isLoading;
        private bool _isRefreshing;

        /// <summary>
        /// Liste observable des distributions WSL
        /// Se met à jour automatiquement dans l'UI
        /// </summary>
        public ObservableCollection<WslDistribution> Distributions { get; }

        /// <summary>
        /// Indique si des données sont en cours de chargement
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsEmptyState));
                OnPropertyChanged(nameof(HasDistributions));
            }
        }

        /// <summary>
        /// Indique si l'état vide doit être affiché (aucune distribution)
        /// </summary>
        public bool IsEmptyState => !IsLoading && Distributions.Count == 0;

        /// <summary>
        /// Indique si des distributions sont présentes
        /// </summary>
        public bool HasDistributions => !IsLoading && Distributions.Count > 0;

        #region Commandes

        /// <summary>
        /// Commande pour rafraîchir la liste des distributions
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Commande pour démarrer une distribution
        /// </summary>
        public ICommand StartCommand { get; }

        /// <summary>
        /// Commande pour arrêter une distribution
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Commande pour redémarrer une distribution
        /// </summary>
        public ICommand RestartCommand { get; }

        /// <summary>
        /// Commande pour ouvrir un terminal dans une distribution
        /// </summary>
        public ICommand OpenTerminalCommand { get; }

        /// <summary>
        /// Commande pour ouvrir l'explorateur de fichiers pour une distribution
        /// </summary>
        public ICommand OpenExplorerCommand { get; }

        /// <summary>
        /// Commande pour définir une distribution par défaut
        /// </summary>
        public ICommand SetDefaultCommand { get; }

        /// <summary>
        /// Commande pour afficher les informations d'une distribution
        /// </summary>
        public ICommand ShowInfoCommand { get; }

        /// <summary>
        /// Commande pour mettre à jour WSL
        /// </summary>
        public ICommand UpdateWslCommand { get; }

        /// <summary>
        /// Commande pour convertir une distribution WSL 1 <-> WSL 2
        /// </summary>
        public ICommand ConvertVersionCommand { get; }

        /// <summary>
        /// Commande pour exporter une distribution
        /// </summary>
        public ICommand ExportCommand { get; }

        /// <summary>
        /// Commande pour supprimer une distribution
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Commande pour arrêter toutes les distributions
        /// </summary>
        public ICommand ShutdownAllCommand { get; }

        /// <summary>
        /// Commande pour installer une nouvelle distribution
        /// </summary>
        public ICommand InstallDistributionCommand { get; }

        /// <summary>
        /// Commande pour mettre à jour les packages d'une distribution
        /// </summary>
        public ICommand UpdateInstanceCommand { get; }

        #endregion

        public MainViewModel()
        {
            _wslService = new WslService();
            _notificationService = NotificationService.Instance;
            Distributions = new ObservableCollection<WslDistribution>();

            // Initialise toutes les commandes
            RefreshCommand = new RelayCommand(async () => await LoadDistributionsAsync());
            StartCommand = new RelayCommand<WslDistribution>(async (dist) => await StartDistributionAsync(dist));
            StopCommand = new RelayCommand<WslDistribution>(async (dist) => await StopDistributionAsync(dist));
            RestartCommand = new RelayCommand<WslDistribution>(async (dist) => await RestartDistributionAsync(dist));
            OpenTerminalCommand = new RelayCommand<WslDistribution>(OpenTerminal);
            OpenExplorerCommand = new RelayCommand<WslDistribution>(OpenExplorer);
            SetDefaultCommand = new RelayCommand<WslDistribution>(async (dist) => await SetDefaultDistributionAsync(dist));
            ShowInfoCommand = new RelayCommand<WslDistribution>(async (dist) => await ShowDistributionInfoAsync(dist));
            UpdateWslCommand = new RelayCommand(async () => await UpdateWslAsync());
            ConvertVersionCommand = new RelayCommand<WslDistribution>(async (dist) => await ConvertVersionAsync(dist));
            ExportCommand = new RelayCommand<WslDistribution>(async (dist) => await ExportDistributionAsync(dist));
            DeleteCommand = new RelayCommand<WslDistribution>(async (dist) => await DeleteDistributionAsync(dist));
            ShutdownAllCommand = new RelayCommand(async () => await ShutdownAllDistributionsAsync());
            InstallDistributionCommand = new RelayCommand(async () => await ShowInstallDialogAsync());
            UpdateInstanceCommand = new RelayCommand<WslDistribution>(async (dist) => await UpdateInstanceAsync(dist));

            // Charge les distributions au démarrage
            _ = LoadDistributionsAsync();
        }

        /// <summary>
        /// Charge la liste de toutes les distributions WSL
        /// Affiche un message de succès ou d'erreur
        /// </summary>
        private async Task LoadDistributionsAsync(bool silent = false)
        {
            // Évite les appels concurrents
            if (_isRefreshing)
            {
                System.Diagnostics.Debug.WriteLine("LoadDistributionsAsync déjà en cours, appel ignoré");
                return;
            }

            _isRefreshing = true;
            IsLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== Début LoadDistributionsAsync ===");
                var distributions = await _wslService.GetDistributionsAsync();
                System.Diagnostics.Debug.WriteLine($"Nombre de distributions récupérées: {distributions.Count}");

                Distributions.Clear();
                foreach (var distribution in distributions)
                {
                    System.Diagnostics.Debug.WriteLine($"Distribution chargée: '{distribution.Name}' - État: {distribution.State} - Version: {distribution.Version} - Défaut: {distribution.IsDefault}");
                    Distributions.Add(distribution);
                }

                // Notifie les changements d'état
                OnPropertyChanged(nameof(IsEmptyState));
                OnPropertyChanged(nameof(HasDistributions));

                if (!silent)
                {
                    if (Distributions.Count == 0)
                    {
                        _notificationService.ShowInfo(
                            "Aucune distribution WSL n'a été trouvée sur votre système.",
                            "Aucune distribution"
                        );
                    }
                    else
                    {
                        _notificationService.ShowSuccess(
                            $"{Distributions.Count} distribution(s) chargée(s)",
                            "Chargement réussi"
                        );
                    }
                }

                System.Diagnostics.Debug.WriteLine("=== Fin LoadDistributionsAsync ===");
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Impossible de charger les distributions : {ex.Message}",
                    "Erreur de chargement"
                );
                System.Diagnostics.Debug.WriteLine($"Erreur LoadDistributions: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                _isRefreshing = false;
            }
        }

        /// <summary>
        /// Démarre une distribution WSL
        /// </summary>
        private async Task StartDistributionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            IsLoading = true;

            try
            {
                var success = await _wslService.StartDistributionAsync(distribution.Name);

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"La distribution '{distribution.Name}' a été démarrée avec succès.",
                        "Démarrage réussi"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowError(
                        $"Impossible de démarrer la distribution '{distribution.Name}'.",
                        "Erreur de démarrage"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors du démarrage : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Arrête une distribution WSL
        /// </summary>
        private async Task StopDistributionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            IsLoading = true;

            try
            {
                var success = await _wslService.StopDistributionAsync(distribution.Name);

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"La distribution '{distribution.Name}' a été arrêtée avec succès.",
                        "Arrêt réussi"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowError(
                        $"Impossible d'arrêter la distribution '{distribution.Name}'.",
                        "Erreur d'arrêt"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de l'arrêt : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Redémarre une distribution WSL (arrêt puis démarrage)
        /// </summary>
        private async Task RestartDistributionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            IsLoading = true;

            try
            {
                _notificationService.ShowInfo(
                    $"Redémarrage de '{distribution.Name}' en cours...",
                    "Redémarrage"
                );

                var success = await _wslService.RestartDistributionAsync(distribution.Name);

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"La distribution '{distribution.Name}' a été redémarrée avec succès.",
                        "Redémarrage réussi"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowError(
                        $"Impossible de redémarrer la distribution '{distribution.Name}'.",
                        "Erreur de redémarrage"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors du redémarrage : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Ouvre un terminal pour la distribution sélectionnée
        /// </summary>
        private void OpenTerminal(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            try
            {
                _wslService.OpenTerminal(distribution.Name);
                _notificationService.ShowSuccess(
                    $"Ouverture du terminal pour '{distribution.Name}'.",
                    "Terminal"
                );
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Impossible d'ouvrir le terminal : {ex.Message}",
                    "Erreur"
                );
            }
        }

        /// <summary>
        /// Ouvre l'explorateur de fichiers pour la distribution sélectionnée
        /// </summary>
        private void OpenExplorer(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            try
            {
                _wslService.OpenFileExplorer(distribution.Name);
                _notificationService.ShowSuccess(
                    $"Ouverture de l'explorateur pour '{distribution.Name}'.",
                    "Explorateur"
                );
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Impossible d'ouvrir l'explorateur : {ex.Message}",
                    "Erreur"
                );
            }
        }

        /// <summary>
        /// Définit une distribution comme distribution par défaut
        /// </summary>
        private async Task SetDefaultDistributionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            if (distribution.IsDefault)
            {
                _notificationService.ShowInfo(
                    $"'{distribution.Name}' est déjà la distribution par défaut.",
                    "Distribution par défaut"
                );
                return;
            }

            IsLoading = true;

            try
            {
                var success = await _wslService.SetDefaultDistributionAsync(distribution.Name);

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"'{distribution.Name}' est maintenant la distribution par défaut.",
                        "Modification réussie"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowError(
                        $"Impossible de définir '{distribution.Name}' comme distribution par défaut.",
                        "Erreur"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de la modification : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Affiche les informations détaillées d'une distribution
        /// </summary>
        private async Task ShowDistributionInfoAsync(WslDistribution? distribution)
        {
            System.Diagnostics.Debug.WriteLine($"=== ShowDistributionInfoAsync appelé ===");

            if (distribution == null)
            {
                System.Diagnostics.Debug.WriteLine("Distribution est null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Distribution: {distribution.Name}");
            IsLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Appel GetDistributionInfoAsync...");
                var info = await _wslService.GetDistributionInfoAsync(distribution.Name);
                System.Diagnostics.Debug.WriteLine($"Info reçue: {info}");

                _notificationService.ShowInfo(
                    info,
                    $"Informations - {distribution.Name}"
                );
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Impossible de récupérer les informations : {ex.Message}",
                    "Erreur"
                );
                System.Diagnostics.Debug.WriteLine($"Erreur ShowInfo: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Met à jour WSL vers la dernière version
        /// </summary>
        private async Task UpdateWslAsync()
        {
            IsLoading = true;

            try
            {
                _notificationService.ShowInfo(
                    "Mise à jour de WSL en cours...",
                    "Mise à jour"
                );

                var success = await _wslService.UpdateWslAsync();

                if (success)
                {
                    _notificationService.ShowSuccess(
                        "WSL a été mis à jour avec succès.",
                        "Mise à jour réussie"
                    );
                }
                else
                {
                    _notificationService.ShowWarning(
                        "La mise à jour de WSL a échoué ou est déjà à jour.",
                        "Mise à jour"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de la mise à jour : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Convertit une distribution WSL 1 <-> WSL 2
        /// </summary>
        private async Task ConvertVersionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            var targetVersion = distribution.Version == 1 ? 2 : 1;

            IsLoading = true;

            try
            {
                _notificationService.ShowInfo(
                    $"Conversion de '{distribution.Name}' vers WSL {targetVersion}...",
                    "Conversion en cours"
                );

                var success = await _wslService.ConvertWslVersionAsync(distribution.Name, targetVersion);

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"'{distribution.Name}' a été converti vers WSL {targetVersion}.",
                        "Conversion réussie"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowError(
                        $"Impossible de convertir '{distribution.Name}'.",
                        "Erreur de conversion"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de la conversion : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Exporte une distribution vers un fichier tar
        /// Exporte vers le dossier Documents par défaut
        /// </summary>
        private async Task ExportDistributionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
                return;

            IsLoading = true;

            try
            {
                // Chemin d'export par défaut dans Documents
                var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var exportPath = System.IO.Path.Combine(documentsPath, $"{distribution.Name}_backup_{timestamp}.tar");

                _notificationService.ShowInfo(
                    $"Export de '{distribution.Name}' vers Documents...",
                    "Export en cours"
                );

                var success = await _wslService.ExportDistributionAsync(distribution.Name, exportPath);

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"Exporté vers: {exportPath}",
                        "Export réussi"
                    );
                }
                else
                {
                    _notificationService.ShowError(
                        $"Impossible d'exporter '{distribution.Name}'.",
                        "Erreur d'export"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de l'export : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Supprime une distribution (avec confirmation)
        /// </summary>
        private async Task DeleteDistributionAsync(WslDistribution? distribution)
        {
            if (distribution == null)
            {
                Logger.Warning("Distribution est null");
                return;
            }

            Logger.BeginOperation("Suppression", distribution.Name);

            try
            {
                _notificationService.ShowWarning(
                    string.Format(Messages.DeletingDistribution, distribution.Name),
                    Messages.TitleDeletion
                );

                Logger.Debug($"Appel UnregisterDistributionAsync pour '{distribution.Name}'");
                var success = await _wslService.UnregisterDistributionAsync(distribution.Name);
                Logger.Debug($"UnregisterDistributionAsync résultat: {success}");

                if (success)
                {
                    _notificationService.ShowSuccess(
                        string.Format(Messages.DistributionDeletedSuccess, distribution.Name),
                        Messages.TitleDeletion
                    );

                    // Attendre un peu pour que WSL mette à jour son état
                    Logger.Debug($"Attente de {AppConstants.RefreshDelayMs}ms avant rechargement...");
                    await Task.Delay(AppConstants.RefreshDelayMs);

                    // Recharger la liste des distributions (silent pour éviter la notification)
                    Logger.Debug("Rechargement de la liste des distributions...");
                    await LoadDistributionsAsync(silent: true);

                    Logger.EndOperation("Suppression", true);
                }
                else
                {
                    _notificationService.ShowError(
                        string.Format(Messages.DistributionDeleteError, distribution.Name),
                        Messages.TitleError
                    );
                    Logger.EndOperation("Suppression", false);
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    string.Format(Messages.GenericError, "la suppression", ex.Message),
                    Messages.TitleError
                );
                Logger.Error($"Erreur lors de la suppression de '{distribution.Name}'", ex);
                Logger.EndOperation("Suppression", false);
            }
        }

        /// <summary>
        /// Arrête toutes les distributions WSL
        /// </summary>
        private async Task ShutdownAllDistributionsAsync()
        {
            IsLoading = true;

            try
            {
                var success = await _wslService.ShutdownAllAsync();

                if (success)
                {
                    _notificationService.ShowSuccess(
                        "Toutes les distributions ont été arrêtées.",
                        "Arrêt complet"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowError(
                        "Impossible d'arrêter toutes les distributions.",
                        "Erreur"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de l'arrêt : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Affiche le dialogue pour installer une nouvelle distribution
        /// Pour l'instant, installe Ubuntu par défaut
        /// </summary>
        private async Task ShowInstallDialogAsync()
        {
            // Liste des distributions populaires
            var distributions = new[] { "Ubuntu", "Debian", "kali-linux", "Ubuntu-22.04", "Ubuntu-20.04" };

            _notificationService.ShowInfo(
                "Distributions disponibles: Ubuntu, Debian, Kali-Linux, Ubuntu-22.04, Ubuntu-20.04. Installation d'Ubuntu...",
                "Installation"
            );

            IsLoading = true;

            try
            {
                // Pour l'instant, on installe Ubuntu par défaut
                // TODO: Ajouter un dialogue pour choisir la distribution
                var success = await _wslService.InstallDistributionAsync("Ubuntu");

                if (success)
                {
                    _notificationService.ShowSuccess(
                        "Ubuntu a été installé avec succès. Configurez-le au premier lancement.",
                        "Installation réussie"
                    );
                    await LoadDistributionsAsync(silent: true);
                }
                else
                {
                    _notificationService.ShowWarning(
                        "L'installation a échoué ou Ubuntu est déjà installé.",
                        "Installation"
                    );
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    $"Erreur lors de l'installation : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Met à jour les packages d'une instance Linux
        /// </summary>
        private async Task UpdateInstanceAsync(WslDistribution? distribution)
        {
            if (distribution == null)
            {
                Logger.Warning("Distribution est null");
                return;
            }

            Logger.BeginOperation("Mise à jour des packages", distribution.Name);

            IsLoading = true;

            try
            {
                _notificationService.ShowInfo(
                    $"Mise à jour de '{distribution.Name}' en cours... Cela peut prendre plusieurs minutes.",
                    Messages.TitleUpdate
                );

                Logger.Debug($"Appel UpdateDistributionPackagesAsync pour '{distribution.Name}'");
                var success = await _wslService.UpdateDistributionPackagesAsync(distribution.Name);
                Logger.Debug($"UpdateDistributionPackagesAsync résultat: {success}");

                if (success)
                {
                    _notificationService.ShowSuccess(
                        $"'{distribution.Name}' a été mis à jour avec succès.",
                        Messages.TitleUpdate
                    );
                    Logger.EndOperation("Mise à jour des packages", true);
                }
                else
                {
                    _notificationService.ShowWarning(
                        $"La mise à jour de '{distribution.Name}' a échoué. Vérifiez que la distribution est accessible et que vous avez les permissions nécessaires.",
                        Messages.TitleUpdate
                    );
                    Logger.EndOperation("Mise à jour des packages", false);
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError(
                    string.Format(Messages.GenericError, "la mise à jour", ex.Message),
                    Messages.TitleError
                );
                Logger.Error($"Erreur lors de la mise à jour de '{distribution.Name}'", ex);
                Logger.EndOperation("Mise à jour des packages", false);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
