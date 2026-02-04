using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace WSL_Manager.Converters
{
    /// <summary>
    /// Convertit l'état d'exécution (bool) en couleur
    /// true (Running) = Vert, false (Stopped) = Gris
    /// </summary>
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isRunning)
            {
                return new SolidColorBrush(isRunning
                    ? Colors.LimeGreen  // Vert pour "Running"
                    : Colors.Gray);      // Gris pour "Stopped"
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
