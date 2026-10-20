using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Role")]
    public class KursRole
    {
        [Key]
        [Column("id_role")]
        public int IdRole { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;
    }
}