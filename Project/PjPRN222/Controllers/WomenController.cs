using Microsoft.AspNetCore.Mvc;
using PjPRN222.Models;

namespace PjPRN222.Controllers
{
	public class WomenController : Controller
	{
		public IActionResult WomenShoes()
		{
			ShoeStoreContext st = new ShoeStoreContext();
			List<Product> productslist = st.Products.Where(x => x.CategoryId == 10 || x.CategoryId == 11 || x.CategoryId == 12
			|| x.CategoryId == 13 || x.CategoryId == 14).ToList();
			return View(productslist);
		}
	}
}
