using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using TimeTrackerPro.Helpers;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.ViewModels
{
    /// <summary>
    /// ViewModel que gestiona la lista de proyectos y el formulario de creación.
    ///
    /// CONCEPTO ObservableCollection: Es como una List<T> normal pero con
    /// un superpoder: cuando añades o quitas elementos, avisa a la UI
    /// automáticamente para que actualice la lista en pantalla.
    /// Una List<T> normal no haría eso.
    /// </summary>
    class ProjectListViewModel : BaseViewModel
    {
        // ——— Campos privados ———
        private ObservableCollection<Project> _projects = new();
        private Project? _selectedProject;
        private bool _isFormVisible;
        private bool _isLoading;
        private string _errorMessge = string.Empty;

        //Campos del formulario ne nuevo proyecto
        private string _newProjectName = string.Empty;
        private string _newProjectDescription = string.Empty;
        private double _newProjectWeeklyHours = 20;

        // ——— Propiedades públicas ———

        /// <summary>
        /// Lista de proyectos que se muestra en la UI.
        /// ObservableCollection notifica a la UI cuando cambia.
        /// </summary>
        public ProjectDetailVewModel DetailViewModel { get; } = new();
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        /// <summary>Proyecto seleccionado en la lista.</summary>
        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                SetProperty(ref _selectedProject, value);
                OnPropertyChanged(nameof(ShowWelcome));
                if (value != null)
                    _ = DetailViewModel.LoadProyectAsync(value.Id);
                else
                    DetailViewModel.Project = null;
            }
        }

        /// <summary>Controla si el formulario de nuevo proyecto está visible.</summary>
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                SetProperty(ref _isFormVisible, value);
                OnPropertyChanged(nameof(ShowWelcome));
            }
        }

        /// <summary>True cuando no hay proyecto seleccionado ni formulario abierto.</summary>
        public bool ShowWelcome => !IsFormVisible && _selectedProject == null;

        /// <summary>Controla el indicador de carga mientras se accede a la BD.</summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessge;
            set => SetProperty(ref _errorMessge, value);
        }

        /// <summary>True si hay un mensaje de error que mostrar.</summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // ——— Propiedades del formulario ———
        public string NewProjectName
        {
            get => _newProjectName;
            set
            {
                SetProperty(ref _newProjectName, value);
                OnPropertyChanged(nameof(CanSaveNewProject));
            }
        }

        public string NewProjectDescription
        {
            get => _newProjectDescription;
            set => SetProperty(ref _newProjectDescription, value);
        }

        public double NewProjectWeeklyHours
        {
            get => _newProjectWeeklyHours;
            set => SetProperty(ref _newProjectWeeklyHours, value);
        }

        /// <summary>
        /// El botón Guardar solo se activa si el nombre no está vacío.
        /// RelayCommand llama a esto para habilitar/deshabilitar el botón.
        /// </summary>
        public bool CanSaveNewProject =>
            !string.IsNullOrWhiteSpace(NewProjectName);

        // ——— Comandos ———
        public ICommand LoadProjectsCommand { get; }
        public ICommand ShowNewProjectFormCommand { get; }
        public ICommand SaveNewProjectCommand { get; }
        public ICommand CancelNewProjectCommand { get; }
        public ICommand DeleteProjectCommand {  get; }

        // ——— Constructor ———
        public ProjectListViewModel()
        {
            LoadProjectsCommand = new RelayCommand(async () => await LoadProjectsAsync());
            ShowNewProjectFormCommand = new RelayCommand(ShowNewProjectForm);
            SaveNewProjectCommand = new RelayCommand(
                async () => await SaveNewProjectAsync(),
                () => CanSaveNewProject);
            CancelNewProjectCommand = new RelayCommand(CancelNewProject);
            DeleteProjectCommand = new RelayCommand<Project>(
                async p => await DeleteProyectAsync(p));

            //Cargamos los proyectos nada mas crear el ViewModel
            _ = LoadProjectsAsync();
        }

        // ——— Métodos privados ———
        private async Task LoadProjectsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var projects = await App.Projects.GetAllAsync();

                Projects.Clear();
                foreach (var p in projects)
                    Projects.Add(p);
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al cargar proyectos: {ex.Message}";
                OnPropertyChanged(nameof(HasError));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowNewProjectForm()
        {
            SelectedProject = null;
            NewProjectName = string.Empty;
            NewProjectDescription = string.Empty;
            NewProjectWeeklyHours = 20;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(HasError));
            IsFormVisible = true;
        }

        private async Task SaveNewProjectAsync()
        {
            try
            {
                IsLoading = true;

                var project = new Project
                {
                    Name = NewProjectName.Trim(),
                    Description = NewProjectDescription.Trim(),
                    WeeklyHours = NewProjectWeeklyHours,
                    CreatedAt = DateTime.Now,
                    Status = ProjectStatus.Active
                };

                await App.Projects.InsertAsync(project);

                //Añadimos directamente a la coleccion en vez de cargar toda la lista
                Projects.Insert(0, project);

                IsFormVisible = false;
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al guardar el proyecto: {ex.Message}";
                OnPropertyChanged(nameof(HasError));
            }
            finally { IsLoading = false; }
        }

        private void CancelNewProject()
        {
            IsFormVisible = false;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof (HasError));
        }

        private async Task DeleteProyectAsync(Project? project)
        {
            if (project == null) return;

            var owner = System.Windows.Application.Current.MainWindow;
            var confirmed = Views.ConfirmDialog.Show(
                owner: owner,
                title: "Eliminar proyecto",
                message: $"¿Eliminar '{project.Name}'?\n\nSe borrarán también todas sus secciones, sesiones de trabajo y gastos. Esta acción no se puede deshacer.",
                confirmText: "Si, eliminar");

            if (!confirmed) return;

            try
            {
                await App.Projects.DeleteAsync(project.Id);
                Projects.Remove(project);

                if (SelectedProject?.Id == project.Id)
                    SelectedProject = null;
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al eliminar: {ex.Message}";
                OnPropertyChanged(nameof(HasError));
            }
        }
    }
}
