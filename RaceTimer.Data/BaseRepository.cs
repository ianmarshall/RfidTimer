using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace RaceTimer.Data
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Delete(T entity);
        void Edit(T entity, int key);
        void Save();
    }


    public abstract class BaseRepository<C, T> :
        IGenericRepository<T> where T : class where C : DbContext, new()
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected C _entities = new C();
        public C Context
        {

            get { return _entities; }
            set { _entities = value; }
        }

        public virtual IQueryable<T> GetAll()
        {

            IQueryable<T> query = _entities.Set<T>();
            return query;
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {

            IQueryable<T> query = _entities.Set<T>().Where(predicate);
            return query;
        }

        public virtual void Add(T entity)
        {
            _entities.Set<T>().Add(entity);
        }

        public virtual void Delete(T entity)
        {
            _entities.Set<T>().Remove(entity);
        }

        public virtual void Edit(T entity, int key)
        {
           // _entities.Entry(entity).State = System.Data.EntityState.Modified;

            if (entity == null)
                return;

            T existing = _entities.Set<T>().Find(key);
            if (existing != null)
            {
                _entities.Entry(existing).CurrentValues.SetValues(entity);
                _entities.SaveChanges();
            }
        }

        public virtual void Save()
        {
            try
            {

                //using (DbContextTransaction dbTran = _entities.Database.BeginTransaction())
                //{
                _entities.SaveChanges();
                //commit transaction
                //    dbTran.Commit();
                //}

            }
            catch(Exception ex)
            {
                logger.Error(ex);

                throw;
            }
        }

        public bool IsDirty()
        {
            // Query the change tracker entries for any adds, modifications, or deletes.
            IEnumerable<DbEntityEntry> res = from e in _entities.ChangeTracker.Entries()
                where e.State.HasFlag(EntityState.Added) ||
                      e.State.HasFlag(EntityState.Modified) ||
                      e.State.HasFlag(EntityState.Deleted)
                select e;

            if (res.Any())
                return true;

            return false;

        }
    }
}