using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Implement
{
	public class BaseRepository<T> : IBaseRepository<T> where T : class
	{
		private readonly AccountsTransactionsContext _context;
		internal DbSet<T> _dbSet { get; set; }
		public BaseRepository(AccountsTransactionsContext context)
		{
			this._context = context;
			this._dbSet = this._context.Set<T>();
		}
		public void Create(T entity)
		{
			throw new NotImplementedException();
		}
		public void Update(T entity)
		{
			throw new NotImplementedException();
		}
		public void Delete(string id)
		{
			throw new NotImplementedException();
		}
		public virtual Task<List<T>> GetAllAsync()
		{
			return this._dbSet.ToListAsync();
		}
		public virtual async Task<T?> GetAsync(string id)
		{
			try
			{
				Guid guidId;
				if (!Guid.TryParse(id, out guidId))
				{
					return null;
				}
				var entity = await _dbSet.FindAsync(guidId);
				return entity;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error occurred in GetAsync: {ex}");
				return null;
			}
		}
		public virtual async Task<bool> CreateAsync(T entity)
		{
			try
			{
				await _dbSet.AddAsync(entity);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error occurred in CreateAsync: {ex}");
				return false;
			}
		}
		public virtual Task<bool> UpdateAsync(T entity)
		{
			try
			{
				_dbSet.Update(entity);
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error occurred in UpdateAsync: {ex}");
				return Task.FromResult(false);
			}
		}
		public virtual async Task<bool> DeleteAsync(string id)
		{
			try
			{
				var entity = await _dbSet.FindAsync(id);
				if (entity != null)
				{
					_context.Set<T>().Remove(entity);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error occurred in DeleteAsync: {ex}");
				return false;
			}
		}

		public void DetachEntity(T entity)
		{
			throw new NotImplementedException();
		}
	}
}
