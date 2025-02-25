using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data
{
    public class GeoChange
    {
        [Key]
        public int Id { get; set; }

        // Foreign key for the user
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Required]
        public string? GeoJson { get; set; }

        [Required]
        public string? Description { get; set; }

        public bool? IsApproved { get; set; }


        // Navigation property
        public virtual WebUser? User { get; set; }

        [Required]
        public string Kommunenavn { get; set; }
        [Required]

        public string Kommunenummer { get; set; }
        [Required]

        public string Fylkesnavn { get; set; }
    }
}