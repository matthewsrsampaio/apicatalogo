using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ApiCatalogoxUnitTests.UnitTests

    /* O que eu entendi até aqui: 
     * Esta classe(GetProdutoUnitTests) que está no projeto de Tests, dentro do
     * projeto APICatalogo, vai acessar a classe Produtos controller que está no projeto APICatalogo através
       da classe ProdutosUnitTestController para fazer os testes unitários.
    */
{
    //Implementando a classe IClassFixture passando a instância de ProdutosUnitTestController
    public class GetProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        //Injeção via construtor
        public GetProdutoUnitTests(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }

        [Fact]
        public async Task GetProdutoById_OkResult() 
        {
            //Arrange
            var prodId = 2;

            //Act
            var data = await _controller.GetProduto(prodId);

            //Assert (xUnit)
            //var okResult = Assert.IsType<OkObjectResult>(data.Result);
            //Assert.Equal(200, okResult.StatusCode);

            //Assert (FluentAssertions)
            data.Result.Should().BeOfType<OkObjectResult>() //Verifica se o resultado é do tipo OkObjectResult
                .Which.StatusCode.Should().Be(200); //Verifica se o código de status do OkObjectResult é 200
        }

        [Fact]
        public async Task GetProdutoById_Return_NotFound() 
        {
            //Arrange
            var prodId = 999;

            //Act
            var data = await _controller.GetProduto(prodId);

            //Assert
            data.Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.StatusCode.Should().Be(404);

        }

        [Fact]
        public async Task GetProdutoById_Return_BadRequest()
        {
            //Arrange
            var prodId = -1;

            //Act
            var data = await _controller.GetProduto(prodId);

            //Assert
            data.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.StatusCode.Should().Be(400);

        }

        [Fact]
        public async Task GetProdutos_Return_ListOfProdutoDTO() 
        {
            //Arrange

            //Act
            var data = await _controller.GetProdutos();

            //Assert
            data.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<ProdutoDTO>>() //Verifica se o valor do OkObjectResult é atribuível a IEnumerable<ProdutoDTO>
                .And.NotBeNull(); //Verifica se o retorno é nulo

        }

        //Para esse teste passar é necessário forjar uma exceção em GetProdutos ou que o BadRequest() aconteça de fato.
        [Fact]
        public async Task GetProdutos_Return_BadRequestResult()
        {
            //Arrange

            //Act
            var data = await _controller.GetProdutos();

            //Assert
            data.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        //Para esse teste passar é necessário forjar um retorno NotFound() em GetProdutos ou que o NotFound() aconteça de fato.
        [Fact]
        public async Task GetProdutos_Return_NotFoundResult()
        {
            //Arrange

            //Act
            var data = await _controller.GetProdutos();

            //Assert
            data.Result.Should().BeOfType<NotFoundResult>();
        }

    }
}
