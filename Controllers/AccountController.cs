using DataSec.Models;
using DataSec.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DataSec.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Required)]
    public class AccountController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                Account account = ValidateUser(login.UserName, login.Password);

                // Debug output
                //System.Diagnostics.Debug.WriteLine($"Account found: {account != null}");
                if (account != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"Setting session for {account.Username}");

                    Session["AccountId"] = account.Id;
                    Session["UserName"] = account.Username;
                    Session["Role"] = account.Role;

                    // Debug session values
                    //System.Diagnostics.Debug.WriteLine($"Session AccountId: {Session["AccountId"]}");
                    //System.Diagnostics.Debug.WriteLine($"Session UserName: {Session["UserName"]}");
                    //System.Diagnostics.Debug.WriteLine($"Session Role: {Session["Role"]}");

                    if (account.Role.Equals("Admin"))
                        return View("AdminDashboard");
                    else
                        return View("UserDashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }

            return View(login);
        }
        private List<Account> getAccountsList()
        {
            List<Account> accounts = new List<Account>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT Id, UserName, PasswordHash, Role FROM Accounts";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(new Account
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3),
                            });
                        }

                    }
                }
            }
            return accounts;
        }



        private Account ValidateUser(string username, string password)
        {

            AccountViewModel accountViewModel = new AccountViewModel
            {
                account = new Account(),
                accountsList = getAccountsList()
            };

            for (int i = 0; i < accountViewModel.accountsList.Count; i++)
            {
                Account account = accountViewModel.accountsList[i];
                if (account.Username.Equals(username)){
                    string str1 = account.PasswordHash;
                    string str2 = ComputeSha256Hash(password);
                    if (str1.Equals(str2))
                    {
                        return account;
                    }else
                    return null;
                }
            }
            return null;
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        [HttpGet]
        public ActionResult ForgetPassword() { return View(); }

        [HttpGet]
        public ActionResult ResetPassword() => View();

        [HttpPost]
        public ActionResult ResetPassword(Account model)
        {
                Account account = getAccountByUserName(model.Username);
                if (account != null)
                {
                    account.PasswordHash = ComputeSha256Hash(model.PasswordHash);
                    SaveNewPasswordToDB(account.Username, account.PasswordHash);
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "User not found.");

            return View("ForgetPassword", model);
        }

        private void SaveNewPasswordToDB(string username, string newPasswordHash)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE Accounts SET PasswordHash = @PasswordHash WHERE UserName = @UserName";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserName", username);
                    command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);

                    command.ExecuteNonQuery();
                }
            }
        }

        private Account getAccountByUserName(string UserName)
        {
            List<Account> accounts = getAccountsList();
            foreach (var account in accounts)
            {
                if (account.Username.Equals(UserName)) return account;
            }
            return null;
        }

        public ActionResult AdminDashboard()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
        public ActionResult UserDashboard()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }

}