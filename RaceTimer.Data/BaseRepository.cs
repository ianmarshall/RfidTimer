using NLog;
using RaceTimer.Data;
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


    public abstract class BaseRepository<T> :
        IGenericRepository<T> where T : class 
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //protected C _entities = new C();
        //public C Context
        //{


        //    get { return _entities; }
        //    set { _entities = value; }
        //}
        private static readonly object padlock = new object();
        private static RaceTimerContext context;
        public static RaceTimerContext Context
        {
            get
            {
                lock (padlock)
                {
                    if (context == null)
                    {
                        context = new RaceTimerContext();
                        return context;
                    }
                    return context;
                }
            }
        }

        public virtual IQueryable<T> GetAll()
        {

            IQueryable<T> query = Context.Set<T>();
            return query;
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {

            IQueryable<T> query = Context.Set<T>().Where(predicate);
            return query;
        }

        public virtual void Add(T entity)
        {
            Context.Set<T>().Add(entity);
        }

        public virtual void Delete(T entity)
        {
            Context.Set<T>().Remove(entity);
        }

        public virtual void Edit(T entity, int key)
        {
           // _entities.Entry(entity).State = System.Data.EntityState.Modified;

            if (entity == null)
                return;

            T existing = Context.Set<T>().Find(key);
            if (existing != null)
            {
                Context.Entry(existing).CurrentValues.SetValues(entity);
                Context.SaveChanges();
            }
        }

        public virtual void Save()
        {
            try
            {

                //using (DbContextTransaction dbTran = _entities.Database.BeginTransaction())
                //{
                Context.SaveChanges();
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
            IEnumerable<DbEntityEntry> res = from e in Context.ChangeTracker.Entries()
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