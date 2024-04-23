using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.ResponseObjects
{
	public class ResponseObject<T>
	{
		public int StatusCode { get; set; }
		public string Message { get; set; } = string.Empty;
		public List<T>? List { get; set; }
		public T? Data { get; set; }
	}
}
