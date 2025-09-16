using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using UniversityWebApp.Models;

namespace UniversityWebApp.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly string _connectionString;
        public ScheduleController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IActionResult> Index(int? dayweek)
        {
            int? selectedDay = dayweek;
            bool showAll = false;
            if(HttpContext.Request.Method == "POST")  
            {
                if (string.IsNullOrEmpty(Request.Form["dayweek"]))
                {
                    showAll = true;
                    selectedDay = null;
                }
                
            }
            if(!showAll && selectedDay ==  null)
            {
                var today = DateTime.Now.DayOfWeek;
                selectedDay = today == DayOfWeek.Saturday ? (int?)null : (int)today;
            }

            var role = int.Parse(User.FindFirstValue(ClaimTypes.Role) ?? "0");
            var group = int.Parse(User.FindFirstValue("Group") ?? "0");
            var teacher = int.Parse(User.FindFirstValue("Teacher") ?? "0");
            var schedule = new List<Schedule>();
            string query = "SELECT sc.ID,s.Name, t.FullName, c.NameCourse,c.Semester, sc.DayOfWeek,sc.StartTime,sc.EndTime,g.GroupName FROM Schedule sc " +
                       "JOIN Subjects s ON sc.SubjectId = s.ID " +
                       "JOIN Teachers t ON sc.TeacherId = t.ID " +
                       "JOIN Courses c ON sc.CoursesId = c.ID " +
                       "JOIN Groups g ON sc.GroupId = g.ID WHERE 1=1";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (role == 1)
                    {
                        query += " AND GroupId=@GroupId";
                        command.Parameters.AddWithValue("@GroupId", group);
                    }
                    if(role == 2)
                    {
                        query += " AND TeacherId=@Teacher";
                        command.Parameters.AddWithValue("@Teacher",teacher);
                    }
                    if (!showAll && selectedDay != null)
                    {
                        query += " AND DayOfWeek=@DayOfWeek";
                        command.Parameters.AddWithValue("@DayOfWeek", selectedDay);
                    }
                    command.CommandText = query;
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            schedule.Add(new Schedule
                            {
                                Id = reader.GetInt32(0),
                                Subject = reader.GetString(1),
                                TeacherName = reader.GetString(2),
                                CourseName = reader.GetString(3),
                                Semester = reader.GetInt32(4),
                                DayOfWeek = reader.GetInt32(5),
                                StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(6)),
                                EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(7)),
                                GroupName = reader.GetString(8),
                            });
                        }
                    }
                }

            }
            ViewBag.SelectedDay = selectedDay;
          
            return View(schedule);
        }
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string qeury = "DELETE FROM Schedule WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(qeury, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Insert(int? dayweek)
        {
            ViewBag.Subjects = GetSubjects();
            ViewBag.Teachers = GetTeachers();
            ViewBag.Courses = GetCourses();
            ViewBag.Groups = GetGroups();
            ViewBag.Weeks = GetWeeks();
            ViewBag.Day = dayweek;
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Schedule schedule)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string qury = "INSERT INTO Schedule(SubjectId,TeacherId,CoursesId,DayOfWeek,StartTime,EndTime,GroupId) " +
                        "VALUES(@SubjectId,@TeacherId,@CoursesId,@DayOfWeek,@StartTime,@EndTime,@GroupId)";
                    using (SqlCommand command = new SqlCommand(qury, connection))
                    {
                        command.Parameters.AddWithValue("@SubjectId", schedule.SubjectId);
                        command.Parameters.AddWithValue("@TeacherId", schedule.TeacherId);
                        command.Parameters.AddWithValue("@CoursesId", schedule.CoursesId);
                        command.Parameters.AddWithValue("@DayOfWeek", schedule.DayOfWeek);
                        command.Parameters.AddWithValue("@StartTime", schedule.StartTime);
                        command.Parameters.AddWithValue("@EndTime", schedule.EndTime);
                        command.Parameters.AddWithValue("@GroupId", schedule.GroupId);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index", new {dayweek = schedule.DayOfWeek});
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Subjects = GetSubjects();
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                ViewBag.Weeks = GetWeeks();

                return View(schedule);
            }
        }
        public IActionResult Edit(int id)
        {
            Schedule schedule = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID,SubjectId,TeacherId,CoursesId,DayOfWeek,StartTime,EndTime,GroupId  " +
                        "FROM Schedule WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                schedule = new Schedule
                                {
                                    Id = reader.GetInt32(0),
                                    SubjectId = reader.GetInt32(1),
                                    TeacherId = reader.GetInt32(2),
                                    CoursesId = reader.GetInt32(3),
                                    DayOfWeek = reader.GetInt32(4),
                                    StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(5)),
                                    EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(6)),
                                    GroupId = reader.GetInt32(7),
                                };
                            }
                        }
                    }
                }
                ViewBag.Subjects = GetSubjects();
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                ViewBag.Weeks = GetWeeks();

                return View(schedule);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult Edit(Schedule schedule)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string qury = "UPDATE Schedule SET SubjectId=@SubjectId,TeacherId=@TeacherId, " +
                        "CoursesId=@CoursesId,DayOfWeek=@DayOfWeek,StartTime=@StartTime,EndTime=@EndTime, GroupId=@GroupId " +
                        "WHERE ID=@Id";

                    using (SqlCommand command = new SqlCommand(qury, connection))
                    {
                        command.Parameters.AddWithValue("@Id", schedule.Id);
                        command.Parameters.AddWithValue("@SubjectId", schedule.SubjectId);
                        command.Parameters.AddWithValue("@TeacherId", schedule.TeacherId);
                        command.Parameters.AddWithValue("@CoursesId", schedule.CoursesId);
                        command.Parameters.AddWithValue("@DayOfWeek", schedule.DayOfWeek);
                        command.Parameters.AddWithValue("@StartTime", schedule.StartTime);
                        command.Parameters.AddWithValue("@EndTime", schedule.EndTime);
                        command.Parameters.AddWithValue("@GroupId", schedule.GroupId);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index", new {dayweek = schedule.DayOfWeek});
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Subjects = GetSubjects();
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                ViewBag.Weeks = GetWeeks();

                return View(schedule);
            }
        }
        private List<SelectListItem> GetSubjects()
        {
            var list = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,Name FROM Subjects";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }
        private List<SelectListItem> GetGroups()
        {
            var list = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,GroupName FROM Groups";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }
        private List<SelectListItem> GetTeachers()
        {
            var list = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,FullName FROM Teachers";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        private List<SelectListItem> GetCourses()
        {
            var list = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,NameCourse,Semester FROM Courses";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = $"{reader.GetString(1)} Семестр - {reader.GetInt32(2)}"
                        });
                    }
                }
            }
            return list;
        }
        private List<SelectListItem> GetWeeks()
        {
            var list = new List<SelectListItem>();
            string[] weeks = { "Понидельник", "Вторник", "Среда", "Четверг", "Пятницца", "Суббота", "Воскресенье" };
            for (int i = 1; i <= 7; i++)
            {
                list.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = weeks[i - 1].ToString()
                });
            }
            return list;
        }
    }
}
