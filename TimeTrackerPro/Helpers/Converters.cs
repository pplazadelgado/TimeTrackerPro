using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
}
