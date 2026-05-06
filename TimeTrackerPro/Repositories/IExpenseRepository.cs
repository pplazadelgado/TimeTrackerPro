using TimeTrackerPro.Models; 

namespace TimeTrackerPro.Repositories
{
    public interface IExpenseRepository 
    {
        Task<IEnumerable<Expense>> GetByProyectAsync(int projectId);
        Task<int> InsertAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync (int id);
    }
}
