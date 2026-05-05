using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    /// <summary>
    /// Contrato que define las operaciones disponibles sobre proyectos.
    /// 
    /// CONCEPTO: Programamos contra la interfaz, no contra la implementación.
    /// El resto de la app usa IProjectRepository, no ProjectRepository directamente.
    /// Esto facilita los tests y el cambio de tecnología de BD en el futuro.
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>Devuelve todos los proyectos (sin cargar secciones ni gastos).</summary>
        Task<IEnumerable<Project>> GetAllAsync();

        /// <summary>Devuelve un proyecto por su Id, incluyendo secciones y gastos.</summary>
        Task<Project?> GetByIdAsync(int id);

        /// <summary>Inserta un proyecto nuevo y devuelve el Id generado por la BD.</summary>
        Task<int> InsertAsync(Project project);

        /// <summary>Actualiza los datos de un proyecto existente.</summary>
        Task UpdateAsync(Project project);

        /// <summary>Elimina un proyecto y todo lo que cuelga de él (cascade).</summary>
        Task DeleteAsync(int id);
    }
}
