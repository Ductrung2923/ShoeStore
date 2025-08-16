using Microsoft.AspNetCore.Mvc;
using PjPRN222.Models;

namespace PjPRN222.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            ShoeStoreContext st = new ShoeStoreContext();
            List<User> listUser = st.Users
            .Where(u => u.Role == "Saler" || u.Role == "Customer")
            .ToList();
            return View(listUser);
        }


        [HttpPost]
        public IActionResult Confirm (int UserId)
        {

            using (ShoeStoreContext st = new ShoeStoreContext())
            {
                var user = st.Users.Find(UserId);
                if (user != null)
                {
                    user.IsActive = !user.IsActive; // Đảo ngược trạng thái Active/Inactive
                    st.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (ShoeStoreContext st = new ShoeStoreContext())
            {
                var user = st.Users.Find(id);
                if (user != null)
                {
                    st.Users.Remove(user);
                    st.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

    }

}
