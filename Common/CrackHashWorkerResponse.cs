using System.Xml.Serialization;

namespace Common {
    [XmlRoot("CrackHashWorkerResponse")]
    public class CrackHashWorkerResponse {
        public Guid RequestId { get; set; }
        public int PartNumber { get; set; }

        [XmlArray("Answers")]
        [XmlArrayItem("words")]
        public List<string> Answers { get; set; } = new();
    }
}
