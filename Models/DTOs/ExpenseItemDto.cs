namespace ExpenseSplitter.Models.DTOs
{
    public class ExpenseItemDTO
    {
        public int id { get; set; }
        public string description { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public DateTime date { get; set; }
        public int payerId { get; set; }
        public string payerName { get; set; } = string.Empty;
        public List<string> sharedByNames { get; set; } = new List<string>();
        public decimal sharePerPerson { get; set; }
    }
}
