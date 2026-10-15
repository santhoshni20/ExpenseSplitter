using System.ComponentModel.DataAnnotations;

namespace ExpenseSplitter.Models.DTOs
{
    public class CreateExpenseDTO
    {
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 10000000.00, ErrorMessage = "Amount must be greater than 0.")]
        public decimal amount { get; set; }

        [Required(ErrorMessage = "Please select who paid.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid person who paid.")]
        public int payerId { get; set; }

        [Required(ErrorMessage = "Please select at least one participant sharing this expense.")]
        [MinLength(1, ErrorMessage = "Please select at least one participant sharing this expense.")]
        public List<int> participantIds { get; set; } = new List<int>();
    }
}
