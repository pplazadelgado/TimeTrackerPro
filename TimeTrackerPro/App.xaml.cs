using System.Windows;
using TimeTrackerPro.Infrastructure;
using TimeTrackerPro.Repositories;
using TimeTrackerPro.Services;
using TimeTrackerPro.Views;
using QuestPDF.Infrastructure;

namespace TimeTrackerPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Guardamos el contexto como propiedad estática para
        // que toda la app pueda acceder. En fases posteriores
        // usaremos inyección de dependencias formal.
        public static DatabaseContext Database { get; private set; } = null!;
        public static IProjectRepository Projects {  get; private set; } = null!;
        public static ISectionRepository Sections { get; private set; } = null!;
        public static IWorkSessionRepository WorkSessions {  get; private set; } = null!;
        public static TimerService Timer {  get; private set; } = null!;
        public static IExpenseRepository Expenses { get; private set; } = null!;
        public static ReportService Reports { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            base.OnStartup(e);

            var splash = new SplashScreenWindow();
            splash.Show();

            try
            {
                var minimumDisplay = Task.Delay(TimeSpan.FromSeconds(2));

                splash.SetStatus("Abriendo base de datos...");
                Database = new DatabaseContext();
                await Database.InitializeAsync();

                splash.SetStatus("Cargando repositorios...");
                Projects = new ProjectRepository(Database);
                Sections = new SectionRepository(Database);
                WorkSessions = new WorkSessionRepository(Database);
                Expenses = new ExpenseRepository(Database);
                Reports = new ReportService();

                splash.SetStatus("Restaurando sesión activa...");
                Timer = new TimerService(WorkSessions);
                await Timer.RestoreActiveSessionAsync();

                await minimumDisplay;

                var mainWindow = new MainWindow();
                mainWindow.Show();
                splash.Close();
            }
            catch (Exception ex)
            {
                splash.Close();
                MessageBox.Show(
                    $"Error al inicializar la base de datos:\n\n{ex.Message}",
                    "Error de inicio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }
        
        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try
            {
                // Si hay cronometro activo al cerrar, lo paramos
                if (Timer.IsRunning)
                    await Timer.StopAsync();
            }
            catch
            {
                // Si algo falla al cerrar, no hacemos nada
                // no queremos errores en el cierre de la app
            }
        }
    }

}
