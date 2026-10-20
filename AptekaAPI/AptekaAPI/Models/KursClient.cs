using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Client")]
    public class KursClient
    {
        [Key]
        [Column("id_client")]
        public int IdClient { get; set; }

        [Column("id_user")]
        public int IdUser { get; set; }

        [Column("LastName")]
        public string LastName { get; set; } = null!;

        [Column("FirstName")]
        public string FirstName { get; set; } = null!;

        [Column("Patronymic")]
        public string? Patronymic { get; set; }

        [Column("Telephone")]
        public string Telephone { get; set; } = null!;

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Adress")]
        public string? Adress { get; set; }

        [ForeignKey("IdUser")]
        public virtual KursUser User { get; set; } = null!;
    }
}