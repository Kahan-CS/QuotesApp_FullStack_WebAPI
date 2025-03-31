using System.Text.Json.Serialization;



namespace web_api.Models

{
    public class TagAssignment
    {
        // Composite key: QuoteId and TagId (configured in the DbContext)
        public int QuoteId { get; set; }

        [JsonIgnore]
        public Quote Quote { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
