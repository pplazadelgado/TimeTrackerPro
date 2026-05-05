using System.Windows.Input;
using TimeTrackerPro.Helpers;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación.
    /// En fases posteriores, este ViewModel gestionará la navegación
    /// entre las distintas secciones (proyectos, cronómetro, gastos).
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // ——— Campos privados (backing fields) ———
        private string _title = "TimeTracker Pro";
        private string _statusMessage = "Listo";
        private int _clickCount = 0;
        private int _projectCount;
        public int ProjectCount
        {
            get => _projectCount;
            set => SetProperty(ref _projectCount, value);
        }

        // ——— Propiedades públicas (las que ve la UI) ———

        /// <summary>Título de la ventana, enlazado en XAML con {Binding Title}</summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value); // SetProperty notifica a la UI
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int ClickCount
        {
            get => _clickCount;
            set => SetProperty(ref _clickCount, value);
        }

        // ——— Comandos ———

        /// <summary>
        /// Comando de prueba para verificar que el binding funciona.
        /// En XAML: Command="{Binding TestCommand}"
        /// </summary>
        public ICommand TestCommand { get; }

        // ——— Constructor ———
        public MainViewModel()
        {
            // Inicializamos el comando con la acción que debe ejecutar
            TestCommand = new RelayCommand(OnTestClick);
            _ = LoadInitialDataAsync(); // Cargamos datos al iniciar
        }

        // ——— Métodos privados (lógica) ———

        /// <summary>
        /// Se ejecuta cada vez que el usuario pulsa el botón de prueba.
        /// Nota: los métodos que responden a comandos suelen tener el prefijo "On".
        /// </summary>
        private void OnTestClick()
        {
            ClickCount++;
            StatusMessage = $"Último clic: {DateTime.Now:HH:mm:ss}";
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var projects = await App.Projects.GetAllAsync();
                ProjectCount = projects.Count();
                StatusMessage = $"BD conectada - {ProjectCount} proyectos(s) cargado(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar datos: {ex.Message}";
            }
        }
    }
}
