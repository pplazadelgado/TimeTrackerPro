// Repositories/ISectionRepository.cs
using TimeTrackerPro.Models;

namespace TimeTrackerPro.Repositories
{
    public interface ISectionRepository
    {
        Task<int> InsertAsync(Section section);
        Task UpdateAsync(Section section);
        Task DeleteAsync(int id);
    }
}