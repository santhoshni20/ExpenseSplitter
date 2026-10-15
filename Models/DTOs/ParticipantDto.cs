using System.ComponentModel.DataAnnotations;

namespace ExpenseSplitter.Models.DTOs
{
    public class ParticipantDTO
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Participant name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string name { get; set; } = string.Empty;
    }
}
