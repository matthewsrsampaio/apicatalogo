﻿using ApiCatalogo.Models;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Repositories
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        IEnumerable<Produto> GetProdutosPorCategoria(int id);

        /*//Get
        IQueryable<Produto> GetProdutos();

        //Get(id)
        Produto GetProduto(int id);

        //Post
        Produto Create(Produto produto);

        //Put
        bool Update(Produto produto);

        //Delete
        bool Delete(int id);*/
    }
}
