using ExpenseSplitter.Interfaces;
using ExpenseSplitter.Models.DTOs;
using ExpenseSplitter.Models.Entities;

namespace ExpenseSplitter.Repositories
{
    public class SettlementService : ISettlementService
    {
        public List<ParticipantBalanceDto> CalculateBalances(IEnumerable<Participant> participants, IEnumerable<Expense> expenses)
        {
            var balances = new List<ParticipantBalanceDto>();
            var participantList = participants.ToList();
            var expenseList = expenses.ToList();

            foreach (var p in participantList)
            {
                decimal totalPaid = expenseList
                    .Where(e => e.PayerId == p.Id)
                    .Sum(e => e.Amount);

                decimal totalOwed = expenseList
                    .SelectMany(e => e.ExpenseShares)
                    .Where(s => s.ParticipantId == p.Id)
                    .Sum(s => s.ShareAmount);

                decimal netBalance = totalPaid - totalOwed;

                balances.Add(new ParticipantBalanceDto
                {
                    ParticipantId = p.Id,
                    ParticipantName = p.Name,
                    TotalPaid = totalPaid,
                    TotalOwed = totalOwed,
                    NetBalance = netBalance
                });
            }

            return balances.OrderBy(b => b.ParticipantName).ToList();
        }

        public List<SettlementTransactionDto> CalculateMinSettlements(List<ParticipantBalanceDto> balances)
        {
            var transactions = new List<SettlementTransactionDto>();

            // Creditors: Positive net balance (people who paid more than their share and should receive money)
            // Debtors: Negative net balance (people who owe money)
            var creditors = balances
                .Where(b => b.NetBalance > 0.005m)
                .Select(b => new PersonBalance { Name = b.ParticipantName, Amount = b.NetBalance })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var debtors = balances
                .Where(b => b.NetBalance < -0.005m)
                .Select(b => new PersonBalance { Name = b.ParticipantName, Amount = -b.NetBalance })
                .OrderByDescending(d => d.Amount)
                .ToList();

            int debtorIndex = 0;
            int creditorIndex = 0;

            while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
            {
                var debtor = debtors[debtorIndex];
                var creditor = creditors[creditorIndex];

                decimal settleAmount = Math.Min(debtor.Amount, creditor.Amount);
                settleAmount = Math.Round(settleAmount, 2);

                if (settleAmount > 0)
                {
                    transactions.Add(new SettlementTransactionDto
                    {
                        FromParticipantName = debtor.Name,
                        ToParticipantName = creditor.Name,
                        Amount = settleAmount
                    });
                }

                debtor.Amount -= settleAmount;
                creditor.Amount -= settleAmount;

                if (debtor.Amount <= 0.005m)
                {
                    debtorIndex++;
                }

                if (creditor.Amount <= 0.005m)
                {
                    creditorIndex++;
                }
            }

            return transactions;
        }

        private class PersonBalance
        {
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }
    }
}
