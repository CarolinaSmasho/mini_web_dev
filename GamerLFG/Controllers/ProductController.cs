using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // GET /Product
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ดึงข้อมูลทั้งหมดจาก MongoDB
            var products = await _productService.GetAllAsync();

            // ส่ง list ไปที่ View ผ่าน Model
            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
