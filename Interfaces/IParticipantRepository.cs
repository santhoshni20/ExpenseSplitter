using ExpenseSplitter.Models.Entities;

namespace ExpenseSplitter.Interfaces
{
    public interface IParticipantRepository
    {
        Task<IEnumerable<Participant>> GetAllAsync();
        Task<Participant?> GetByIdAsync(int id);
        Task AddAsync(Participant participant);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> DeleteAsync(int id);
    }
}
