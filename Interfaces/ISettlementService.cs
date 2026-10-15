using ExpenseSplitter.Models.DTOs;
using ExpenseSplitter.Models.Entities;

namespace ExpenseSplitter.Interfaces
{
    public interface ISettlementService
    {
        List<ParticipantBalanceDto> CalculateBalances(IEnumerable<Participant> participants, IEnumerable<Expense> expenses);
        List<SettlementTransactionDto> CalculateMinSettlements(List<ParticipantBalanceDto> balances);
    }
}
