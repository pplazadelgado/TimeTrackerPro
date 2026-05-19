// Repositories/SectionRepository.cs
using Dapper;
using TimeTrackerPro.Infrastructure;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        private readonly DatabaseContext _db;

        public SectionRepository(DatabaseContext db) => _db = db;

        public async Task<int> InsertAsync(Section section)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                INSERT INTO Sections
                    (ProjectId, ParentSectionId, Name, Description,
                     EstimatedHours, [Order], CreatedAt, Status, DeadlineDate)
                VALUES
                    (@ProjectId, @ParentSectionId, @Name, @Description,
                     @EstimatedHours, @Order, @CreatedAt, @Status, @DeadlineDate);
                SELECT last_insert_rowid();";

            var newId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                section.ProjectId,
                section.ParentSectionId,
                section.Name,
                section.Description,
                section.EstimatedHours,
                section.Order,
                CreatedAt = section.CreatedAt.ToString("o"),
                Status = section.Status.ToString(),
                DeadlineDate = section.DeadlineDate?.ToString("o")
            });

            section.Id = newId;
            return newId;
        }

        public async Task UpdateAsync(Section section)
        {
            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(@"
                UPDATE Sections
                SET Name           = @Name,
                    Description    = @Description,
                    EstimatedHours = @EstimatedHours,
                    Status         = @Status,
                    DeadlineDate   = @DeadlineDate
                WHERE Id = @Id", new
            {
                section.Name,
                section.Description,
                section.EstimatedHours,
                Status = section.Status.ToString(),
                DeadlineDate = section.DeadlineDate?.ToString("o"),
                section.Id
            });
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
            await connection.ExecuteAsync(
                "DELETE FROM Sections WHERE Id = @Id", new { Id = id });
        }
    }
}