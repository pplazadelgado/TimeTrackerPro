using Dapper;
using TimeTrackerPro.Infrastructure;
using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    public class ExpenseRepository :IExpenseRepository
    {
        private readonly DatabaseContext _db;

        public ExpenseRepository(DatabaseContext db) => _db = db;

        public async Task<IEnumerable<Expense>> GetByProyectAsync(int projectId)
        {
            using var connection = _db.GetConnection();
            return await connection.QueryAsync<Expense>(
                "SELECT * FROM Expenses WHERE ProjectId = @ProjectId ORDER BY Date DESC",
                new { ProjectId = projectId });
        }

        public async Task<int> InsertAsync(Expense expense)
        {
            using var connetion = _db.GetConnection();

            var sql = @"
                INSERT INTO Expenses (ProjectId, Description, Amount, Category, Date, Reference)
                VALUES (@ProjectId, @Description, @Amount, @Category, @Date, @Reference);
                SELECT last_insert_rowid();";

            var newId = await connetion.ExecuteScalarAsync<int>(sql, new
            {
                expense.ProjectId,
                expense.Description,
                expense.Amount,
                Category = expense.Category.ToString(),
                Date = expense.Date.ToString("o"),
                expense.Reference
            });

            expense.Id = newId;
            return newId;
        }

        public async Task UpdateAsync(Expense expense)
        {
            using var connection = _db.GetConnection();

            await connection.ExecuteAsync(@"
                UPDATE Expenses
                SET Description = @Description,
                    Amount      = @Amount,
                    Category    = @Category,
                    Date        = @Date,
                    Reference   = @Reference
                WHERE Id = @Id", new
            {
                expense.Description,
                expense.Amount,
                Category = expense.Category.ToString(),
                Date = expense.Date.ToString("o"),
                expense.Reference,
                expense.Id
            });
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();
            await connection.ExecuteAsync(
                "DELETE FROM Expenses WHERE Id = @Id", new {Id = id});
        }
    }
}
