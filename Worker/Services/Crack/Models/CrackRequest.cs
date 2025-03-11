namespace Worker.Services.Crack.Models {
    public class CrackRequest {
        public int PartNumber { get; set; }
        public int PartCount { get; set; }
        public List<string> Alphabet { get; set; } = null!;
        public int MaxLength { get; set; }
        public string Hash { get; set; } = null!;
    }
}
