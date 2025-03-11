using System.ComponentModel.DataAnnotations;

namespace Manager.Models {
    public class CrackRequest {
        [Required]
        public string Hash { get; set; } = null!;

        [Required]
        public int MaxLength { get; set; }
    }
}
