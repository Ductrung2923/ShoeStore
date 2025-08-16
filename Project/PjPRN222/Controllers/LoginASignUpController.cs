using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PjPRN222.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Net;
using BCrypt.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PjPRN222.Controllers
{
    public class LoginASignUpController : Controller
    {
        private readonly EmailService _emailService;

        public LoginASignUpController(EmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /////////////////////////////////LOGIN////////////////////////////////////////////

        public User GetUserByUsername(string username)
        {
            using (var db = new ShoeStoreContext())
            {
                return db.Users.FirstOrDefault(a => a.Username == username);
            }
        }

        public bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            if (enteredPassword == storedPasswordHash)
            {
                return true;
            }
            return false;
        }

        public bool Loginnm(string username, string password)
        {
            var user = GetUserByUsername(username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
                HttpContext.Session.SetInt32("UserId", user.UserId);


				return true;
            }
            return false;
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {

            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Message1 = "Vui lòng nhập tên tài khoản!";
                return View("Index");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Message1 = "Vui lòng nhập mật khẩu!";
                return View("Index");
            }
            username = username.Trim();

            var user = GetUserByUsername(username);

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                ViewBag.Message1 = "Tên tài khoản không được chứa ký tự đặc biệt!";
                return View("Index");
            }
            if (user == null)
            {
                ViewBag.Message1 = "Sai tài khoản hoặc mật khẩu!";
                return View("Index");
            }

            if (!user.IsActive)
            {
                ViewBag.Message1 = "Tài khoản của bạn chưa được kích hoạt!";
                return View("Index");
            }

            if (user.Role == "Admin" && Loginnm(username, password))
            {
                return RedirectToAction("Index", "HomeAdmin");
            }
            else if (user.Role == "Saler" && Loginnm(username, password))
            {
                return RedirectToAction("Index", "HomeSaler");
            }
            else if (Loginnm(username, password))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Message1 = "Sai tài khoản hoặc mật khẩu!";
                return View("Index");
            }
        }

        ///////////////////////////////////////////SIGN UP/////////////////////////////////

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task SendOTP(string email, string otp)
        {
            string subject = "Xác nhận OTP đăng ký";
            string body = $"Mã OTP của bạn là: {otp}. Vui lòng nhập OTP để xác nhận tài khoản.";
            await _emailService.SendEmailAsync(email, subject, body);
        }

        [HttpPost]
        public async Task<JsonResult> SignUp(string username, string password, string repeatPassword, string email, string phoneNumber, string address)
        {
            // Trim khoảng trắng
            username = username.Trim();
            email = email.Trim();
            phoneNumber = phoneNumber.Trim();

            // Kiểm tra username: Không chứa ký tự đặc biệt & khoảng trắng
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                return Json(new { success = false, message = "Tên tài khoản không hợp lệ! Chỉ được chứa chữ, số, dấu gạch dưới." });
            }

            // Kiểm tra email hợp lệ
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return Json(new { success = false, message = "Email không hợp lệ!" });
            }

            // Kiểm tra số điện thoại: Chỉ chứa 10 số và bắt đầu bằng 0
            if (!Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
            {
                return Json(new { success = false, message = "Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0!" });
            }

            if (password != repeatPassword)
            {
                return Json(new { success = false, message = "Mật khẩu nhập lại không khớp!" });
            }

            using (var db = new ShoeStoreContext())
            {
                if (db.Users.Any(u => u.Email == email))
                {
                    return Json(new { success = false, message = "Email đã được sử dụng!" });
                }

                string otp = GenerateOTP();
                try
                {
                    await SendOTP(email, otp);
                }
                catch (Exception ex)
                {
                    HttpContext.Session.Remove("PendingUser");
                    HttpContext.Session.Remove("OTP");
                    HttpContext.Session.Remove("OTPCreatedTime");
                    return Json(new { success = false, message = "Không thể gửi OTP: " + ex.Message });
                }

                HttpContext.Session.SetString("PendingUser", JsonConvert.SerializeObject(new
                {
                    Username = username,
                    PasswordHash = password,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Address = address
                }));

                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OTPCreatedTime", DateTime.Now.ToString());

                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn." });
            }
        }


        [HttpPost]
        public JsonResult VerifyOTP(string otp)
        {
            string storedOTP = HttpContext.Session.GetString("OTP");
            var pendingUserJson = HttpContext.Session.GetString("PendingUser");
            string otpCreatedTime = HttpContext.Session.GetString("OTPCreatedTime");

            if (storedOTP == null || pendingUserJson == null || otpCreatedTime == null)
            {
                return Json(new { success = false, message = "Phiên đăng ký hết hạn, vui lòng thử lại!" });
            }

            DateTime createdTime = DateTime.Parse(otpCreatedTime);
            if ((DateTime.Now - createdTime).TotalMinutes > 10)
            {
                return Json(new { success = false, message = "Mã OTP đã hết hạn!" });
            }

            if (otp != storedOTP)
            {
                return Json(new { success = false, message = "Mã OTP không chính xác!" });
            }

            var pendingUser = JsonConvert.DeserializeObject<dynamic>(pendingUserJson);

            using (var db = new ShoeStoreContext())
            {
                string email = pendingUser.Email;
                if (db.Users.Any(u => u.Email == email))
                {
                    return Json(new { success = false, message = "Email đã được sử dụng!" });
                }

                try
                {
                    var newUser = new User
                    {
                        Username = pendingUser.Username,
                        PasswordHash = pendingUser.PasswordHash,
                        Email = email,
                        PhoneNumber = pendingUser.PhoneNumber,
                        Address = pendingUser.Address,
                        Role = "Customer",
                        IsActive = true
                    };
                    Console.WriteLine($"Adding user: Username={newUser.Username}, Email={newUser.Email}, PhoneNumber={newUser.PhoneNumber}, Address={newUser.Address}");
                    db.Users.Add(newUser);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving user: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    return Json(new { success = false, message = "Không thể lưu người dùng: " + ex.Message });
                }
            }

            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("PendingUser");
            HttpContext.Session.Remove("OTPCreatedTime");

            return Json(new { success = true });
        }

        /////////////////////////////////Login GG/////////////////////////////////////////////////

        public async Task LoginGG()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                TempData["Message2"] = "Không thể xác thực tài khoản Google của bạn.";
                return RedirectToAction("Index");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            if (!CheckEmailExists(email))
            {
                return await GoogleResponseSignUp();
            }

            using (var db = new ShoeStoreContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
                HttpContext.Session.SetInt32("UserId", user.UserId);


				return user.Role switch
                {
                    "Admin" => RedirectToAction("Index", "HomeAdmin"),
                    "Saler" => RedirectToAction("Index", "HomeSaler"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
        }

        private bool CheckEmailExists(string email)
        {
            using (var db = new ShoeStoreContext())
            {
                var exists = db.Users.Any(a => a.Email == email);
                if (!exists)
                {
                    Console.WriteLine($"Email {email} không tồn tại trong cơ sở dữ liệu.");
                }
                return exists;
            }
        }

        //////////////////////////////// Sign Up GG/////////////////////////////////////////////

        public async Task<IActionResult> GoogleResponseSignUp()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                TempData["Message2"] = "Không thể xác thực tài khoản Google của bạn.";
                return RedirectToAction("Index");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
            {
                TempData["Error"] = "Không thể lấy email từ tài khoản Google của bạn.";
                return RedirectToAction("Index");
            }

            using (var db = new ShoeStoreContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    var username = email.Split('@')[0];
                    var password = GenerateRandomPassword();
                    user = new User
                    {
                        Email = email,
                        Username = username,
                        PasswordHash = password,
                        Role = "Customer",
                        IsActive = true,
                    };

                    db.Users.Add(user);
                    await db.SaveChangesAsync();

                    TempData["Success"] = "Tài khoản của bạn đã được tạo. Vui lòng cập nhật thông tin cá nhân.";
                }

                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));

            }

            return RedirectToAction("Index", "Profile");
        }

        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        //////////////////////////////// Reset Pass /////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string username, string resetEmail)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(resetEmail))
            {
                TempData["ErrorMessage"] = "Username and Email address are required.";
                return RedirectToAction("Index");
            }

            User user = GetUserByUsername(username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            string verificationCode = GenerateRandomPassword();

            try
            {
                using (var db = new ShoeStoreContext())
                {
                    user.PasswordHash = verificationCode;
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password: {ex.Message}");
                TempData["ErrorMessage"] = "Error updating password.";
                return RedirectToAction("Index");
            }

            string subject = "Reset Password Verification Code";
            string message = $"Your new Password code is: {verificationCode}";

            try
            {
                await _emailService.SendEmailAsync(resetEmail, subject, message);

                TempData["Email"] = resetEmail;
                TempData["VerificationCode"] = verificationCode;
                TempData["SuccessMessage"] = "Verification code has been sent to your email. Redirecting to login...";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
        }



    }
}