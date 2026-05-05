using System.Windows.Threading;
using TimeTrackerPro.Models;
using TimeTrackerPro.Repositories;

namespace TimeTrackerPro.Services
{
    /// <summary>
    /// Gestiona el cronómetro de la aplicación.
    ///
    /// CONCEPTO DispatcherTimer: Es un temporizador especial de WPF que
    /// ejecuta su evento en el hilo de la UI. Esto es importante porque
    /// solo el hilo de la UI puede actualizar controles visuales.
    /// Un Timer normal lanzaría una excepción al intentar actualizar la pantalla.
    /// </summary>
    public class TimerService
    {
        private readonly IWorkSessionRepository _repository;
        private readonly DispatcherTimer _timer;
        private WorkSession? _activeSession;

        /// <summary>Se dispara cada segundo con el tiempo transcurrido actualizado.</summary>
        public event Action<TimeSpan>? TimerTick;

        /// <summary>Se dispara cuando el cronómetro se inicia o se detiene.</summary>
        public event Action<bool>? IsRunningChanged;

        public bool IsRunning => _activeSession != null && _activeSession.IsActive;

        /// <summary>Sección que tiene el cronómetro activo, null si no hay ninguna.</summary>
        public int? ActiveSectionId => _activeSession?.SectionId;

        public TimerService(IWorkSessionRepository repository)
        {
            _repository = repository;

            //Configuramos el timer para que se dispare cada segundo
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += OnTimerTick;
        }

        /// <summary>
        /// Inicia el cronómetro para una sección.
        /// Si hay otro cronómetro activo, lo para primero.
        /// </summary>
        public async Task StartAsync(int sectionId)
        {
            //Primero comprobamos si haya una sesion activa en BD
            var existing = await _repository.GetActiveSessionAsync();
            if (existing != null)
                await StopAsync();

            //Creas la sesion en BD con EndTime = null
            _activeSession = new WorkSession
            {
                SectionId = sectionId,
                StartTime = DateTime.Now,
                EndTime = null,
                IsManual = false
            };

            await _repository.InsertAsync(_activeSession);

            _timer.Start();
            IsRunningChanged?.Invoke(true);
        }

        /// <summary>
        /// Para el cronómetro y guarda el EndTime en la BD.
        /// Devuelve la sesión completada para que el ViewModel la añada a la lista.
        /// </summary>
        public async Task<WorkSession> StopAsync()
        {
            if (_activeSession == null) return null;

            _timer.Stop();
            _activeSession.EndTime = DateTime.Now;

            await _repository.UpdateAsync(_activeSession);

            var completedSession = _activeSession;
            _activeSession = null;

            IsRunningChanged?.Invoke(false);
            return completedSession;
        }

        /// <summary>
        /// Recupera una sesión activa que quedó abierta si la app
        /// se cerró sin parar el cronómetro.
        /// </summary>
        public async Task RestoreActiveSessionAsync()
        {
            var active = await _repository.GetActiveSessionAsync();
            if (active == null) return;

            _activeSession = active;
            _timer.Start();
            IsRunningChanged?.Invoke(true);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_activeSession == null) return;

            var elapsed = DateTime.Now - _activeSession.StartTime;
            TimerTick?.Invoke(elapsed);
        }
    }
}
