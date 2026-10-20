using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Status")]
    public class KursStatus
    {
        [Key]
        [Column("id_status")]
        public int IdStatus { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;
    }
}