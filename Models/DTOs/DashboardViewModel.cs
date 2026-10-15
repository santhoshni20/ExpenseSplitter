namespace ExpenseSplitter.Models.DTOs
{
    public class DashboardViewModel
    {
        public List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
        public List<ExpenseItemDto> Expenses { get; set; } = new List<ExpenseItemDto>();
        public List<ParticipantBalanceDto> Balances { get; set; } = new List<ParticipantBalanceDto>();
        public List<SettlementTransactionDto> Settlements { get; set; } = new List<SettlementTransactionDto>();
        public decimal TotalExpenses { get; set; }

        public ParticipantDto NewParticipant { get; set; } = new ParticipantDto();
        public CreateExpenseDto NewExpense { get; set; } = new CreateExpenseDto();
    }
}
