// Infrastructure/DatabaseContext.cs
using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;

namespace TimeTrackerPro.Infrastructure
{
    /// <summary>
    /// Clase responsable de crear y gestionar la base de datos SQLite.
    ///
    /// CONCEPTO: Esta clase hace dos cosas:
    ///   1. Proporciona conexiones a la BD (GetConnection)
    ///   2. Crea las tablas si no existen (InitializeAsync)
    ///
    /// El archivo .db se guarda en AppData del usuario para que no haya
    /// problemas de permisos de escritura.
    /// </summary>
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            // Ruta: C:\Users\[usuario]\AppData\Roaming\TimeTrackerPro\
            // Usamos AppData porque siempre tenemos permiso de escritura ahí.
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TimeTrackerPro");

            // Creamos la carpeta si no existe
            Directory.CreateDirectory(appDataPath);

            var dbPath = Path.Combine(appDataPath, "timetracker.db");
            _connectionString = $"Data Source={dbPath}";

            // Guardamos la ruta para poder mostrarla al usuario si la necesita
            DatabasePath = dbPath;
        }

        /// <summary>Ruta completa al archivo .db (útil para exportar).</summary>
        public string DatabasePath { get; }

        /// <summary>
        /// Abre y devuelve una conexión a la base de datos.
        /// Usamos 'using' al llamar a este método para cerrar la conexión
        /// automáticamente cuando terminamos. Las conexiones son recursos
        /// limitados, siempre hay que cerrarlas.
        /// </summary>
        public SqliteConnection GetConnection()
            => new SqliteConnection(_connectionString);

        /// <summary>
        /// Crea todas las tablas si no existen todavía.
        /// Es seguro llamar a esto cada vez que arranca la app:
        /// "CREATE TABLE IF NOT EXISTS" no hace nada si la tabla ya existe.
        /// </summary>
        public async Task InitializeAsync()
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            // Activamos las foreign keys de SQLite (están desactivadas por defecto)
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

            // Creamos las tablas en orden (primero las que no tienen dependencias)
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Projects (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name        TEXT    NOT NULL,
                    Description TEXT    NOT NULL DEFAULT '',
                    WeeklyHours REAL    NOT NULL DEFAULT 8,
                    CreatedAt   TEXT    NOT NULL,
                    Status      TEXT    NOT NULL DEFAULT 'Active'
                );

                CREATE TABLE IF NOT EXISTS Sections (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId       INTEGER NOT NULL,
                    ParentSectionId INTEGER,
                    Name            TEXT    NOT NULL,
                    Description     TEXT    NOT NULL DEFAULT '',
                    EstimatedHours  REAL    NOT NULL DEFAULT 0,
                    [Order]         INTEGER NOT NULL DEFAULT 0,
                    CreatedAt       TEXT    NOT NULL,
                    Status          TEXT    NOT NULL DEFAULT 'Pending',
                    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ParentSectionId) REFERENCES Sections(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS WorkSessions (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    SectionId INTEGER NOT NULL,
                    StartTime TEXT    NOT NULL,
                    EndTime   TEXT,
                    Notes     TEXT    NOT NULL DEFAULT '',
                    IsManual  INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (SectionId) REFERENCES Sections(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Expenses (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId   INTEGER NOT NULL,
                    Description TEXT    NOT NULL,
                    Amount      REAL    NOT NULL DEFAULT 0,
                    Category    TEXT    NOT NULL DEFAULT 'Other',
                    Date        TEXT    NOT NULL,
                    Reference   TEXT    NOT NULL DEFAULT '',
                    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
                );
            ");
        }
    }
}