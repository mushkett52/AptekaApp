using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Medicine")]
    public class KursMedicine
    {
        [Key]
        [Column("id_Medicine")]
        public int IdMedicine { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("manufacturer")]
        public string Manufacturer { get; set; } = null!;

        [Column("price")]
        public decimal Price { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [Column("expDate")]
        public DateTime? ExpDate { get; set; }

        [Column("prescription")]
        public bool Prescription { get; set; }

        [Column("id_supplier")]
        public int IdSupplier { get; set; }

        [Column("id_category")]
        public int IdCategory { get; set; }

        [Column("minAmount")]
        public int? MinAmount { get; set; }

        [ForeignKey("IdCategory")]
        public virtual KursCategory Category { get; set; } = null!;

        [ForeignKey("IdSupplier")]
        public virtual KursSupplier Supplier { get; set; } = null!;
    }
}