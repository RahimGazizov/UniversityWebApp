using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using UniversityWebApp.Models;

namespace UniversityWebApp.Controllers
{
    public class CoursesController : Controller
    {
        private readonly string _connectionString;
        public CoursesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            var courses = new List<Courses>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID, NameCourse,Semester FROM Courses ";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new Courses
                        {
                            Id = reader.GetInt32(0),
                            CoursesName = reader.GetString(1),
                            Semester = reader.GetInt32(2)

                        });
                    }
                }
            }
            return View(courses);
        }
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Courses WHERE ID=@Id";
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
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Courses courses)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Courses(NameCourse,Semester) VALUES(@NameCourse,@Semester)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NameCourse", courses.CoursesName);
                        command.Parameters.AddWithValue("@Semester", courses.Semester);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View(courses);
            }
        }
        public IActionResult Edit(int id)
        {
            Courses courses = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID, NameCourse,Semester FROM Courses WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                courses = new Courses
                                {
                                    Id = reader.GetInt32(0),
                                    CoursesName = reader.GetString(1),
                                    Semester = reader.GetInt32(2)
                                };
                            }
                        }
                    }
                }
                return View(courses);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View(courses);
            }
            catch (NullReferenceException ex)
            {
                TempData["Error"] = ex.Message;
                return View(courses);
            }
        }

        [HttpPost]
        public IActionResult Edit(Courses courses)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Courses SET NameCourse=@NameCourse,Semester=@Semester" +
                        " WHERE ID=@Id";
                    using(SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", courses.Id);
                        command.Parameters.AddWithValue("@NameCourse", courses.CoursesName);
                        command.Parameters.AddWithValue("@Semester", courses.Semester);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View(courses);
            }
            catch (NullReferenceException ex)
            {
                TempData["Error"] = ex.Message;
                return View(courses);
            }
        }
    }
}
