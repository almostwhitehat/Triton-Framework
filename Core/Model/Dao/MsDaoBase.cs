using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Triton.Model.Dao
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// This is the base class of all DAOs accessing an MS SQL Server system.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class MsDaoBase : IDao
	{
		protected string connectionType;

		#region Dao Members

		public abstract string Name { get; }


		public string ConnectionType
		{
			get { return this.connectionType; }
		}


		/// <summary>
		/// Gets a database connection based on DB access information specified in web.config.
		/// </summary>
		/// <returns>A <c>IDbConnection</c> reference to a <c>SqlConnection</c> instance.</returns>
		/// <exception cref="ApplicationException">Throws the exception when connection string could not be found.</exception>
		public IDbConnection GetConnection()
		{
			// Get the connection string from web.config.
			// If no connnection type is specified, use the default connection (*) of this Dao.
			NameValueCollection daoConfig = (NameValueCollection) ConfigurationManager.GetSection("daoSettings/" + this.Name);

			string connName;
			try {
				//  see if there is a connection named based on the connection type, if not, try to get the default
				connName = daoConfig[this.ConnectionType] ?? daoConfig["*"];
			} catch {
				connName = this.ConnectionType;
			}

			string connString;
			if ((ConfigurationManager.ConnectionStrings != null) 
				&& (ConfigurationManager.ConnectionStrings[connName] != null)) {

				ConnectionStringSettings connSettings = ConfigurationManager.ConnectionStrings[connName];
				connString = connSettings.ConnectionString;

			} else {
				throw new ApplicationException("Could not retrieve the connection string by the name of "+ connName);
			}

			return new SqlConnection(connString);
		}

		#endregion
	}
}