using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TimeTrackerPro.ViewModels
{
    /// <summary>
    /// Clase base para todos los ViewModels de la aplicación.
    /// Implementa INotifyPropertyChanged, que es el mecanismo que permite
    /// a la interfaz gráfica (View) "escuchar" cambios en los datos.
    ///
    /// CONCEPTO CLAVE: Cuando una propiedad cambia en el ViewModel, WPF
    /// necesita saberlo para actualizar la pantalla. Este evento es el
    /// "mensajero" que avisa a la UI de que algo ha cambiado.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        // Este evento es el contrato de INotifyPropertyChanged.
        // La UI se suscribe a él automáticamente cuando usas Binding en XAML.
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Lanza el evento PropertyChanged para avisar a la UI de que
        /// una propiedad ha cambiado y debe redibujar ese elemento.
        ///
        /// [CallerMemberName] es un "truco" de C# que detecta automáticamente
        /// el nombre del método o propiedad que llama a este método,
        /// así no tenemos que escribirlo a mano cada vez.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que cambió (auto-detectado)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Método de ayuda para actualizar el valor de una propiedad
        /// y notificar el cambio solo si el valor es diferente.
        /// Esto evita notificaciones innecesarias que ralentizarían la UI.
        ///
        /// USO: En lugar de escribir 10 líneas en cada propiedad,
        /// simplemente llamas a SetProperty(ref _campo, valor).
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            // Si el valor no ha cambiado, no hacemos nada
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
