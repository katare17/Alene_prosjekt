using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data
{
    public class GeoChange
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? GeoJson { get; set; }

        [Required]
        public string? Description { get; set; }

        public virtual WebUser? User { get; set; }
    }
}
