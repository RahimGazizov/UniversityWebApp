namespace UniversityWebApp.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int? StudentId { get; set; }
        public string StudentName { get; set; }
        public int? TeaccherId { get; set; }
        public string TeaccherName { get; set; }
    }
}
