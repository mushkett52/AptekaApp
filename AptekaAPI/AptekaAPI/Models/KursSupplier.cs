using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Supplier")]
    public class KursSupplier
    {
        [Key]
        [Column("id_supplier")]
        public int IdSupplier { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("Telephone")]
        public string Telephone { get; set; } = null!;

        [Column("ContactPerson")]
        public string? ContactPerson { get; set; }

        [Column("email")]
        public string? Email { get; set; }
    }
}