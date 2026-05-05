using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using TimeTrackerPro.Infrastructure;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    /// <summary>
    /// Implementación concreta del repositorio de proyectos usando SQLite + Dapper.
    ///
    /// CONCEPTO DAPPER: En lugar de leer columna a columna, le pasamos a Dapper
    /// el SQL y él mapea automáticamente cada columna al campo de la clase
    /// que tenga el mismo nombre. Simple y transparente.
    /// </summary>
    class ProjectRepository : IProjectRepository
    {
        private readonly DatabaseContext _db;

        // El DatabaseContext nos lo pasan desde fuera(inyeccion de dependencias manual)
        public ProjectRepository(DatabaseContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Obtiene todos los proyectos. No carga secciones ni gastos para
        /// no sobrecargar la lista principal (los cargamos solo cuando se abre un proyecto).
        /// </summary>
        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var projects = await connection.QueryAsync<Project>(
                "SELECT * FROM Projects ORDER BY CreatedAt DESC");

            return projects;
        }

        /// <summary>
        /// Obtiene un proyecto completo con todas sus secciones y gastos.
        /// Hacemos tres consultas separadas y las ensamblamos en memoria.
        /// Esto es más sencillo y legible que un JOIN complejo.
        /// </summary>
        public async Task<Project> GetByIdAsync(int id)
        {
            using var connection = _db.GetConnection();

            // 1. Cargamos el proyecto base
            var project = await connection.QueryFirstOrDefaultAsync<Project>(
                "SELECT * FROM Projects WHERE Id = @Id", new { Id = id });

            if (project == null) return null;

            // 2. Cargamos todas las secciones del proyecto
            var sections = await connection.QueryAsync<Section>(
                "SELECT * FROM Sections WHERE ProjectId = @ProjectId ORDER BY [Order]",
                new { ProjectId = id });

            var sectionList = sections.ToList();

            // 3. Cargamos las sesions de trabajo de todas las secciones
            var sectionsIds = sectionList.Select(s => s.Id).ToList();

            if(sectionsIds.Any())
            {
                var sessions = await connection.QueryAsync<WorkSession>(
                    "SELECT * FROM WorkSessions WHERE SectionId IN @Ids ORDER BY StartTime",
                    new {Ids = sectionsIds });

                //Asignamos cada sesion a su seccion correspondiente
                foreach(var section in sectionList)
                {
                    section.WorkSessions = sessions
                        .Where(ws => ws.SectionId == section.Id)
                        .ToList();
                }
            }

            // 4. Emsamblamos la jerarquia: secciones raiz con sus subsecciones
            var rootSections = sectionList.Where(s => s.ParentSectionId == null).ToList();
            foreach(var root in rootSections)
            {
                root.SubSections = sectionList
                    .Where(s => s.ParentSectionId == root.Id)
                    .ToList();
            }
            project.Sections = rootSections;

            // 5. Cargamos los gastos 
            var expenses = await connection.QueryAsync<Expense>(
                "SELECT * FROM Expenses WHERE ProjectId = @ProjectId ORDER BY Date DESC",
                new { ProjectId = id });

            project.Expenses = expenses.ToList();

            return project;
        }

        /// <summary>
        /// Inserta un proyecto nuevo en la BD.
        /// Devolvemos el Id generado para que el ViewModel pueda usarlo.
        /// </summary>
        public async Task<int> InsertAsync(Project project)
        {
            using var connection = _db.GetConnection();

            //last_insert_rowid() es la funcion SQLite para obtener elide generado
            var sql = @"
                INSERT INTO Projects (Name, Description, WeeklyHours, CreatedAt, Status)
                VALUES (@Name, @Description,@WeeklyHours,@CreatedAt,@Status);
                SELECT last_insert_rowid();";

            var newId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                project.Name,
                project.Description,
                project.WeeklyHours,
                CreatedAt = project.CreatedAt.ToString("o"),
                Status = project.Status.ToString()
            });     
            
            project.Id = newId;
            return newId;
        }

        /// <summary>
        /// Actualiza los datos editables de un proyecto existente.
        /// </summary>
        public async Task UpdateAsync(Project project)
        {
            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(@"
            UPDATE Projects
            SET Name = @Name,
                Description = @Description,
                WeeklyHours = @WeeklyHours,
                Status = @Status
            WHERE Id = @Id", new
            {
                project.Name,
                project.Description,
                project.WeeklyHours,
                Status = project.Status.ToString(),
                project.Id
            });  
        }

        /// <summary>
        /// Elimina un proyecto. Gracias al ON DELETE CASCADE en la BD,
        /// SQLite elimina automáticamente secciones, sesiones y gastos asociados.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();

            //Activamos foreing keys en esta conexion para que CASCADE funcione
            await connection.ExecuteAsync("PRAGMA foreing_keys = ON;");
            await connection.ExecuteAsync(
                "DELETE FROM Projects WHERE Id = @Id", new { Id = id });
        }
    }
}
