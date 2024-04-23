using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_DataAccess.Repository.Interface
{
	public interface IBaseRepository<T> where T : class
	{
		Task<List<T>> GetAllAsync();
		Task<T?> GetAsync(string id);
		public void Create(T entity);
		Task<bool> CreateAsync(T entity);
		public void Update(T entity);
		Task<bool> UpdateAsync(T entity);
		public void Delete(string id);
		Task<bool> DeleteAsync(string id);
		public void DetachEntity(T entity);
	}
}
