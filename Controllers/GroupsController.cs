using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using UniversityWebApp.Models;
namespace UniversityWebApp.Controllers
{
    public class GroupsController : Controller
    {
        private readonly string _connectionString;
        public GroupsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            var groups = new List<Groups>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT g.ID, g.GroupName, t.FullName, c.NameCourse FROM Groups g " +
                                    "JOIN Teachers t ON g.CuratorId = t.ID " +
                                    "JOIN Courses c ON g.CourseId = c.ID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groups.Add(new Groups
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CuratorName = reader.GetString(2),
                                CourseName = reader.GetString(3)
                            });
                        }
                    }
                }
                return View(groups);
            }
            catch(SqlException ex)
            {
                TempData["Error"] = ex.Message;
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
                    string query = "DELETE FROM Groups WHERE ID=@Id";
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
        public IActionResult Insert()
        {
            ViewBag.Teachers = GetTeachers();
            ViewBag.Courses = GetCourses();
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Groups groups)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Groups(GroupName,CuratorId,CourseId) " +
                        "VALUES(@GroupName,@CuratorId,@CourseId)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupName", groups.Name);
                        command.Parameters.AddWithValue("@CuratorId", groups.CuratorId);
                        command.Parameters.AddWithValue("@CourseId", groups.CourseId);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();

                return View(groups);
            }
        }
        public IActionResult Edit(int id)
        {
            Groups groups = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID,GroupName, CuratorId,CourseId FROM Groups " +
                        "WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                groups = new Groups
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    CuratorId = reader.GetInt32(2),
                                    CourseId = reader.GetInt32(3)
                                };
                            }
                        }
                    }

                }
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();

                return View(groups);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();

                return View(groups);
            }
        }
        [HttpPost]
        public IActionResult Edit(Groups groups)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "UPDATE Groups SET GroupName=@GroupName,CuratorId=@CuratorId, CourseId=@CourseId " +
                        "WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id",groups.Id);
                        command.Parameters.AddWithValue("@GroupName", groups.Name);
                        command.Parameters.AddWithValue("@CuratorId", groups.CuratorId);
                        command.Parameters.AddWithValue("@CourseId", groups.CourseId);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Teachers = GetTeachers();
                ViewBag.Courses = GetCourses();

                return View(groups);
            }
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
                string query = "SELECT ID, NameCourse FROM Courses ";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var seen = new HashSet<string>(); // чтобы не повторять NameCourse
                    while (reader.Read())
                    {
                        string name = reader.GetString(1);
                        if (!seen.Contains(name))
                        {
                            list.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(0).ToString(), // ID
                                Text = name                           // название курса
                            });
                            seen.Add(name);
                        }
                    }
                }
            }
            return list;
        }

    }
}
