namespace UniversityWebApp.Models
{
    public class Students
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDay { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int? CoursesId { get; set; }
        public string? CoursesName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
    }
}
