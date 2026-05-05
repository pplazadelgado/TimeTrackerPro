using System.Collections.ObjectModel;
using System.Formats.Asn1;
using System.Windows.Input;
using TimeTrackerPro.Helpers;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.ViewModels
{
    /// <summary>
    /// ViewModel que gestiona el detalle de un proyecto seleccionado.
    /// Muestra secciones, progreso y estimaciones de fecha de fin.
    /// </summary>
     public class ProjectDetailVewModel :BaseViewModel
    {
        // ——— Campos privados ———
        private Project? _project;
        private ObservableCollection<Section> _sections = new();
        private Section? _selectedSection;
        private bool _isAddingSection;
        private bool _isAddingSubSection;
        private Section? _parentSectionForNew;
        private string _newSectionName = string.Empty;
        private double _newSectionEstimateHours;
        private string _errorMessage = string.Empty;
        private string _timerDisplay = "00:00:00";
        private bool _hasActiveTimer;
        private int? _activeTimerSectionId;

        // edición proyecto
        private bool _isEditingProject;
        private string _editProjectName = string.Empty;
        private string _editProjectDescription = string.Empty;
        private double _editProjectWeeklyHours;

        // edición sección inline
        private Section? _editingSection;
        private string _editSectionName = string.Empty;
        private double _editSectionEstimatedHours;

        // ——— Propiedades públicas ———

        public Project? Project
        {
            get => _project;
            set
            {
                SetProperty(ref _project, value);

                // Cuando cambia el proyecto, notificamos todas las
                // propiedades calculadas para que la UI se refresque
                OnPropertyChanged(nameof(ProjectName));
                OnPropertyChanged(nameof(TotalEstimatedHours));
                OnPropertyChanged(nameof(TotalWorkedHours));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(EstimatedEndDate));
                OnPropertyChanged(nameof(DeviationStatus));
                OnPropertyChanged(nameof(HasProject));
            }
        }

        public bool HasProject => _project != null;

        public ObservableCollection<Section> Sections
        {
            get => _sections;
            set=> SetProperty(ref _sections, value);
        }

        public Section? SelectedSection
        {
            get => _selectedSection;
            set => SetProperty(ref _selectedSection, value);
        }

        public bool IsAddingSectionOrSub => _isAddingSection || _isAddingSubSection;

        public bool IsAddingSection
        {
            get => _isAddingSection;
            set
            {
                SetProperty(ref _isAddingSection, value);
                OnPropertyChanged(nameof(IsAddingSectionOrSub));
                OnPropertyChanged(nameof(NewSectionFormTitle));
            }
        }

        public string NewSectionFormTitle =>
            _isAddingSubSection
                ? $"Nueva subseccion en '{_parentSectionForNew.Name}"
                : "Nueva seccion";

        public string NewSectionName
        {
            get => _newSectionName;
            set
            {
                SetProperty(ref _newSectionName, value);
                OnPropertyChanged(nameof(CanSaveSection));
            }
        }

        public double NewSectionEstimatedHours
        {
            get => _newSectionEstimateHours;
            set => SetProperty(ref _newSectionEstimateHours, value);
        }

        public bool CanSaveSection =>
            !string.IsNullOrWhiteSpace(NewSectionName);

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // ——— Propiedades edición proyecto ———

        public bool IsEditingProject
        {
            get => _isEditingProject;
            set => SetProperty(ref _isEditingProject, value);
        }

        public string EditProjectName
        {
            get => _editProjectName;
            set => SetProperty(ref _editProjectName, value);
        }

        public string EditProjectDescription
        {
            get => _editProjectDescription;
            set => SetProperty(ref _editProjectDescription, value);
        }

        public double EditProjectWeeklyHours
        {
            get => _editProjectWeeklyHours;
            set => SetProperty(ref _editProjectWeeklyHours, value);
        }

        // ——— Propiedades edición sección inline ———

        public Section? EditingSection
        {
            get => _editingSection;
            set => SetProperty(ref _editingSection, value);
        }

        public string EditSectionName
        {
            get => _editSectionName;
            set => SetProperty(ref _editSectionName, value);
        }

        public double EditSectionEstimatedHours
        {
            get => _editSectionEstimatedHours;
            set => SetProperty(ref _editSectionEstimatedHours, value);
        }

        // ——— Propiedades calculadas del proyecto ———

        public string ProjectName => _project?.Name ?? string.Empty;

        public double TotalEstimatedHours =>
            _sections.Sum(s => s.EstimatedHours);

        public double TotalWorkedHours =>
            _sections.Sum(s =>
                s.TotalWorkedHours +
                s.SubSections.Sum(ss => ss.TotalWorkedHours));

        public double ProgressPercentage =>
            TotalEstimatedHours > 0
                ? Math.Min(100, (TotalWorkedHours / TotalEstimatedHours)*100)
                : 0;

        /// <summary>
        /// Calcula la fecha estimada de fin basándose en:
        /// horas restantes / horas disponibles por semana.
        /// </summary>
        public string EstimatedEndDate
        {
            get
            {
                if (_project == null || _project.WeeklyHours <= 0)
                    return "Sin datos";

                var remainingHours = TotalEstimatedHours - TotalWorkedHours;
                if(remainingHours < 0) return "Completado";

                var weeksNeeded = remainingHours / _project.WeeklyHours;
                var estimatedEnd = DateTime.Now.AddDays(weeksNeeded * 7);

                return estimatedEnd.ToString("dd MMM yyyy");
            }
        }

        /// <summary>
        /// Indica visualmente si vamos bien, con retraso leve o con retraso grave.
        /// Verde: menos del 110% de lo estimado
        /// Amarillo: entre 110% y 130%
        /// Rojo: más del 130%
        /// </summary>
        public DeviationLevel DeviationStatus
        {
            get
            {
                if (TotalEstimatedHours <= 0) return DeviationLevel.OnTrack;
                var ratio = TotalWorkedHours / TotalEstimatedHours;
                if (ratio <= 1.10) return DeviationLevel.OnTrack;
                if (ratio <= 1.30) return DeviationLevel.SlightDelay;
                return DeviationLevel.Delayed;
            }
        }

        public string TimerDisplay
        {
            get => _timerDisplay;
            set => SetProperty(ref _timerDisplay, value);
        }

        /// <summary>True si hay un cronómetro activo en alguna sección de este proyecto.</summary>
        public bool HasActiveTimer
        {
            get => _hasActiveTimer;
            set => SetProperty(ref _hasActiveTimer, value);
        }

        public int? ActiveTimerSectionId
        {
            get => _activeTimerSectionId;
            set => SetProperty(ref _activeTimerSectionId, value);
        }

        // ——— Comandos ———
        public ICommand ShowAddSectionCommand { get; }
        public ICommand ShowAddSubSectionCommand { get; }
        public ICommand SaveSectionCommand { get; }
        public ICommand CancelSectionCommand { get; }
        public ICommand DeleteSectionCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand CancelEditProjectCommand { get; }
        public ICommand EditSectionCommand { get; }
        public ICommand SaveSectionEditCommand { get; }
        public ICommand CancelEditSectionCommand { get; }
        public ICommand StartTimeCommand {  get; }
        public ICommand StopTimerCommand { get; }
        public ICommand AddManualSessionCommand { get; }
        public ICommand DeleteSessionCommand { get; }

        // ——— Constructor ———
        public ProjectDetailVewModel()
        {
            ShowAddSectionCommand = new RelayCommand(ShowAddSection);
            ShowAddSubSectionCommand = new RelayCommand<Section>(ShowAddSubSection);
            SaveSectionCommand = new RelayCommand(
                async () => await SaveSectionAsync(),
                () => CanSaveSection);
            CancelSectionCommand = new RelayCommand(CancelSection);
            DeleteSectionCommand = new RelayCommand<Section>(
                async s => await DeleteSectionAsync(s));
            EditProjectCommand = new RelayCommand(StartEditProject);
            SaveProjectCommand = new RelayCommand(async () => await SaveProjectAsync());
            CancelEditProjectCommand = new RelayCommand(() => IsEditingProject = false);
            EditSectionCommand = new RelayCommand<Section>(StartEditSection);
            SaveSectionEditCommand = new RelayCommand(async () => await SaveSectionEditAsync());
            CancelEditSectionCommand = new RelayCommand(() => EditingSection = null);
            StartTimeCommand = new RelayCommand<Section>(async s => await StartTimerAsync(s));
            StopTimerCommand = new RelayCommand(async () => await StopTimerAsync());
            AddManualSessionCommand = new RelayCommand<Section>(
                async s => await AddManualSessionCommand(s));
            DeleteSessionCommand = new RelayCommand<WorkSession>(
                async ws => await DeleteSessionAsync(ws));

            App.Timer.TimerTick += OnTimerTick;
            App.Timer.IsRunningChanged += OnTimerRunningChanged;

            SyncTimerState();
        }

        // ——— Métodos públicos ———

        /// <summary>
        /// Carga el proyecto completo desde la BD y actualiza todas las
        /// propiedades. Lo llama ProjectListViewModel al seleccionar un proyecto.
        /// </summary>
        public async Task LoadProyectAsync(int projectId)
        {
            try
            {
                ErrorMessage = string.Empty;
                var project = await App.Projects.GetByIdAsync(projectId);
                Project = project;

                Sections.Clear();
                if (project?.Sections != null)
                    foreach (var s in project.Sections)
                        Sections.Add(s);

                RefreshCalculations();
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al cargar el proyecto: {ex.Message}";
            }
        }

        // ——— Métodos privados ———
        private void ShowAddSection()
        {
            _parentSectionForNew = null;
            NewSectionName = string.Empty;
            NewSectionEstimatedHours = 0;
            _isAddingSubSection = false;
            IsAddingSection = true;
        }

        private void ShowAddSubSection(Section? parent)
        {
            if (parent == null) return;
            _parentSectionForNew = parent;
            NewSectionName = string.Empty;
            NewSectionEstimatedHours = 0;
            IsAddingSection = false;
            _isAddingSubSection = true;
            OnPropertyChanged(nameof(IsAddingSectionOrSub));
            OnPropertyChanged(nameof(NewSectionFormTitle));
        }

        private async Task SaveSectionAsync()
        {
            if (_project == null) return;

            try
            {
                var section = new Section
                {
                    ProjectId = _project.Id,
                    ParentSectionId = _parentSectionForNew?.Id,
                    Name = NewSectionName.Trim(),
                    EstimatedHours = NewSectionEstimatedHours,
                    Order = Sections.Count,
                    CreatedAt = DateTime.Now,
                    Status = SectionStatus.Pending
                };

                await App.Sections.InsertAsync(section);

                // Añadimos a la colección en el lugar correcto
                if (_parentSectionForNew == null)
                {
                    Sections.Add(section);
                }
                else
                {
                    _parentSectionForNew.SubSections.Add(section);
                    // Forzamos refresco del item padre en la UI
                    var idx = Sections.IndexOf(_parentSectionForNew);
                    if (idx >= 0)
                    {
                        Sections.RemoveAt(idx);
                        Sections.Insert(idx, _parentSectionForNew);
                    }
                }

                CancelSection();
                RefreshCalculations();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar la sección: {ex.Message}";
            }
        }

        private async Task DeleteSectionAsync(Section? section)
        {
            if(section ==null) return;

            var owner = System.Windows.Application.Current.MainWindow;
            var confirmed = Views.ConfirmDialog.Show(
                owner,
                "Eliminar seccion",
                $"¿Eliminar '{section.Name}'?\n\nSe borrarán también sus subsecciones y todas las sesiones de trabajo registradas.",
                "Si, eliminar");

            if (!confirmed) return;

            try
            {
                await App.Sections.DeleteAsync(section.Id);
                Sections.Remove(section);
                RefreshCalculations();
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al eliminar la seccion: {ex.Message}";
            }
        }

        private void CancelSection()
        {
            IsAddingSection = false;
            _isAddingSubSection = false;
            _parentSectionForNew = null;
            OnPropertyChanged(nameof(IsAddingSectionOrSub));
        }

        /// <summary>
        /// Notifica a la UI que recalcule todas las propiedades derivadas.
        /// Se llama después de cualquier cambio que afecte horas o secciones.
        /// </summary>
        private void RefreshCalculations()
        {
            OnPropertyChanged(nameof(TotalEstimatedHours));
            OnPropertyChanged(nameof(TotalWorkedHours));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(EstimatedEndDate));
            OnPropertyChanged(nameof(DeviationStatus));
        }

        private void StartEditProject()
        {
            if (_project == null) return;
            EditProjectName = _project.Name;
            EditProjectDescription = _project.Description ?? string.Empty;
            EditProjectWeeklyHours = _project.WeeklyHours;
            IsEditingProject = true;
        }

        private async Task SaveProjectAsync()
        {
            if (_project == null) return;
            try
            {
                _project.Name = EditProjectName.Trim();
                _project.Description = EditProjectDescription.Trim();
                _project.WeeklyHours = (int)EditProjectWeeklyHours;
                await App.Projects.UpdateAsync(_project);
                IsEditingProject = false;
                OnPropertyChanged(nameof(ProjectName));
                OnPropertyChanged(nameof(EstimatedEndDate));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar el proyecto: {ex.Message}";
            }
        }

        private void StartEditSection(Section? section)
        {
            if (section == null) return;
            EditSectionName = section.Name;
            EditSectionEstimatedHours = section.EstimatedHours;
            EditingSection = section;
        }

        private async Task SaveSectionEditAsync()
        {
            if (_editingSection == null) return;
            try
            {
                _editingSection.Name = EditSectionName.Trim();
                _editingSection.EstimatedHours = EditSectionEstimatedHours;
                await App.Sections.UpdateAsync(_editingSection);

                // Forzar refresco del item en la lista
                var idx = Sections.IndexOf(_editingSection);
                if (idx >= 0)
                {
                    Sections.RemoveAt(idx);
                    Sections.Insert(idx, _editingSection);
                }
                else
                {
                    // Es una subsección — refrescar la sección padre
                    foreach (var s in Sections)
                    {
                        var subIdx = s.SubSections.IndexOf(_editingSection);
                        if (subIdx >= 0)
                        {
                            var parent = s;
                            var pIdx = Sections.IndexOf(parent);
                            Sections.RemoveAt(pIdx);
                            Sections.Insert(pIdx, parent);
                            break;
                        }
                    }
                }

                EditingSection = null;
                RefreshCalculations();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar la sección: {ex.Message}";
            }
        }

        /// <summary>Niveles de desviación respecto a la estimación.</summary>
        public enum DeviationLevel
        {
            OnTrack,      // Verde: en plazo
            SlightDelay,  // Amarillo: leve retraso
            Delayed       // Rojo: retraso significativo
        }

        
    }
}
