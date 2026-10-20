using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Order")]
    public class KursOrder
    {
        [Key]
        [Column("id_order")]
        public int IdOrder { get; set; }

        [Column("id_client")]
        public int? IdClient { get; set; }

        [Column("orderDate")]
        public DateTime OrderDate { get; set; }

        [Column("sum")]
        public decimal Sum { get; set; }

        [Column("id_status")]
        public int IdStatus { get; set; }

        [Column("orderNumber")]
        public string OrderNumber { get; set; } = null!;

        [ForeignKey("IdClient")]
        public virtual KursClient? Client { get; set; }

        [ForeignKey("IdStatus")]
        public virtual KursStatus Status { get; set; } = null!;

        public virtual ICollection<KursOrderSostav> OrderItems { get; set; } = new List<KursOrderSostav>();
    }
}