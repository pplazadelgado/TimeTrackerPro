using System.Windows;
using TimeTrackerPro.Infrastructure;
using TimeTrackerPro.Repositories;
using TimeTrackerPro.Services;
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

            try
            {
                Database = new DatabaseContext();
                await Database.InitializeAsync();

                Projects = new ProjectRepository(Database);
                Sections = new SectionRepository(Database);
                WorkSessions = new WorkSessionRepository(Database);
                Timer = new TimerService (WorkSessions);
                await Timer.RestoreActiveSessionAsync();
                Expenses = new ExpenseRepository(Database);
                Reports = new ReportService();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar la base de datos:\n\n{ex.Message}",
                    "Error de inicio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Si no podemos iniciar la BD, cerramos la app
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
