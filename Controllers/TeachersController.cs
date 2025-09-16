using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using UniversityWebApp.Models;

namespace UniversityWebApp.Controllers
{
    public class TeachersController : Controller
    {
        private readonly string _connectionString;
        public TeachersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            var teachers = new List<Teachers>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,FullName,PhoneNumber,BirthDay,Address" +
                    " FROM Teachers";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teachers.Add(new Teachers
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            PhoneNumber = reader.GetString(2),
                            BirthDay = reader.GetDateTime(3),
                            Address = reader.GetString(4),

                        });
                    }
                }
            }
            return View(teachers);
        }
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string delUsers = "DELETE FROM Users WHERE TeacherId=@Id";
                using (SqlCommand command = new SqlCommand(delUsers, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
                string query = "DELETE FROM Teachers WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }

            }
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            Teachers teacher = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID,FullName,PhoneNumber,BirthDay,Address" +
                    " FROM Teachers WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            teacher = new Teachers
                            {
                                Id = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                PhoneNumber = reader.GetString(2),
                                BirthDay = reader.GetDateTime(3),
                                Address = reader.GetString(4),
                            };
                        }
                    }
                }
            }
            return View(teacher);
        }
        [HttpPost]
        public IActionResult Edit(Teachers teachers)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE Teachers SET FullName=@FullName,PhoneNumber=@ProneNumber, " +
                    "BirthDay=@BirthDay,Address=@Address" +
                    " WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", teachers.Id);
                    command.Parameters.AddWithValue("@FullName", teachers.FullName);
                    command.Parameters.AddWithValue("@ProneNumber", teachers.PhoneNumber);
                    command.Parameters.AddWithValue("@BirthDay", teachers.BirthDay);
                    command.Parameters.AddWithValue("@Address", teachers.Address);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
