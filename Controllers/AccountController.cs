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
                    CreditCard card = getCardByID(account.Id);

                    if (account.Role.Equals("Admin"))
                        return View("AdminDashboard", card);
                    else
                        return View("UserDashboard", card);
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



        //private Account ValidateUser(string username, string password)
        //{

        //    AccountViewModel accountViewModel = new AccountViewModel
        //    {
        //        account = new Account(),
        //        accountsList = getAccountsList()
        //    };

        //    for (int i = 0; i < accountViewModel.accountsList.Count; i++)
        //    {
        //        Account account = accountViewModel.accountsList[i];
        //        if (account.Username.Equals(username)){
        //            string str1 = account.PasswordHash;
        //            string str2 = ComputeSha256Hash(password);
        //            if (str1.Equals(str2))
        //            {
        //                return account;
        //            }else
        //            return null;
        //        }
        //    }
        //    return null;
        //}

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
                Account account = getAccountByUserName(model.Username, model.PasswordHash); 
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

        //private Account GetAccountByUserName(string UserName)
        //{
        //    List<Account> accounts = getAccountsList();
        //    foreach (var account in accounts)
        //    {
        //        if (account.Username.Equals(UserName)) return account;
        //    }
        //    return null;
        //}

        public ActionResult AdminDashboard()
        {
            string username = Session["UserName"]?.ToString();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            // Assuming you have a method to get the AccountId
            var account = getAccountByUserName(username);
            if (account == null)
                return HttpNotFound();

            // Try to find existing card
            var card = getCardByID(account.Id);

            // If not found, create empty one with AccountId set
            if (card == null)
                card = new CreditCard { AccountId = account.Id };
            return View(card); // Return it to the view

        }
        public ActionResult UserDashboard()
        {
            string username = Session["UserName"]?.ToString();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            // Assuming you have a method to get the AccountId
            var account = getAccountByUserName(username);
            if (account == null)
                return HttpNotFound();

            // Try to find existing card
            var card = getCardByID(account.Id);

            // If not found, create empty one with AccountId set
            if (card == null)
                card = new CreditCard { AccountId = account.Id };
            return View(card); // Return it to the view
        }

        private CreditCard getCardByID(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"SELECT AccountId, FirstName, LastName, IDNumber, CardNumber, ValidDate, CVC 
                            FROM CreditCards 
                            WHERE AccountId = @AccountId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CreditCard
                            {
                                AccountId = reader.GetInt32(0),
                                CardNumber = reader.GetString(4),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                IDNumber = reader.GetString(3),
                                ValidDate = reader.GetString(5),
                                CVC = reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return null;
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        //secured

        private Account getAccountByUserName(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT Id, UserName, PasswordHash, Role FROM Accounts WHERE UserName = @UserName";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserName", username);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3),
                            };
                        }
                    }
                }
            }
            return null;
        }




        //Secured
        //private Account ValidateUser(string username, string password)
        //{
        //    Account account = getAccountByUserName(username);
        //    if (account != null)
        //    {
        //        string enteredHash = ComputeSha256Hash(password);
        //        if (account.PasswordHash.Equals(enteredHash))
        //        {
        //            return account;
        //        }
        //    }
        //    return null;
        //}

        private Account getAccountByUserName(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT Id, UserName, PasswordHash, Role FROM Accounts " +
                                  "WHERE UserName = '" + username + "' AND PasswordHash = '" + ComputeSha256Hash(password) + "'";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3),
                            };
                        }
                    }
                }
            }
            return null;
        }


        private Account ValidateUser(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string enteredHash = ComputeSha256Hash(password);
                // VULNERABLE TO SQL INJECTION (By-Pass Password)
                string sqlQuery = "SELECT Id, UserName, PasswordHash, Role FROM Accounts WHERE UserName = '" + username + "' AND PasswordHash = '" + enteredHash + "' ";

                // VULNERABLE TO SQL INJECTION (Attacking Password)
                //string sqlQuery = "SELECT Id, UserName, PasswordHash, Role FROM Accounts WHERE UserName = '" + username + "' AND PasswordHash = '" + password + "' ";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Role = reader.GetString(3),
                            };
                        }
                    }
                }
            }
            return null;
        }


        [HttpPost]
        public ActionResult UpdateCardAdmin(CreditCard updatedCard)
        {
            if (!ModelState.IsValid)
            {
               return View("AdminDashboard", updatedCard);
            }

            SaveCardChangesToDB(updatedCard);

            return View("AdminDashboard", updatedCard);
        }

        [HttpPost]
        public ActionResult UpdateCard(CreditCard updatedCard)
        {
            if (!ModelState.IsValid)
            {
                return View("UserDashboard", updatedCard);
            }

            SaveCardChangesToDB(updatedCard);

            return View("UserDashboard", updatedCard);
        }

        private void SaveCardChangesToDB(CreditCard card)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = @"UPDATE CreditCards 
                            SET CardNumber = @CardNumber, FirstName = @FirstName, LastName = @LastName, 
                                IDNumber = @IDNumber, ValidDate = @ValidDate, CVC = @CVC 
                            WHERE AccountId = @AccountId";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", card.AccountId);
                    command.Parameters.AddWithValue("@CardNumber", card.CardNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FirstName", card.FirstName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LastName", card.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IDNumber", card.IDNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ValidDate", card.ValidDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CVC", card.CVC ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
          
        }


    }

}