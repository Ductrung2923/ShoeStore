using Microsoft.AspNetCore.Mvc;
using PjPRN222.Models;

namespace PjPRN222.Controllers
{
	public class MenController : Controller
	{
		public IActionResult MenShoes()
		{
			ShoeStoreContext st = new ShoeStoreContext();
			List<Product> productslist = st.Products.Where(x => x.CategoryId == 5 || x.CategoryId == 6 || x.CategoryId == 7
			|| x.CategoryId == 8 || x.CategoryId == 9).ToList();

			return View(productslist);
		}
	}
}
