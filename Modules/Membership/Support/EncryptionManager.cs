using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Triton.Membership.Support
{
	public class EncryptionManager
	{
		private byte[] key;


		/// <summary>
		/// Temporary Encryption Manager.
		/// </summary>
		public EncryptionManager()
		{
			this.key = Encoding.ASCII.GetBytes("grf7grq3");
		}


		public string EncryptField(string fieldValue)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(fieldValue)) {
				try {
					DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

					MemoryStream memoryStream = new MemoryStream();

					CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(this.key, this.key), CryptoStreamMode.Write);

					StreamWriter writer = new StreamWriter(cryptoStream);

					writer.Write(fieldValue);

					writer.Flush();

					cryptoStream.FlushFinalBlock();

					writer.Flush();

					result = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
				} catch (Exception ex) {
					/*LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("Could not encrypt value: {0}", fieldValue), ex);*/

					throw new ApplicationException("Could not encrypt value: " + fieldValue, ex);
				}
			}

			return result;
		}


		public string DecryptField(string fieldValue)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(fieldValue)) {
				try {
					DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

					MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(fieldValue));

					CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(this.key, this.key), CryptoStreamMode.Read);

					StreamReader reader = new StreamReader(cryptoStream);
					result = reader.ReadToEnd();
				} catch (Exception ex) {
					/*LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("Could not decrypt value: {0}", fieldValue), ex);*/

					throw new ApplicationException("Could not decrypt value: " + fieldValue, ex);
				}
			}

			return result;
		}
	}
}