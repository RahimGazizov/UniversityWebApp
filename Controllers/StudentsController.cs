using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using UniversityWebApp.Models;

namespace UniversityWebApp.Controllers
{
    public class StudentsController : Controller
    {
        private readonly string _connectionString;
        public StudentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index(int? courseId, int? groupId)
        {

            var students = new List<Students>();
            ViewBag.Courses = GetCourses();
            ViewBag.Groups = GetGroups();
            try
            {
                string query="";
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    if (courseId == null && groupId == null)
                    {
                        query = "SELECT s.ID,s.FullName,s.PhoneNumber,s.BirthDay,s.Address, " +
                                "c.NameCourse, g.GroupName " +
                                "FROM Students s " +
                                "LEFT JOIN Courses c ON s.CourseId = c.ID " +
                                "LEFT JOIN Groups g ON s.GroupId = g.ID";
                    }
                    else if(courseId != null && groupId == null)
                    {
                        query = "SELECT s.ID,s.FullName,s.PhoneNumber,s.BirthDay,s.Address, " +
                           "c.NameCourse, g.GroupName" +
                           " FROM Students s " +
                           "LEFT JOIN Courses c ON s.CourseId = c.ID " +
                           "LEFT JOIN Groups g ON s.GroupId = g.ID" +
                           " WHERE s.CourseId=@CourseId";
                    }   
                    else
                    {
                        query = "SELECT s.ID,s.FullName,s.PhoneNumber,s.BirthDay,s.Address, " +
                           "c.NameCourse, g.GroupName" +
                           " FROM Students s " +
                           "LEFT JOIN Courses c ON s.CourseId = c.ID " +
                           "LEFT JOIN Groups g ON s.GroupId = g.ID" +
                           " WHERE s.CourseId=@CourseId AND s.GroupId=@GroupId";
                    }
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (courseId != null && groupId == null)
                        {
                            command.Parameters.AddWithValue("@CourseId", courseId);
                        }
                        else if (groupId != null && courseId != null)
                        {
                            command.Parameters.AddWithValue("@CourseId", courseId);
                            command.Parameters.AddWithValue("@GroupId", groupId);

                        }
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new Students
                                {
                                    Id = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    PhoneNumber = reader.GetString(2),
                                    BirthDay = reader.GetDateTime(3),
                                    Address = reader.GetString(4),
                                    CoursesName = reader.GetValue(5)?.ToString() ?? "Не назначено",
                                    GroupName = reader.GetValue(6)?.ToString() ?? "Не назначено"
                                });
                            }
                        }
                    }     
                }
                ViewBag.CourseId = courseId;
                ViewBag.GroupId = groupId;
                return View(students);
            }
            catch(SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                return View();
            }

        }
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string delUsers = "DELETE FROM Users WHERE StudentsId=@Id";
                    using (SqlCommand command = new SqlCommand(delUsers, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                    string query = "DELETE FROM Students WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }

                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        public IActionResult Edit(int id)
        {
            Students student = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID,FullName,PhoneNumber,BirthDay,Address,CourseId,GroupId" +
                        " FROM Students WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                student = new Students
                                {
                                    Id = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    PhoneNumber = reader.GetString(2),
                                    BirthDay = reader.GetDateTime(3),
                                    Address = reader.GetString(4),
                                    CoursesId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                    GroupId = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6)
                                };
                            }
                        }
                    }
                }
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                return View(student);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                return View();
            }
        }
        [HttpPost]
        public IActionResult Edit(Students student)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Students SET FullName=@FullName,PhoneNumber=@ProneNumber, " +
                        "BirthDay=@BirthDay, Address=@Address,CourseId=@CourseId, GroupId=@GroupId" +
                        " WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", student.Id);
                        command.Parameters.AddWithValue("@FullName", student.FullName);
                        command.Parameters.AddWithValue("@ProneNumber", student.PhoneNumber);
                        command.Parameters.AddWithValue("@BirthDay", student.BirthDay);
                        command.Parameters.AddWithValue("@Address", student.Address);
                        command.Parameters.AddWithValue("@CourseId", student.CoursesId);
                        command.Parameters.AddWithValue("@GroupId", student.GroupId);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Courses = GetCourses();
                ViewBag.Groups = GetGroups();
                return View(student);
            }
        }
        private List<SelectListItem> GetCourses()
        {
            var list = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,NameCourse FROM Courses";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var seen = new HashSet<string>();
                    while (reader.Read())
                    {
                        string name = reader.GetString(1);
                        if (!seen.Contains(name))
                        {
                            list.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(0).ToString(),
                                Text = reader.GetString(1)
                            });
                            seen.Add(name);
                        }

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
    }
}
