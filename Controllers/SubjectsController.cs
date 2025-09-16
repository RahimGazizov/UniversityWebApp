using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using UniversityWebApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UniversityWebApp.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly string _connectionString;
        public SubjectsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            var subjects = new List<Subjects>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT ID, Name FROM Subjects";
                       
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                       
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subjects.Add(new Subjects
                                {
                                    Id = reader.GetInt32(0),
                                    SubjectName = reader.GetString(1),
                                    
                                });
                            }
                        }
                    }
                  
                }
                return View(subjects);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }
        public IActionResult Insert()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Subjects subject)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Subjects(Name) VALUES(@Name)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", subject.SubjectName);
                     
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View(subject);
            }
        }
        public IActionResult Edit(int id)
        {
            Subjects subjects = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID,Name FROM Subjects " +
                        "WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                subjects = new Subjects
                                {
                                    Id = reader.GetInt32(0),
                                    SubjectName = reader.GetString(1)
                                };
                            }
                        }
                    }
                }

               
                return View(subjects);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult Edit(Subjects subject)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Subjects SET Name=@Name " +
                        "WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", subject.Id);
                        command.Parameters.AddWithValue("@Name", subject.SubjectName);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
               
                return View(subject);
            }
        }
    }
}
