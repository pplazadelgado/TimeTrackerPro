using System.Globalization;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TimeTrackerPro.ViewModels;

namespace TimeTrackerPro.Helpers
{
    /// <summary>
    /// Convierte true → Visible, false → Collapsed.
    /// Uso en XAML: Visibility="{Binding MiBooleano,
    ///     Converter={StaticResource BoolToVisibilityConverter}}"
    /// </summary>
     public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? Visibility.Visible :Visibility.Collapsed;

        public object ConvertBack(object value, Type target, object parameter, CultureInfo culture)
            => value is Visibility.Visible;
    }

    /// <summary>
    /// Lo contrario: true → Collapsed, false → Visible.
    /// Lo usamos para mostrar la pantalla de bienvenida cuando
    /// el formulario NO está visible.
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
            => value is true ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => value is not Visibility.Visible;
    }

    /// <summary>
    /// Convierte un porcentaje (0-100) en un ancho en píxeles
    /// relativo al ancho del contenedor padre.
    /// Uso: barra de progreso proporcional al contenedor.
    /// </summary>
    public class PercentageToWidthConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0.0;

            var percentage = System.Convert.ToDouble(values[0]);
            var totalWidth = System.Convert.ToDouble(values[1]);

            if (totalWidth <= 0 || double.IsNaN(totalWidth)) return 0.0;
            return Math.Max(0, Math.Min(totalWidth, totalWidth * percentage / 100));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Devuelve Visible si los dos valores que recibe son el mismo objeto.
    /// Lo usamos para mostrar el formulario de edición solo en la fila
    /// cuya sección coincide con la que está siendo editada en el ViewModel.
    /// </summary>
    public class EqualToVisibilityConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Collapsed;
            return Equals(values[0], values[1])
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Inverso: Collapsed si son iguales, Visible si son distintos.
    /// Para ocultar la fila de lectura cuando esa sección se está editando.
    /// </summary>
    public class NotEqualToVisibilityConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values,Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Visible;
            return Equals(values[0], values[1])
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, System.Globalization.CultureInfo culture)
                => throw new NotImplementedException();
    }

    /// <summary>
    /// Convierte un entero a Visibility: Visible si count > 0, Collapsed si es 0.
    /// Usado para mostrar el historial de sesiones solo cuando hay sesiones.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Convierte un DeviationLevel en un color de fondo para la tarjeta de desviación.
    /// Verde (#DCF0CD) = en plazo, Ámbar (#F6E2A6) = leve, Rojo (#F4DAD6) = retraso.
    /// </summary>
    public class DeviationToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = value is ProjectDetailVewModel.DeviationLevel dl
                ? dl
                : ProjectDetailVewModel.DeviationLevel.OnTrack;

            var hex = level switch
            {
                ProjectDetailVewModel.DeviationLevel.SlightDelay => "#F6E2A6",
                ProjectDetailVewModel.DeviationLevel.Delayed      => "#F4DAD6",
                _                                                  => "#DCF0CD"
            };

            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Convierte un ProjectStatus en un color de fondo para la etiqueta.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not TimeTrackerPro.Models.ProjectStatus status)
                return System.Windows.Media.Brushes.Gray;

            return status switch
            {
                Models.ProjectStatus.Active =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#27AE60")),
                Models.ProjectStatus.Paused =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#F39C12")),
                Models.ProjectStatus.Completed =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#3498DB")),
                Models.ProjectStatus.Archived =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#95A5A6")),
                _ => System.Windows.Media.Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Collapsed si el valor es null, Visible si tiene valor.</summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value != null ? Visibility.Visible:Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();

    }

    // <summary>
    /// Devuelve el color de texto apropiado según el nivel de desviación.
    /// </summary>
    public class DeviationToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not TimeTrackerPro.Models.DeviationLevel level)
                return System.Windows.Media.Brushes.Gray;

            return level switch
            {
                Models.DeviationLevel.OnTrack =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#2D5A1B")),
                Models.DeviationLevel.SlightDelay =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#7A4F00")),
                Models.DeviationLevel.Delayed =>
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString("#7A1F1F")),
                _ => System.Windows.Media.Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Convierte un double de horas al formato HH:mm.
    /// Ejemplo: 1.5 → "01:30", 13.9 → "13:54", 0.25 → "00:15"
    /// </summary>
    public class HoursToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is not double hours) return "00:00";
            var totalMinutes = (int)Math.Round(hours * 60);
            var h = totalMinutes / 60;
            var m = totalMinutes % 60;
            return $"{h:D2}:{m:D2}";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
