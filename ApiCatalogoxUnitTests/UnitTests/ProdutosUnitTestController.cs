using APICatalogo.Context;
using APICatalogo.Controllers;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class ProdutosUnitTestController
    {
        //Lembrando que métodos e variáveis estáticas são carregados apenas a primeira vez.

        public IUnitOfWork repository;
        public IMapper mapper;

        public static DbContextOptions<AppDbContext> dbContextOptions {  get; }

        public static string connectionString =
            "Server=localhost;DataBase=CatalogoDB;Uid=root;Pwd=123456";

        static ProdutosUnitTestController() 
        {
            dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options;
        }

        public ProdutosUnitTestController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProdutoDTOMappingProfile());
            });

            mapper = config.CreateMapper();
            var context = new AppDbContext(dbContextOptions);
            repository = new UnitOfWork(context);
        }
    }
}
