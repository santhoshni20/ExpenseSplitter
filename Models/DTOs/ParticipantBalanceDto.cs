namespace ExpenseSplitter.Models.DTOs
{
    public class ParticipantBalanceDto
    {
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public decimal TotalPaid { get; set; }
        public decimal TotalOwed { get; set; }
        public decimal NetBalance { get; set; }

        public string Status
        {
            get
            {
                if (NetBalance > 0.005m) return "Receives";
                if (NetBalance < -0.005m) return "Owes";
                return "Settled";
            }
        }
    }
}
