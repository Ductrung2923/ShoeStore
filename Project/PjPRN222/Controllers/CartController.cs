using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PjPRN222.Models;
using System.Security.Claims;

namespace PjPRN222.Controllers
{
	public class CartController : Controller
	{
		private readonly ShoeStoreContext _context;

		public CartController(ShoeStoreContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerId == userId.Value);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartItems = new List<CartItem>()
                };
            }

            return View(cart);
		}

		[HttpPost]
		public IActionResult Add(int productId)
		{
			
			var userId = HttpContext.Session.GetInt32("UserId");


			// 1. Tìm giỏ hàng hiện tại
			var cart = _context.Carts.FirstOrDefault(c => c.CustomerId == userId.Value);
			if (cart == null)
			{
				cart = new Cart
				{
					CustomerId = userId.Value,
					CreatedAt = DateTime.Now
				};
				_context.Carts.Add(cart);
				_context.SaveChanges(); 
			}

			// 2. Kiểm tra sản phẩm đã có trong giỏ hàng chưa
			var cartItem = _context.CartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
			if (cartItem != null)
			{
				cartItem.Quantity += 1;
			}
			else
			{
				cartItem = new CartItem
				{
					CartId = cart.CartId,
					ProductId = productId,
					Quantity = 1,
					CreatedAt = DateTime.Now
				};
				_context.CartItems.Add(cartItem);
			}

			_context.SaveChanges();

            return RedirectToAction("Index", "Home"); 
		}

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var item = _context.CartItems.Find(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var item = _context.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (item != null)
            {
                item.Quantity = quantity;
                _context.SaveChanges();
                return Ok();
            }

            return NotFound();
        }



    }
}
