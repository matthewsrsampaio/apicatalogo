using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    //Esse será um repositório genérico. Vai evitar repetição de código
    //Cuidade para não violar o princípio ISP - Não forçar o cliente a depender de interfaces que não utilizam
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        
        T? Get(Expression<Func<T,bool>> predicate); //Esse get vai receber uma função Lambda como parâmetro

        T Create(T entity);

        T Update(T entity);

        T Delete(T entity);

    }
}
