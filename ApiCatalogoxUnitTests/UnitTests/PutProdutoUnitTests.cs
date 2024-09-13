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
    public class PutProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        public PutProdutoUnitTests(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }

        //Testes de unidade para PUT
        [Fact]
        public async Task PutProduto_Return_OkResult()
        {
            //Arrange
            var prodId = 9;

            var updatedProdutoDto = new ProdutoDTO
            {
                ProdutoId = prodId,
                Nome = "Produto atualizado - Testes",
                Preco = 100,
                Descricao = "Minha descricao testes",
                ImagemUrl = "imagemtestes.jpg",
                CategoriaId = 2
            };

            //Act
            var result = await _controller.Put(prodId, updatedProdutoDto) as ActionResult<ProdutoDTO>;

            //Assert
            result.Should().NotBeNull(); //Verifica se o resultado não é nulo
            result.Result.Should().BeOfType<OkObjectResult>(); //Verifica se o resultado OkObjectResult

        }

        [Fact]
        public async Task PutProduto_Return_BadRequestResult()
        {
            //Arrange
            var prodId = 19;

            var updatedProdutoDto = new ProdutoDTO
            {
                ProdutoId = 9,
                Nome = "Produto atualizado - Testes",
                Preco = 1000,
                Descricao = "Minha Descricao Testes",
                ImagemUrl = "imagemtestes.jpg",
                CategoriaId = 3
            };

            //Act
            var result = await _controller.Put(prodId, updatedProdutoDto) as ActionResult<ProdutoDTO>;

            //Assert
            result.Result.Should().BeOfType<BadRequestResult>().Which.StatusCode.Should().Be(400);

        }

    }
}
