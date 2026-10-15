using ExpenseSplitter.Interfaces;
using ExpenseSplitter.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Repositories
{
    public class ParticipantRepository : IParticipantRepository
    {
        private readonly ExpenseSplitterDbContext _context;

        public ParticipantRepository(ExpenseSplitterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Participant>> GetAllAsync()
        {
            return await _context.Participants
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Participant?> GetByIdAsync(int id)
        {
            return await _context.Participants.FindAsync(id);
        }

        public async Task AddAsync(Participant participant)
        {
            participant.Name = participant.Name.Trim();
            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            string trimmed = name.Trim().ToLower();
            return await _context.Participants
                .AnyAsync(p => p.Name.ToLower() == trimmed);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var participant = await _context.Participants
                .Include(p => p.PaidExpenses)
                .Include(p => p.ExpenseShares)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participant == null) return false;

            // Only allow delete if participant has no expenses or shares
            if (participant.PaidExpenses.Any() || participant.ExpenseShares.Any())
            {
                return false;
            }

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
