namespace ExpenseSplitter.Models.DTOs
{
    public class SettlementTransactionDto
    {
        public string FromParticipantName { get; set; } = string.Empty;
        public string ToParticipantName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Instruction => $"{FromParticipantName} pays {ToParticipantName} {Amount:C2}";
    }
}
