using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Asn1;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private ObservableCollection<Expense> _expenses = new();
        private bool _isAddingExpense;
        private string _newExpenseDescription = string.Empty;
        private double _newExpenseAmount;
        private ExpenseCategory _newExpenseCategory = ExpenseCategory.Other;
        private string _newExpenseReference = string.Empty;
        private ProjectStatus _editProjectStatus;

        public IEnumerable<ProjectStatus> ProjectStatuses => Enum.GetValues<ProjectStatus>();

        // edición proyecto
        private bool _isEditingProject;
        private string _editProjectName = string.Empty;
        private string _editProjectDescription = string.Empty;
        private double _editProjectWeeklyHours;

        // edición sección inline
        private Section? _editingSection;
        private string _editSectionName = string.Empty;
        private double _editSectionEstimatedHours;
        private DateTime? _editSectionDeadline;

        // fechas de entrega (edición)
        private DateTime? _editProjectDeadline;

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
                OnPropertyChanged(nameof(Deviation));
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
                ? $"Nueva subseccion en '{_parentSectionForNew?.Name}"
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

        public DateTime? EditProjectDeadline
        {
            get => _editProjectDeadline;
            set => SetProperty(ref _editProjectDeadline, value);
        }

        public DateTime? EditSectionDeadline
        {
            get => _editSectionDeadline;
            set => SetProperty(ref _editSectionDeadline, value);
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
        /// Fecha estimada de fin basada en horas restantes / horas semanales.
        /// Si hay fecha de entrega, añade cuántos días de margen o retraso hay.
        /// </summary>
        public string EstimatedEndDate
        {
            get
            {
                if (_project == null || _project.WeeklyHours <= 0)
                    return "Sin datos";

                var remaining = TotalEstimatedHours - TotalWorkedHours;
                if (remaining <= 0) return "Completado";

                var weeksNeeded = remaining / _project.WeeklyHours;
                var estimatedEnd = DateTime.Today.AddDays(weeksNeeded * 7);
                var dateStr = estimatedEnd.ToString("dd MMM yyyy");

                if (_project.DeadlineDate.HasValue)
                {
                    var diff = (int)(estimatedEnd.Date - _project.DeadlineDate.Value.Date).TotalDays;
                    if (diff > 0)  return $"{dateStr}  (+{diff}d)";
                    if (diff < 0)  return $"{dateStr}  ({-diff}d margen)";
                }

                return dateStr;
            }
        }

        /// <summary>
        /// Desviación combinada: considera tanto la ratio horas trabajadas/estimadas
        /// como el cumplimiento de la fecha de entrega si está configurada.
        /// </summary>
        public DeviationInfo Deviation
        {
            get
            {
                // Nivel basado en horas
                var level = DeviationLevel.OnTrack;
                if (TotalEstimatedHours > 0)
                {
                    var ratio = TotalWorkedHours / TotalEstimatedHours;
                    if (ratio > 1.30) level = DeviationLevel.Delayed;
                    else if (ratio > 1.10) level = DeviationLevel.SlightDelay;
                }

                var remaining = TotalEstimatedHours - TotalWorkedHours;
                if (remaining <= 0)
                    return new DeviationInfo { Level = level, Summary = "Completado" };

                // Si hay fecha de entrega, cruzamos con la fecha estimada de fin
                if (_project?.DeadlineDate.HasValue == true && _project.WeeklyHours > 0)
                {
                    var deadline = _project.DeadlineDate.Value.Date;
                    var estimatedEnd = DateTime.Today.AddDays(
                        remaining / _project.WeeklyHours * 7).Date;

                    if (estimatedEnd > deadline)
                    {
                        var daysLate = (int)(estimatedEnd - deadline).TotalDays;
                        var dlLevel = daysLate >= 14 ? DeviationLevel.Delayed : DeviationLevel.SlightDelay;
                        if (dlLevel > level) level = dlLevel;
                        return new DeviationInfo
                        {
                            Level = level,
                            Summary = $"Fin est. {estimatedEnd:dd MMM yyyy} · {daysLate}d sobre entrega ({deadline:dd MMM})"
                        };
                    }
                    else
                    {
                        var margin = (int)(deadline - estimatedEnd).TotalDays;
                        return new DeviationInfo
                        {
                            Level = level,
                            Summary = $"Fin est. {estimatedEnd:dd MMM yyyy} · {margin}d de margen ({deadline:dd MMM})"
                        };
                    }
                }

                // Sin fecha de entrega — resumen solo de horas
                var excess = TotalWorkedHours - TotalEstimatedHours;
                var summary = level switch
                {
                    DeviationLevel.OnTrack    => "En plazo",
                    DeviationLevel.SlightDelay => $"+{excess:F1}h sobre estimación",
                    _                          => $"+{excess:F1}h · retraso significativo"
                };
                return new DeviationInfo { Level = level, Summary = summary };
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

        // ProjectDetailViewModel.cs — reemplaza ActiveTimerSectionId
        public int? ActiveTimerSectionId
        {
            get => _activeTimerSectionId;
            set
            {
                // Desactivamos la sección anterior
                if (_activeTimerSectionId.HasValue)
                {
                    var prev = FindSection(_activeTimerSectionId.Value);
                    if (prev != null) prev.IsTimerActive = false;
                }

                SetProperty(ref _activeTimerSectionId, value);

                // Activamos la nueva sección
                if (value.HasValue)
                {
                    var next = FindSection(value.Value);
                    if (next != null)
                    {
                        next.IsTimerActive = true;
                        RefreshSection(next);
                    }
                }
            }
        }

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public bool IsAddingExpense
        {
            get => _isAddingExpense;
            set => SetProperty(ref _isAddingExpense, value);
        }

        public string NewExpenseDescription
        {
            get => _newExpenseDescription;
            set
            {
                SetProperty(ref _newExpenseDescription, value);
                OnPropertyChanged(nameof(CanSaveExpense));
            }
        }

        public double NewExpenseAmount
        {
            get => _newExpenseAmount;
            set
            {
                SetProperty(ref _newExpenseAmount, value);
                OnPropertyChanged(nameof(CanSaveExpense));
            }
        }

        public ExpenseCategory NewExpenseCategory
        {
            get => _newExpenseCategory;
            set => SetProperty(ref _newExpenseCategory, value);
        }

        public string NewExpenseReference
        {
            get => _newExpenseReference;
            set => SetProperty(ref _newExpenseReference, value);
        }

        public bool CanSaveExpense =>
            !string.IsNullOrWhiteSpace(NewExpenseDescription) && NewExpenseAmount > 0;

        /// <summary>Lista de categorías disponibles para el ComboBox.</summary>
        public IEnumerable<ExpenseCategory> ExpenseCategories =>
            Enum.GetValues<ExpenseCategory>();

        public double TotalExpenses => _expenses.Sum(e => e.Amount);

        public ProjectStatus EditProjectStatus
        {
            get => _editProjectStatus;
            set => SetProperty(ref _editProjectStatus, value);
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
        public ICommand StartTimerCommand {  get; }
        public ICommand StopTimerCommand { get; }
        public ICommand AddManualSessionCommand { get; }
        public ICommand DeleteSessionCommand { get; }
        public ICommand ShowAddExpenseCommand {  get; }
        public ICommand SaveExpenseCommand { get; }
        public ICommand CancelExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; }
        public ICommand GenerateReportCommand { get; }

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
            StartTimerCommand = new RelayCommand<Section>(async s => await StartTimerAsync(s));
            StopTimerCommand = new RelayCommand(async () => await StopTimerAsync());
            AddManualSessionCommand = new RelayCommand<Section>(
                async s => await AddManualSessionAsync(s));
            DeleteSessionCommand = new RelayCommand<WorkSession>(
                async ws => await DeleteSessionAsync(ws));
            ShowAddExpenseCommand = new RelayCommand(ShowAddExpense);
            SaveExpenseCommand = new RelayCommand(
                async () => await SaveExpenseAsync(),
                () => CanSaveExpense);
            CancelExpenseCommand = new RelayCommand(CancelExpense);
            DeleteExpenseCommand = new RelayCommand<Expense>(
                async e => await DeleteExpenseAsync(e));
            GenerateReportCommand = new RelayCommand(
                async () => await GenerateReportAsync(),
                () => _project != null);

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

                var expenses = await App.Expenses.GetByProyectAsync(projectId);
                Expenses.Clear();
                foreach (var e in expenses)
                    Expenses.Add(e);

                OnPropertyChanged(nameof(TotalExpenses));
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
            OnPropertyChanged(nameof(Deviation));
        }

        private void StartEditProject()
        {
            if (_project == null) return;
            EditProjectName = _project.Name;
            EditProjectDescription = _project.Description ?? string.Empty;
            EditProjectWeeklyHours = _project.WeeklyHours;
            EditProjectStatus = _project.Status;
            EditProjectDeadline = _project.DeadlineDate;
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
                _project.Status = EditProjectStatus;
                _project.DeadlineDate = EditProjectDeadline;
                await App.Projects.UpdateAsync(_project);
                IsEditingProject = false;
                OnPropertyChanged(nameof(ProjectName));
                OnPropertyChanged(nameof(EstimatedEndDate));
                OnPropertyChanged(nameof(Deviation));
                OnPropertyChanged(nameof(Project));
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
            EditSectionDeadline = section.DeadlineDate;
            EditingSection = section;
        }

        private async Task SaveSectionEditAsync()
        {
            if (_editingSection == null) return;
            try
            {
                _editingSection.Name = EditSectionName.Trim();
                _editingSection.EstimatedHours = EditSectionEstimatedHours;
                _editingSection.DeadlineDate = EditSectionDeadline;
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

        /// <summary>
        /// Resultado completo del cálculo de desviación: nivel semáforo + texto explicativo.
        /// Tiene en cuenta tanto las horas como la fecha de entrega si está configurada.
        /// </summary>
        public sealed class DeviationInfo
        {
            public DeviationLevel Level { get; init; }
            public string Summary { get; init; } = string.Empty;
        }

        private async Task StartTimerAsync (Section? section)
        {
            if(section == null) return;
            try
            {
                await App.Timer.StartAsync(section.Id);
                ActiveTimerSectionId = section.Id;
                HasActiveTimer = true;
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al iniciar el cronometro: {ex.Message}";
            }
        }

        private async Task StopTimerAsync()
        {
            try
            {
                var session = await App.Timer.StopAsync();
                if (session == null) return;

                //Añadimos la sesion completada a la seccion correspondiente
                var section = FindSection(session.SectionId);
                if(section != null)
                {
                    section.WorkSessions.Add(session);
                    RefreshSection(section);
                }

                ActiveTimerSectionId = null;
                HasActiveTimer=false;
                TimerDisplay = "00:00:00";
                RefreshCalculations();
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al parar el cronometro: {ex.Message}";
            }
        }

        private async Task AddManualSessionAsync(Section? section)
        {
            if (section == null) return;

            //Abrimos el dialogo de sesion manual
            var dialog = new Views.ManualSessionDialog(section.Name);
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            if (dialog.ShowDialog() != true) return;

            try
            {
                var session = new WorkSession
                {
                    SectionId = section.Id,
                    StartTime = dialog.StartTime,
                    EndTime = dialog.EndTime,
                    Notes = dialog.Notes,
                    IsManual = true
                };

                await App.WorkSessions.InsertAsync(session);
                section.WorkSessions.Add(session);
                RefreshSection(section);
                RefreshCalculations();
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al guardar la sesion: {ex.Message}";
            }
        }

        private async Task DeleteSessionAsync(WorkSession? session)
        {
            if (session == null) return;

            var confirmed = Views.ConfirmDialog.Show(
                System.Windows.Application.Current.MainWindow,
                "Eliminar sesion",
                $"¿Eliminar la sesion del {session.StartTime:dd/MM/yyyy HH:mm}?",
                "Si, eliminar");

            if(!confirmed) return;

            try
            {
                await App.WorkSessions.DeleteAsync(session.Id);

                var section = FindSection(session.SectionId);
                if(section != null)
                {
                    section.WorkSessions.Remove(session);
                    RefreshSection(section);
                }

                RefreshCalculations();
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al eliminar la sesion: {ex.Message}";
            }
        }

        private void OnTimerTick(TimeSpan elapsed)
        {
            TimerDisplay = elapsed.ToString(@"hh\:mm\:ss");
        }

        private void OnTimerRunningChanged(bool isRunning)
        {
            HasActiveTimer = isRunning;
            if (!isRunning)
            {
                ActiveTimerSectionId = null;
                TimerDisplay = "00:00:00";
            }
        }

        private void SyncTimerState()
        {
            HasActiveTimer = App.Timer.IsRunning;
            ActiveTimerSectionId = App.Timer.ActiveSectionId;
        }

        /// <summary>
        /// Busca una sección o subsección por Id en toda la jerarquía.
        /// </summary>
        private Section? FindSection(int sectionId)
        {
            foreach (var s in _sections)
            {
                if (s.Id == sectionId) return s;
                var sub = s.SubSections.FirstOrDefault(ss => ss.Id == sectionId);
                if (sub != null) return sub;
            }
            return null;
        }

        /// <summary>
        /// Fuerza el refresco visual de una sección en la ObservableCollection.
        /// </summary>
        private void RefreshSection(Section section)
        {
            // Si es seccion raiz
            var idx = Sections.IndexOf(section);
            if(idx >= 0)
            {
                Sections.RemoveAt(idx);
                Sections.Insert(idx, section);
                return;
            }

            // Si es subseccion, refrescamos la seccion padre
            var parent = _sections.FirstOrDefault(
                s => s.SubSections.Contains(section) );
            if(parent != null)
            {
                var pidx = Sections.IndexOf(parent);
                if(pidx >= 0)
                {
                    Sections.RemoveAt(pidx);
                    Sections.Insert(pidx, parent);
                }
            }
        }

        private void ShowAddExpense()
        {
            NewExpenseDescription = string.Empty;
            NewExpenseAmount = 0;
            NewExpenseCategory = ExpenseCategory.Other;
            NewExpenseReference = string.Empty;
            IsAddingExpense = true;
        }

        private async Task SaveExpenseAsync()
        {
            if (_project == null) return;
            try
            {
                var expense = new Expense
                {
                    ProjectId = _project.Id,
                    Description = NewExpenseDescription.Trim(),
                    Amount = NewExpenseAmount,
                    Category = NewExpenseCategory,
                    Date = DateTime.Now,
                    Reference = NewExpenseReference.Trim()
                };

                await App.Expenses.InsertAsync(expense);
                Expenses.Insert(0, expense);
                IsAddingExpense = false;
                OnPropertyChanged(nameof(TotalExpenses));
            }
            catch(Exception ex)
            {
                ErrorMessage=$"Error al guardar el gasto: {ex.Message}";
            }
        }

        private void CancelExpense()
        {
            IsAddingExpense = false;
        }

        private async Task DeleteExpenseAsync(Expense? expense)
        {
            if (expense == null) return;

            var confirmed = Views.ConfirmDialog.Show(
                System.Windows.Application.Current.MainWindow,
                "Eliminar gasto",
                $"¿Eliminar '{expense.Description}' ({expense.Amount:C})?",
                "Si, eliminar");

            if (!confirmed) return;

            try
            {
                await App.Expenses.DeleteAsync(expense.Id);
                Expenses.Remove(expense);
                OnPropertyChanged(nameof(TotalExpenses));
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Error al eliminar el gasto: {ex.Message}";
            }
        }

        private async Task GenerateReportAsync()
        {
            if (_project == null) return;

            try
            {
                // Diálogo para elegir dónde guardar el PDF
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Guardar informe",
                    FileName = $"Informe_{_project.Name}_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".pdf",
                    Filter = "PDF|*.pdf"
                };

                if (dialog.ShowDialog() != true) return;

                // Cargamos el proyecto completo con todas las relaciones
                var fullProject = await App.Projects.GetByIdAsync(_project.Id);
                if (fullProject == null) return;

                // Cargamos los gastos (GetByIdAsync no los incluye aún)
                var expenses = await App.Expenses.GetByProyectAsync(_project.Id);
                fullProject.Expenses = expenses.ToList();

                // Generamos el PDF en un hilo secundario para no bloquear la UI
                await Task.Run(() => App.Reports.GenerateProjectReport(
                    fullProject, dialog.FileName));

                // Abrimos el PDF automáticamente
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(dialog.FileName)
                    { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al generar el informe: {ex.Message}";
            }
        }
    }
}
