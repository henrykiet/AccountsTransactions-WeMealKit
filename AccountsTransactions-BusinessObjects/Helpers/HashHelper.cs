using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Helpers
{
	public static class HashHelper
	{
		public static String GetSignature256(String text, String key)
		{
			ASCIIEncoding encoding = new ASCIIEncoding();

			Byte[] textBytes = encoding.GetBytes(text);
			Byte[] keyBytes = encoding.GetBytes(key);

			Byte[] hashBytes;

			using (HMACSHA256 hash = new HMACSHA256(keyBytes))
				hashBytes = hash.ComputeHash(textBytes);

			return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
		}
		public static string ComputeHmacSha512(string key, string inputData)
		{
			var hash = new StringBuilder();
			var keyBytes = Encoding.UTF8.GetBytes(key);
			var inputBytes = Encoding.UTF8.GetBytes(inputData);

			byte[] hashBytes;

			using (var hmac = new HMACSHA512(keyBytes))
			{
				hashBytes = hmac.ComputeHash(inputBytes);
			}

			foreach (var theByte in hashBytes)
			{
				hash.Append(theByte.ToString("x2"));
			}

			return hash.ToString();
		}
	}
}
