namespace UniversityWebApp.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string Subject { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int CoursesId { get; set; }
        public string CourseName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int Semester {  get; set; }
        public int DayOfWeek { get; set; }
        public string DayOfWeekName
        {
            get
            {
                return DayOfWeek switch
                {
                    1 => "Понедельник",
                    2 => "Вторник",
                    3 => "Среда",
                    4 => "Четверг",
                    5 => "Пятница",
                    6 => "Суббота",
                    7 => "Воскресенье",
                    _ => "Неизвестно"
                };
            }
        }


        public TimeOnly StartTime {  get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
