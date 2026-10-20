using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekaAPI.Models
{
    [Table("kurs_Users")]
    public class KursUser
    {
        [Key]
        [Column("id_user")]
        public int IdUser { get; set; }

        [Column("login")]
        public string Login { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("id_role")]
        public int IdRole { get; set; }

        [ForeignKey("IdRole")]
        public virtual KursRole Role { get; set; } = null!;
    }
}