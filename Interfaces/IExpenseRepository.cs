using ExpenseSplitter.Models.Entities;

namespace ExpenseSplitter.Interfaces
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetAllWithDetailsAsync();
        Task<Expense?> GetByIdWithDetailsAsync(int id);
        Task AddAsync(Expense expense, IEnumerable<int> participantIds);
        Task<bool> DeleteAsync(int id);
    }
}
