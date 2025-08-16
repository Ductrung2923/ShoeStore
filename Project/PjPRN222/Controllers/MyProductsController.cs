using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PjPRN222.Models;

namespace PjPRN222.Controllers
{
	public class MyProductsController : Controller
	{
		public IActionResult Index()
		{
			int? userId = HttpContext.Session.GetInt32("UserId");
			ShoeStoreContext st = new ShoeStoreContext();
			List<Product> productslist = st.Products.Where(p => p.SallerId == userId).ToList();	

			return View(productslist);
			
		}


		public IActionResult addPro()
		{
			
            ShoeStoreContext st = new ShoeStoreContext();
            ViewBag.CategoryId = new SelectList(st.Categories.ToList(), "CategoryId", "CategoryName");
			return View();
		}
           

		[HttpPost]
		public IActionResult addPro(Product product)
		{
            int? userId = HttpContext.Session.GetInt32("UserId");
            ShoeStoreContext st = new ShoeStoreContext();

			product.SallerId = userId;
			st.Products.Add(product);
			st.SaveChanges();

			return RedirectToAction("Index");
		}


		public IActionResult Details(int id)
		{
			ShoeStoreContext fu = new ShoeStoreContext();
			var ls = fu.Products.FirstOrDefault(x => x.ProductId == id);			
			return View(ls);
		}

        [HttpPost]
        public IActionResult editpro(Product pro)
        {
            ShoeStoreContext fu = new ShoeStoreContext();
            Product product = fu.Products.FirstOrDefault(x => x.ProductId == pro.ProductId);

            product.ProductName = pro.ProductName;
            product.Description = pro.Description;
            product.Price = pro.Price;
			product.Quantity = pro.Quantity;
            
            fu.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            ShoeStoreContext fu = new ShoeStoreContext();
            Product pro = fu.Products.FirstOrDefault(x => x.ProductId == id);
            fu.Products.Remove(pro);
            fu.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
