using Microsoft.AspNetCore.Mvc;
using PjPRN222.Models;

namespace PjPRN222.Controllers
{
	public class HomeSalerController : Controller
	{
		public IActionResult Index()
		{
            ShoeStoreContext st = new ShoeStoreContext();
            List<Product> products = st.Products.ToList();
            return View(products);
		}

        
       

    }
}
