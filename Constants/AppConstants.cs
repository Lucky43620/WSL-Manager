using System.Collections.Generic;

namespace WSL_Manager.Constants
{
    /// <summary>
    /// Constantes globales de l'application
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Nom de l'application
        /// </summary>
        public const string AppName = "WSL Manager";

        /// <summary>
        /// Version de l'application
        /// </summary>
        public const string Version = "1.0.0";

        /// <summary>
        /// Délai avant le rechargement après une opération (ms)
        /// </summary>
        public const int RefreshDelayMs = 1000;

        /// <summary>
        /// Délai avant le redémarrage d'une distribution (ms)
        /// </summary>
        public const int RestartDelayMs = 500;
    }

    /// <summary>
    /// Messages utilisés dans l'application
    /// </summary>
    public static class Messages
    {
        // Messages de succès
        public const string DistributionStartedSuccess = "La distribution '{0}' a été démarrée avec succès.";
        public const string DistributionStoppedSuccess = "La distribution '{0}' a été arrêtée avec succès.";
        public const string DistributionRestartedSuccess = "La distribution '{0}' a été redémarrée avec succès.";
        public const string DistributionDeletedSuccess = "'{0}' a été supprimé avec succès.";
        public const string DistributionSetDefaultSuccess = "'{0}' est maintenant la distribution par défaut.";
        public const string DistributionConvertedSuccess = "'{0}' a été converti vers WSL {1}.";
        public const string DistributionExportedSuccess = "Exporté vers: {0}";
        public const string WslUpdatedSuccess = "WSL a été mis à jour avec succès.";
        public const string AllDistributionsStoppedSuccess = "Toutes les distributions ont été arrêtées.";
        public const string DistributionInstalledSuccess = "{0} a été installé avec succès. Configurez-le au premier lancement.";
        public const string LoadedDistributionsSuccess = "{0} distribution(s) chargée(s)";

        // Messages d'information
        public const string DistributionAlreadyDefault = "'{0}' est déjà la distribution par défaut.";
        public const string NoDistributionsFound = "Aucune distribution WSL n'a été trouvée sur votre système.";
        public const string TerminalOpened = "Ouverture du terminal pour '{0}'.";
        public const string ExplorerOpened = "Ouverture de l'explorateur pour '{0}'.";
        public const string WslStatusRetrieved = "État WSL récupéré avec succès.";
        public const string WslVersionRetrieved = "Version WSL récupérée avec succès.";
        public const string DefaultVersionSet = "Version WSL par défaut définie sur WSL {0}.";
        public const string MountSuccess = "Disque monté avec succès dans WSL.";
        public const string UnmountSuccess = "Disque démonté avec succès.";
        public const string ImportSuccess = "Distribution '{0}' importée avec succès.";
        public const string ImportInPlaceSuccess = "Distribution '{0}' importée en place avec succès.";

        // Messages d'avertissement
        public const string DeletingDistribution = "Suppression de '{0}' en cours... (ATTENTION: Action irréversible!)";
        public const string RestartingDistribution = "Redémarrage de '{0}' en cours...";
        public const string ConvertingDistribution = "Conversion de '{0}' vers WSL {1}...";
        public const string ExportingDistribution = "Export de '{0}' vers Documents...";
        public const string WslUpdateWarning = "La mise à jour de WSL a échoué ou est déjà à jour.";
        public const string InstallationWarning = "L'installation a échoué ou {0} est déjà installé.";
        public const string UpdatingWsl = "Mise à jour de WSL en cours...";
        public const string ImportingDistribution = "Import de '{0}' en cours...";
        public const string MountingDisk = "Montage du disque en cours...";
        public const string UnmountingDisk = "Démontage du disque en cours...";

        // Messages d'erreur
        public const string DistributionStartError = "Impossible de démarrer la distribution '{0}'.";
        public const string DistributionStopError = "Impossible d'arrêter la distribution '{0}'.";
        public const string DistributionRestartError = "Impossible de redémarrer la distribution '{0}'.";
        public const string DistributionDeleteError = "Impossible de supprimer '{0}'. Vérifiez que la distribution n'est pas en cours d'exécution.";
        public const string DistributionSetDefaultError = "Impossible de définir '{0}' comme distribution par défaut.";
        public const string DistributionConvertError = "Impossible de convertir '{0}'.";
        public const string DistributionExportError = "Impossible d'exporter '{0}'.";
        public const string AllDistributionsStopError = "Impossible d'arrêter toutes les distributions.";
        public const string LoadDistributionsError = "Impossible de charger les distributions : {0}";
        public const string TerminalOpenError = "Impossible d'ouvrir le terminal : {0}";
        public const string ExplorerOpenError = "Impossible d'ouvrir l'explorateur : {0}";
        public const string InfoRetrievalError = "Impossible de récupérer les informations : {0}";
        public const string GenericError = "Erreur lors de {0} : {1}";
        public const string ImportError = "Impossible d'importer la distribution : {0}";
        public const string MountError = "Impossible de monter le disque : {0}";
        public const string UnmountError = "Impossible de démonter le disque : {0}";
        public const string DefaultVersionError = "Impossible de définir la version WSL par défaut : {0}";
        public const string WslStatusError = "Impossible de récupérer l'état WSL : {0}";
        public const string WslVersionError = "Impossible de récupérer la version WSL : {0}";

        // Titres
        public const string TitleSuccess = "Succès";
        public const string TitleError = "Erreur";
        public const string TitleWarning = "Attention";
        public const string TitleInfo = "Information";
        public const string TitleLoading = "Chargement réussi";
        public const string TitleDeletion = "Suppression";
        public const string TitleRestart = "Redémarrage";
        public const string TitleConversion = "Conversion";
        public const string TitleExport = "Export";
        public const string TitleUpdate = "Mise à jour";
        public const string TitleInstallation = "Installation";
        public const string TitleTerminal = "Terminal";
        public const string TitleExplorer = "Explorateur";
        public const string TitleModification = "Modification réussie";
        public const string TitleShutdown = "Arrêt complet";
        public const string TitleNoDistributions = "Aucune distribution";
        public const string TitleImport = "Import";
        public const string TitleMount = "Montage de disque";
        public const string TitleUnmount = "Démontage de disque";
        public const string TitleWslStatus = "État WSL";
        public const string TitleWslVersion = "Version WSL";
        public const string TitleDefaultVersion = "Version par défaut";
        public const string TitleUserSelection = "Sélection d'utilisateur";

        // Confirmations
        public const string DeleteConfirmationTitle = "Confirmer la suppression";
        public const string DeleteConfirmationMessage = "Êtes-vous sûr de vouloir supprimer '{0}' ?\n\nCette action est IRRÉVERSIBLE et supprimera toutes les données de la distribution.";
        public const string DeleteConfirmButton = "Supprimer";
        public const string CancelButton = "Annuler";
    }

    /// <summary>
    /// Constantes WSL
    /// </summary>
    public static class WslConstants
    {
        /// <summary>
        /// Chemin réseau pour accéder aux distributions (Windows 11/10 récent)
        /// </summary>
        public const string NetworkPathModern = "\\\\wsl.localhost\\{0}";

        /// <summary>
        /// Chemin réseau pour accéder aux distributions (Windows 10 ancien)
        /// </summary>
        public const string NetworkPathLegacy = "\\\\wsl$\\{0}";

        /// <summary>
        /// Liste des distributions populaires disponibles
        /// </summary>
        public static readonly string[] PopularDistributions = new[]
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

        /// <summary>
        /// États possibles d'une distribution
        /// </summary>
        public const string StateRunning = "Running";
        public const string StateStopped = "Stopped";
        public const string StateInstalling = "Installing";

        /// <summary>
        /// Filtres de fichiers pour les dialogues
        /// </summary>
        public static readonly Dictionary<string, List<string>> FileFilters = new()
        {
            { "TAR Archives", new List<string> { ".tar", ".tar.gz", ".tgz" } },
            { "VHD Files", new List<string> { ".vhd", ".vhdx" } },
            { "All Files", new List<string> { "*" } }
        };

        /// <summary>
        /// Types de systèmes de fichiers supportés pour le montage
        /// </summary>
        public static readonly string[] FileSystemTypes = new[]
        {
            "ext4",
            "vfat",
            "ntfs",
            "btrfs",
            "xfs"
        };

        /// <summary>
        /// Emplacement par défaut pour les imports de distributions
        /// </summary>
        public static string DefaultImportLocation =>
            System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "WSL",
                "Imports"
            );
    }
}
