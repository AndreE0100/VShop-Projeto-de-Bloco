using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VShop.Web.Models;
using VShop.Web.Roles;
using VShop.Web.Services.Contracts;
using Xunit;

namespace VShop.Web.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductService productService,
                                  ICategoryService categoryService,
                                  IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductViewModel>>> Index()
        {
            var result = await _productService.GetAllProducts(await GetAccessToken());
            if (result is null)
                return View("Error");
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(await GetAccessToken()), "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductViewModel productVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _productService.CreateProduct(productVM, await GetAccessToken());
                if (result != null)
                    return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(await GetAccessToken()), "CategoryId", "Name");
            }
            return View(productVM);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {
            ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(await GetAccessToken()), "CategoryId", "Name");
            var result = await _productService.FindProductById(id, await GetAccessToken());
            if (result is null)
                return View("Error");
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(ProductViewModel productVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _productService.UpdateProduct(productVM, await GetAccessToken());
                if (result is not null)
                    return RedirectToAction(nameof(Index));
            }
            return View(productVM);
        }

        [HttpGet]
        public async Task<ActionResult<ProductViewModel>> DeleteProduct(int id)
        {
            var result = await _productService.FindProductById(id, await GetAccessToken());
            if (result is null)
                return View("Error");
            return View(result);
        }

        [HttpPost, ActionName("DeleteProduct")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteProductById(id, await GetAccessToken());
            if (!result)
                return View("Error");
            return RedirectToAction("Index");
        }

        private async Task<string> GetAccessToken()
        {
            return await HttpContext.GetTokenAsync("access_token");
        }
    }

    public class ProductsControllerTests
    {
        [Fact]
        public async Task Index_Returns_View_With_Products()
        {
            // Arrange
            var productServiceMock = new Mock<IProductService>();
            productServiceMock.Setup(p => p.GetAllProducts(It.IsAny<string>()))
                              .ReturnsAsync(new List<ProductViewModel>
                              {
                          new ProductViewModel { Id = 1, Name = "Product 1", Price = 10.99m },
                          new ProductViewModel { Id = 2, Name = "Product 2", Price = 15.99m }
                              });

            var categoryServiceMock = new Mock<ICategoryService>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            var controller = new ProductsController(productServiceMock.Object, categoryServiceMock.Object, webHostEnvironmentMock.Object);

            // Act
            var result = await controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ProductViewModel>>(viewResult.Model);

            // Assert
            Assert.NotNull(viewResult);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count());
            Assert.Equal(1, model.ElementAt(0).Id);
            Assert.Equal("Product 1", model.ElementAt(0).Name);
            Assert.Equal(10.99m, model.ElementAt(0).Price);
            Assert.Equal(2, model.ElementAt(1).Id);
            Assert.Equal("Product 2", model.ElementAt(1).Name);
            Assert.Equal(15.99m, model.ElementAt(1).Price);
        }

    }
}
