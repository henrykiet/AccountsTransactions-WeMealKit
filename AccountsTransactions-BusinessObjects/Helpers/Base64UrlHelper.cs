using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Helpers
{
	public static class Base64UrlHelper
	{
		public static string EncodeTokenToBase64(string token)
		{
			byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
			string base64Token = Convert.ToBase64String(tokenBytes);
			// Thay thế các ký tự '+' thành '-'
			base64Token = base64Token.Replace('+', '-');

			// Thay thế các ký tự '/' thành '_'
			base64Token = base64Token.Replace('/', '_');

			// Loại bỏ tất cả các ký tự '='
			base64Token = base64Token.Replace("=", "");
			return base64Token;
		}
		public static string DecodeTokenFromURL(string base64Token)
		{
			// Giải mã base64Token và chuyển thành token gốc
			base64Token = base64Token.Replace('-', '+').Replace('_', '/');
			int mod4 = base64Token.Length % 4;
			if (mod4 > 0)
			{
				base64Token += new string('=', 4 - mod4);
			}
			byte[] tokenBytes = Convert.FromBase64String(base64Token);
			string token = Encoding.UTF8.GetString(tokenBytes);
			return token;
		}
	}
}
