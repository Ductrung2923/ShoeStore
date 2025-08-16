using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PjPRN222.Services;

namespace PjPRN222.Controllers
{
    public class GhnController : Controller
    {
        private readonly GhnService _ghnService;

        public GhnController(GhnService ghnService)
        {
            _ghnService = ghnService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            var data = await _ghnService.GetProvincesAsync();
            return Content(data, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> GetDistrict([FromBody] int provinceId)
        {
            var data = await _ghnService.GetDistrictAsync(provinceId);
            return Content(data, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> GetWard([FromBody] int districtId)
        {
            var data = await _ghnService.GetWardAsync(districtId);
            return Content(data, "application/json");
        }

    }
}
