namespace UniversityWebApp.Models
{
    public class Groups
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CuratorId { get; set; }
        public string CuratorName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
    }
}
