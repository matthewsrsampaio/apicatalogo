using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class DeleteProdutoUnitTest : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        public DeleteProdutoUnitTest(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }

        [Fact]
        public async Task DeleteProdutoById_Return_OkResult()
        {
            //Arrange
            var prodId = 2;

            //Act
            var result = await _controller.Delete(prodId);

            //Assert
            result.Should().NotBeNull(); //Verifica se é nulo
            result.Result.Should().BeOfType<OkObjectResult>(); //Verifica se o resultado é OkResult.
        }

        [Fact]
        public async Task DeleteProdutoById_Return_NorFoundResult()
        {
            //Arrange
            var prodId = 99;

            //Act
            var result = await _controller.Delete(prodId) as ActionResult<ProdutoDTO>;

            //Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NotFoundObjectResult>();

        }

    }
}
