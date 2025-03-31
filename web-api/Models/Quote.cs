using System.ComponentModel.DataAnnotations;

namespace web_api.Models
{
    public class Quote
    {
        [Key]
        public int QuoteId { get; set; }

        [Required]
        public string Content { get; set; }

        // Author is optional
        public string Author { get; set; }

        // To store the number of likes
        public int Likes { get; set; } = 0;

        // Navigation property for many-to-many relation with Tag via TagAssignment
        public ICollection<TagAssignment> TagAssignments { get; set; } = new List<TagAssignment>();
    }
}
