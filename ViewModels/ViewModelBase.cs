using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WSL_Manager.ViewModels
{
    /// <summary>
    /// Classe de base pour tous les ViewModels
    /// Implémente INotifyPropertyChanged pour notifier l'UI des changements
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifie l'UI qu'une propriété a changé
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Modifie la valeur d'une propriété et notifie l'UI si la valeur change
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
