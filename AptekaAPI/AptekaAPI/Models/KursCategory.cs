using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Category")]
    public class KursCategory
    {
        [Key]
        [Column("id_category")]
        public int IdCategory { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }
    }
}