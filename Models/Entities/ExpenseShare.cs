namespace ExpenseSplitter.Models.Entities
{
    public class ExpenseShare
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }
        public Expense? Expense { get; set; }

        public int ParticipantId { get; set; }
        public Participant? Participant { get; set; }

        public decimal ShareAmount { get; set; }
    }
}
