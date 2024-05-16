using APICatalogo.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    public class Repository<T> : IRepository<T> where T : class // where T : class    força o T a ser sempre uma classe
    {
        protected readonly AppDbContext _context; //O modificador aqui tem que ser protected pq esse atributo precisa ser acessado pela herança

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>().AsNoTracking().ToList();
        }

        public T? Get(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().FirstOrDefault(predicate);
        }

        public T Create(T entity)
        {
            _context.Set<T>().Add(entity);
            //_context.SaveChanges();
            return entity;
        }
        
        public T Update(T entity)
        {
            //_context.Entry(entity).State = EntityState.Modified; // <- Poderia ser usado assim tbm
            _context.Set<T>().Update(entity);
            //_context.SaveChanges();
            return entity;
        }

        public T Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            //_context.SaveChanges();
            return entity;
        }
    }
}
