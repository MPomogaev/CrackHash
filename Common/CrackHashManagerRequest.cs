using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Xml.Serialization;

namespace Common {
    [XmlRoot("CrackHashManagerRequest")]
    public class CrackHashManagerRequest {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid RequestId { get; set; }
        public int PartNumber { get; set; }
        public int PartCount { get; set; }
        public string Hash { get; set; }
        public int MaxLength { get; set; }

        [XmlArray("Alphabet")]
        [XmlArrayItem("symbols")]
        public List<string> Alphabet { get; set; } = new List<string>();
    }
}
