using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using UniversityWebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
namespace UniversityWebApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly string _connectionString;
        public RegistrationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            ViewBag.Roles = GetRoles();
            return View();
        }
        [HttpPost]
        public IActionResult Index(Registration registration)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ошибка не верный формат почты";
                ViewBag.Roles = GetRoles();
                return View(registration);
            }
            if (registration.Password != registration.ConfirmPassword)
            {
                TempData["Error"] = "Пароли не совпадают";
                ViewBag.Roles = GetRoles();
                return View(registration);
            }
            try
            {
                int personId;
                if (registration.RoleId == 1)
                    personId = InternPerson("Students", registration.FullName, registration.PhoneNumber, registration.BirthDay, registration.Address, registration.Email);
                else
                    personId = InternPerson("Teachers", registration.FullName, registration.PhoneNumber, registration.BirthDay, registration.Address, registration.Email);
                InsertUsers(registration.FullName, registration.Email, registration.Password = (HachCode(registration.Password)), registration.RoleId,
                    registration.RoleId == 1 ? personId : (int?)null,
                    registration.RoleId == 2 ? personId : (int?)null,
                    registration.RoleId == 3 ? 3 : (int?)null);
                var role = HttpContext.Session.GetInt32("Role");
                if (role == 3 && registration.RoleId == 1)
                    return RedirectToAction("Index", "Students");
                else if (role == 3 && registration.RoleId == 2)
                    return RedirectToAction("Index", "Teachers");
                else
                    return RedirectToAction("Index", "Authorization");
            }
            catch (SqlException)
            {
                TempData["Error"] = "Такая почта уже существует";
                ViewBag.Roles = GetRoles();
                return View(registration);
            }
        }
        private int InternPerson(string tableName, string fullName, string phoneNumber, DateTime birthDay, string address, string email)
        {
            int personId;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = $"INSERT INTO {tableName}(FullName,PhoneNumber,BirthDay,Address,Email)" +
                    $" VALUES(@FullName,@PhoneNumber,@BirthDay,@Address,@Email);" +
                    $" SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@BirthDay", birthDay);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Email", email);
                    personId = (int)command.ExecuteScalar();
                }
            }
            return personId;
        }
        public void InsertUsers(string name, string login, string password, int roleId, int? studentId, int? teacherId, int? adminId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string userQuery = @"INSERT INTO Users(FullName, Email, PasswordHash, RoleId, StudentsId, TeacherId)
                                 VALUES(@FullName, @Email, @PasswordHash, @RoleId, @StudentsId, @TeacherId)";

                using (SqlCommand command = new SqlCommand(userQuery, connection))
                {
                    command.Parameters.AddWithValue("@FullName", name);
                    command.Parameters.AddWithValue("@Email", login);
                    command.Parameters.AddWithValue("@PasswordHash", password);
                    command.Parameters.AddWithValue("@RoleId", roleId);

                    // StudentsId и TeacherId
                    if (studentId.HasValue)
                        command.Parameters.AddWithValue("@StudentsId", studentId.Value);
                    else
                        command.Parameters.AddWithValue("@StudentsId", DBNull.Value);

                    if (teacherId.HasValue)
                        command.Parameters.AddWithValue("@TeacherId", teacherId.Value);
                    else
                        command.Parameters.AddWithValue("@TeacherId", DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }
        private List<SelectListItem> GetRoles()
        {
            List<SelectListItem> roles = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT ID, NameRole FROM Roles";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return roles;
        }
        private string HachCode(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hach = pbkdf2.GetBytes(32);
            byte[] hachCode = new byte[48];
            Array.Copy(salt, 0, hachCode, 0, 16);
            Array.Copy(hach, 0, hachCode, 16, 32);
            return Convert.ToBase64String(hachCode);
        }
    }
}
