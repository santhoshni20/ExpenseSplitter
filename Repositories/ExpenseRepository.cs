using ExpenseSplitter.Interfaces;
using ExpenseSplitter.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ExpenseSplitterDbContext _context;

        public ExpenseRepository(ExpenseSplitterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Expense>> GetAllWithDetailsAsync()
        {
            return await _context.Expenses
                .Include(e => e.Payer)
                .Include(e => e.ExpenseShares)
                    .ThenInclude(s => s.Participant)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<Expense?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.Payer)
                .Include(e => e.ExpenseShares)
                    .ThenInclude(s => s.Participant)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Expense expense, IEnumerable<int> participantIds)
        {
            var distinctIds = participantIds.Distinct().ToList();
            if (distinctIds.Count == 0)
            {
                throw new ArgumentException("Expense must have at least one participant sharing the cost.");
            }

            expense.Description = expense.Description.Trim();
            expense.Date = DateTime.Now;

            // Equal split calculation
            decimal totalAmount = expense.Amount;
            decimal sharePerPerson = Math.Round(totalAmount / distinctIds.Count, 2, MidpointRounding.AwayFromZero);

            // Handle rounding differences so sum(shares) == totalAmount exactly
            decimal remainder = totalAmount - (sharePerPerson * distinctIds.Count);

            expense.ExpenseShares = new List<ExpenseShare>();
            for (int i = 0; i < distinctIds.Count; i++)
            {
                decimal individualShare = sharePerPerson;
                if (i == 0)
                {
                    individualShare += remainder; // assign any leftover fraction of a cent to first person
                }

                expense.ExpenseShares.Add(new ExpenseShare
                {
                    ParticipantId = distinctIds[i],
                    ShareAmount = individualShare
                });
            }

            await _context.Expenses.AddAsync(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.ExpenseShares)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null) return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
