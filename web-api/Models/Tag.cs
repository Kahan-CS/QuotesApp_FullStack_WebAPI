using System.ComponentModel.DataAnnotations;

namespace web_api.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        public string Name { get; set; }

        // Navigation property for many-to-many relation with Quote via TagAssignment
        public ICollection<TagAssignment> TagAssignments { get; set; } = new List<TagAssignment>();
    }
}
