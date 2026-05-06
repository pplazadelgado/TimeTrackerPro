using System.Windows;

namespace TimeTrackerPro.Helpers
{
    /// <summary>
    /// Permite acceder al DataContext del UserControl desde dentro
    /// de un DataTemplate, donde el contexto cambia al objeto de la fila.
    ///
    /// CONCEPTO: Dentro de un DataTemplate, el DataContext es el objeto
    /// de cada fila (una Section). El Proxy "congela" el DataContext
    /// del UserControl para poder acceder a él desde cualquier nivel.
    /// </summary>
    public class BindingProxy :Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();
        
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty=
            DependencyProperty.Register("Data", typeof(object), 
                typeof(BindingProxy), new PropertyMetadata(null));
        
    }
}
