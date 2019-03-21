using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using GcSite.ModuleSys.Models;

namespace GcSite.ModuleSys.DAL
{
    public class GenericRepository<TEntity> : IRepository<TEntity>
         where TEntity : EntityBase
    {
        internal DbContext _db;
        internal DbSet<TEntity> _dbSet;
        public GenericRepository(DbContext context)
        {
            this._db = context;
            this._dbSet = this._db.Set<TEntity>();
        }
        public void Delete(object id)
        {
            TEntity entity = GetEntityById(id);
            Delete(entity);
        }

        public void Delete(TEntity entity)
        {
            if (_db.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public int GetCount(Expression<Func<TEntity, bool>> where = null)
        {
            if (where == null)
                return _dbSet.Count();
            else
                return _dbSet.Count(where);
        }

        public TEntity GetEntityById(object id)
        {
            return this._dbSet.Find(id);//根据主键查找实体
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> where = null)
        {
            return _dbSet.Where(where).First();
        }

        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> where = null)
        {
            IEnumerable<TEntity> query = null;
            if (where == null)
            {
                query = _dbSet;

            }
            else
            {
                query = _dbSet.Where(where);
            }
            return query;
        }

        public IEnumerable<TEntity> GetPageList(Expression<Func<TEntity, bool>> where, string order = "", int pageIndex = 1, int pageSize = 10)
        {
            IEnumerable<TEntity> query = GetList(where);
            if (order == "")
            {
                order = "Id Asc";
            }
            query = query.OrderBy(order);
            return query.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        public void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(TEntity entity, params string[] excludeFields)
        {
            DbEntityEntry entry = this._db.Entry(entity);
            entry.State = EntityState.Modified;
            foreach (var item in excludeFields)
            {
                entry.Property(item).IsModified = false;
            }
        }
    }
}
