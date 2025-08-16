using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PjPRN222.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace PjPRN222.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ShoeStoreContext _context;

        public ProfileController(ShoeStoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userSession = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = JsonConvert.DeserializeObject<User>(userSession);
            return View(user);
        }

        [HttpPost]
        public IActionResult Update(User model)
        {
            var userSession = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = JsonConvert.DeserializeObject<User>(userSession);
            var existingUser = _context.Users.Find(user.UserId); // Dùng Find() thay vì FirstOrDefault()

            if (existingUser == null)
            {
                return NotFound();
            }
        
            // Cập nhật thông tin user
            existingUser.PasswordHash = model.PasswordHash;
            existingUser.Email = model.Email;
            existingUser.PhoneNumber = model.PhoneNumber;
            existingUser.Address = model.Address;
            existingUser.UpdatedAt = DateTime.Now;

            _context.Users.Update(existingUser); // Đảm bảo Entity Framework theo dõi thay đổi
            var affectedRows = _context.SaveChanges(); // Lưu vào database

            Console.WriteLine($"Rows affected: {affectedRows}");

            // 🔥 Lấy lại dữ liệu mới nhất từ database
            var updatedUser = _context.Users.Find(user.UserId);

            // 🔥 Cập nhật lại session với dữ liệu mới
            HttpContext.Session.SetString("User", JsonConvert.SerializeObject(updatedUser));

            return RedirectToAction("Index");
        }



    }
}
