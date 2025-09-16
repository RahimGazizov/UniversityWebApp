using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityWebApp.Models
{
    public class Registration
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDay { get; set; }
        public string Address { get; set; }
        [EmailAddress(ErrorMessage = "Введите корректную почту")]
        public string Email { get; set; }
        public string Password { get; set; }
        [NotMapped]
        public string ConfirmPassword { get; set; }
        public int RoleId { get; set; }

    }
}
