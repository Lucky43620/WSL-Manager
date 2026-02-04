using System;

namespace WSL_Manager.Services
{
    /// <summary>
    /// Type de notification à afficher
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Information générale
        /// </summary>
        Info,

        /// <summary>
        /// Opération réussie
        /// </summary>
        Success,

        /// <summary>
        /// Avertissement
        /// </summary>
        Warning,

        /// <summary>
        /// Erreur
        /// </summary>
        Error
    }

    /// <summary>
    /// Données d'une notification
    /// </summary>
    public class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Message de la notification
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Type de notification
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Titre optionnel de la notification
        /// </summary>
        public string? Title { get; set; }
    }

    /// <summary>
    /// Service de notifications pour afficher des messages à l'utilisateur
    /// Permet d'afficher des infos, succès, avertissements et erreurs
    /// </summary>
    public class NotificationService
    {
        /// <summary>
        /// Instance singleton du service
        /// </summary>
        public static NotificationService Instance { get; } = new NotificationService();

        /// <summary>
        /// Événement déclenché quand une notification doit être affichée
        /// </summary>
        public event EventHandler<NotificationEventArgs>? NotificationRequested;

        private NotificationService()
        {
        }

        /// <summary>
        /// Affiche une notification d'information
        /// </summary>
        /// <param name="message">Message à afficher</param>
        /// <param name="title">Titre optionnel</param>
        public void ShowInfo(string message, string? title = null)
        {
            Show(message, NotificationType.Info, title);
        }

        /// <summary>
        /// Affiche une notification de succès
        /// </summary>
        /// <param name="message">Message à afficher</param>
        /// <param name="title">Titre optionnel</param>
        public void ShowSuccess(string message, string? title = null)
        {
            Show(message, NotificationType.Success, title);
        }

        /// <summary>
        /// Affiche une notification d'avertissement
        /// </summary>
        /// <param name="message">Message à afficher</param>
        /// <param name="title">Titre optionnel</param>
        public void ShowWarning(string message, string? title = null)
        {
            Show(message, NotificationType.Warning, title);
        }

        /// <summary>
        /// Affiche une notification d'erreur
        /// </summary>
        /// <param name="message">Message à afficher</param>
        /// <param name="title">Titre optionnel</param>
        public void ShowError(string message, string? title = null)
        {
            Show(message, NotificationType.Error, title);
        }

        /// <summary>
        /// Affiche une notification
        /// </summary>
        private void Show(string message, NotificationType type, string? title = null)
        {
            NotificationRequested?.Invoke(this, new NotificationEventArgs
            {
                Message = message,
                Type = type,
                Title = title
            });
        }
    }
}
