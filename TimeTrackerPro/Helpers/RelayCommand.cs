// Helpers/RelayCommand.cs
using System.Windows.Input;

namespace TimeTrackerPro.Helpers
{
    /// <summary>
    /// Implementación genérica de ICommand para usar en ViewModels.
    ///
    /// CONCEPTO CLAVE: En MVVM, los botones de la UI no deben llamar
    /// directamente a métodos del código. En su lugar, se "enlazan" a
    /// un Command. Esto desacopla la UI de la lógica.
    ///
    /// Ejemplo de uso en ViewModel:
    ///   public ICommand SaveCommand => new RelayCommand(Save, CanSave);
    ///
    /// Ejemplo en XAML:
    ///   <Button Command="{Binding SaveCommand}" Content="Guardar"/>
    /// </summary>
    public class RelayCommand : ICommand
    {
        // La acción que se ejecuta cuando el comando se invoca
        private readonly Action<object?> _execute;

        // Función opcional que determina si el comando puede ejecutarse
        // (habilita o deshabilita el botón automáticamente)
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Constructor principal.
        /// </summary>
        /// <param name="execute">Método que se ejecuta al hacer clic</param>
        /// <param name="canExecute">Función que indica si el botón está activo (opcional)</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            // Guardamos la referencia a la acción (no null gracias al operador ??)
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor simplificado para cuando no necesitamos parámetro
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute == null ? null : _ => canExecute())
        { }

        /// <summary>
        /// WPF llama a este método para saber si debe habilitar o deshabilitar
        /// el botón. Si no hay función canExecute, siempre devuelve true.
        /// </summary>
        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Se ejecuta cuando el usuario hace clic en el botón.
        /// Envuelto en try-catch para capturar errores inesperados.
        /// </summary>
        public void Execute(object? parameter)
        {
            try
            {
                _execute(parameter);
            }
            catch (Exception ex)
            {
                // Por ahora lanzamos, en fases posteriores conectaremos Serilog aquí
                throw new InvalidOperationException(
                    $"Error ejecutando el comando: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Este evento permite forzar a WPF a re-evaluar CanExecute.
        /// Se usa cuando cambia el estado y queremos que el botón se
        /// habilite o deshabilite sin que el usuario haga nada.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Método manual para forzar la re-evaluación del estado del botón.
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Versión genérica de RelayCommand que acepta un parámetro tipado.
    /// Uso: new RelayCommand&lt;Project&gt;(p => DeleteProject(p))
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?,bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) =>
            _canExecute ==null || _canExecute(parameter is T t ? t:default);

        public void Execute(object? parameter) => 
            _execute(parameter is T t ? t :default);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
