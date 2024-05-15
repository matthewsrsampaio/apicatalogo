using APICatalogo.Context;

namespace APICatalogo.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private IProdutoRepository? _produtoRepo;
        private ICategoriaRepository? _categoriaRepo;
        public AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

       
        //LAZY LOADING significa adia a obtenção dos objetos até que eles sejam realmente necessários.
        //LAZY LOADING - Com os métodos abaixo eu garanto que eu só criarei uma instância caso eu não tenha nenhuma.
        public IProdutoRepository ProdutoRepository
        {
            get
            {
                // ?? -> Coalescência Nula - Verifica se já existe uma instância de IProdutoReepository, se não exister nós criaremos uma nova.
                return _produtoRepo = _produtoRepo ?? new ProdutoRepository(_context);
            }
        }

        public ICategoriaRepository CategoriaRepository
        {
            get
            {
                // ?? -> Coalescência Nula - Verifica se já existe uma instância de IProdutoReepository, se não exister nós criaremos uma nova.
                return _categoriaRepo = _categoriaRepo ?? new CategoriaRepository(_context);
            }
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        //Libera todos os recursos alocados do contexto
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
