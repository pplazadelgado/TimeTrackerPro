using Dapper;
using TimeTrackerPro.Infrastructure;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    public class WorkSessionRepository :IWorkSessionRepository
    {
        private readonly DatabaseContext _db;

        public WorkSessionRepository(DatabaseContext db) => _db = db;

        public async Task<int> InsertAsync(WorkSession session)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                INSERT INTO WorkSessions (SectionId, StartTime, EndTime, Notes, IsManual)
                VALUES (@SectionId, @StartTime, @EndTime, @Notes, @IsManual);
                SELECT last_insert_rowid();";

            var newId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                session.SectionId,
                StartTime = session.StartTime.ToString("o"),
                EndTime = session.EndTime?.ToString("o"),
                session.Notes,
                IsManual = session.IsManual ? 1 : 0
            });

            session.Id = newId;
            return newId;
        }

        public async Task UpdateAsync(WorkSession session)
        {
            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(@"
                UPDATE WorkSessions
                SET EndTime  = @EndTime,
                    Notes    = @Notes
                WHERE Id = @Id", new
            {
                EndTime = session.EndTime?.ToString("o"),
                session.Notes,
                session.Id
            });
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();
            await connection.ExecuteAsync(
                "DELETE FROM WorkSessions WHERE Id = @Id", new { Id = id });
        }

        public async Task<WorkSession?> GetActiveSessionAsync()
        {
            using var connection = _db.GetConnection();

            // Una sesion activa es la que no tiene EndTime
            return await connection.QueryFirstOrDefaultAsync<WorkSession>(
                "SELECT* FROM Worksessions Where EndTime IS NULL LIMIT 1");
        }
    }
}
