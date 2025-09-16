using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using UniversityWebApp.Models;
namespace UniversityWebApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly string _connectionString;
        public AuthorizationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(AuthorizationModel authorization)
        {
            string hachPassword = "";
            int role = 0;
            int group = 0;
            int teacher = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT PasswordHash,RoleId FROM Users " +
                        "WHERE Email=@Email";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", authorization.Login);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hachPassword = reader.GetString(0);
                                role = reader.GetInt32(1);

                            }
                        }
                    }
                    if (role == 1)
                    {
                        string groupQuery = "SELECT GroupId FROM Students " +
                            "WHERE Email=@Email";
                        using (SqlCommand commandGroup = new SqlCommand(groupQuery, connection))
                        {
                            commandGroup.Parameters.AddWithValue("@Email", authorization.Login);
                            using (SqlDataReader readerGroup = commandGroup.ExecuteReader())
                            {
                                if (readerGroup.Read())
                                {
                                    group = readerGroup.GetInt32(0);
                                }
                            }
                        }
                    }
                    if (role == 2)
                    {
                        string getTeacher = "SELECT TeacherId FROM Users " +
                            "WHERE Email=@Email";
                        using (SqlCommand command = new SqlCommand(getTeacher, connection))
                        {
                            command.Parameters.AddWithValue("@Email", authorization.Login);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    teacher = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                }
            }
            catch(SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
            // 
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authorization.Login),
                new Claim(ClaimTypes.Role, role.ToString())
            };
            if(role == 1) { claims.Add(new Claim("Group", group.ToString())); }
            if(role == 2) { claims.Add(new Claim("Teacher", teacher.ToString())); }

            var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);

            bool isCorrectPassword = VerefyPassword(authorization.Password, hachPassword);
            if (isCorrectPassword)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Не верный логин или пароль";
                return View();
            }

        }
        private bool VerefyPassword(string password, string standHach)
        {
            byte[] hachCode = Convert.FromBase64String(standHach);
            byte[] salt = new byte[16];
            Array.Copy(hachCode, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hach = pbkdf2.GetBytes(32);
            for (int i = 0; i < 32; i++)
            {
                if (hachCode[i + 16] != hach[i])
                    return false;
            }
            return true;
        }
    }
}
