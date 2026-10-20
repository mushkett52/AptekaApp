using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_OrderSostav")]
    public class KursOrderSostav
    {
        [Key]
        [Column("id_sostav")]
        public int IdSostav { get; set; }

        [Column("id_order")]
        public int IdOrder { get; set; }

        [Column("id_medicine")]
        public int IdMedicine { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [ForeignKey("IdOrder")]
        public virtual KursOrder Order { get; set; } = null!;

        [ForeignKey("IdMedicine")]
        public virtual KursMedicine Medicine { get; set; } = null!;
    }
}