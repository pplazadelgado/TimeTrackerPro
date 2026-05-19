using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    /// <summary>
    /// Operaciones de base de datos para sesiones de trabajo.
    /// </summary>
    public interface IWorkSessionRepository
    {
        /// <summary>Inserta una sesión nueva y devuelve el Id generado.</summary>
        Task<int> InsertAsync(WorkSession session);

        /// <summary>
        /// Actualiza una sesión existente. Se usa principalmente
        /// para guardar el EndTime al parar el cronómetro.
        /// </summary>
        Task UpdateAsync(WorkSession session);

        /// <summary>Elimina una sesión por su Id.</summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Busca si hay alguna sesión activa (sin EndTime) en toda la BD.
        /// Garantiza que no haya dos cronómetros corriendo a la vez.
        /// </summary>
        Task<WorkSession?> GetActiveSessionAsync();
    }
}
