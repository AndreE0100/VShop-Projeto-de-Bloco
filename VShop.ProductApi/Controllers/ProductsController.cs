using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.ProductApi.DTOs;
using VShop.ProductApi.Roles;
using VShop.ProductApi.Services;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VShop.ProductApi.Controllers
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task Get_Returns_Ok_With_Products()
        {
            // Arrange
            var productServiceMock = new Mock<IProductService>();
            productServiceMock.Setup(p => p.GetProducts())
                              .ReturnsAsync(new List<ProductDTO>
                              {
                                  new ProductDTO { Id = 1, Name = "Product 1", Price = 10.99m },
                                  new ProductDTO { Id = 2, Name = "Product 2", Price = 15.99m }
                              });

            var controller = new ProductsController(productServiceMock.Object);

            // Act
            var actionResult = await controller.Get();
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var products = Assert.IsAssignableFrom<IEnumerable<ProductDTO>>(result.Value);
            Assert.Equal(2, products.Count());
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Get()
        {
            var produtosDto = await _productService.GetProducts();
            if (produtosDto == null)
            {
                return NotFound("Products not found");
            }
            return Ok(produtosDto);
        }

        // Other controller actions...
    }
}
