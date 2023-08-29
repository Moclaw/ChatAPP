using System.Linq.Expressions;
using ChatAPP.API.Contexts;

namespace ChatAPP.API.Services
{
    public class BaseServices<T> where T : class
    {
        protected readonly ChatAPPContext _context;
        protected readonly ILogger<T> _logger;

        public BaseServices(ChatAPPContext context, ILogger<T> logger)
        {
            _context = context;
            _logger = logger;
        }
        public T GetById(int Id)
        {
            try
            {
                return _context.Set<T>().Find(Id)!;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null!;
            }
        }

        public int Add(T entity)
        {
            int result = 0;
            try
            {
                _context.Set<T>().Add(entity);
                result = _context.SaveChanges();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return result;
            }
        }

        public async Task<int> AddAsync(T entity)
        {
            int result = 0;
            try
            {
                await _context.Set<T>().AddAsync(entity);
                result = await _context.SaveChangesAsync();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return result;
            }
        }

        public int Update(T entity)
        {
            int result = 0;
            try
            {
                _context.Set<T>().Update(entity);
                result = _context.SaveChanges();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<int> UpdateAsync(T entity)
        {
            int result = 0;
            try
            {
                _context.Set<T>().Update(entity);
                result = await _context.SaveChangesAsync();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return result;
            }
        }

        public int Delete(T entity)
        {
            int result = 0;
            try
            {
                _context.Set<T>().Remove(entity);
                result = _context.SaveChanges();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return result;
            }
        }

        public async Task<int> DeleteAsync(T entity)
        {
            int result = 0;
            try
            {
                _context.Set<T>().Remove(entity);
                result = await _context.SaveChangesAsync();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return result;
            }
        }

        public List<T> GetAll()
        {
            try
            {
                return _context.Set<T>().ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null!;
            }
        }

        public List<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            try
            {
                return _context.Set<T>().Where(expression).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null!;
            }
        }

        public T GetSingleByCondition(Expression<Func<T, bool>> expression)
        {
            try
            {
                return _context.Set<T>().FirstOrDefault(expression)!;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null!;
            }
        }

        public int Count(Expression<Func<T, bool>> expression)
        {
            try
            {
                return _context.Set<T>().Count(expression);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return 0;
            }
        }

        public bool Exist(Expression<Func<T, bool>> expression)
        {
            try
            {
                return _context.Set<T>().Any(expression);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public bool Exist(int Id)
        {
            try
            {
                return _context.Set<T>().Find(Id) != null;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }


    }
}
