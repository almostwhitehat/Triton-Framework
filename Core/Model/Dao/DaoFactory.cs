using System;
using System.Collections.Specialized;
using System.Configuration;
using Common.Logging;
using Triton.Utilities.Reflection;

namespace Triton.Model.Dao {

	#region History

	// History:
	//   6/5/2009	KP	Changed the logging to Common.Logging.
	//  8/13/2009	GV	Changed the function names to be Dao at the end
	//  9/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	//  12/5/2013	SD	Added GetDao overload to take both an instance (class) name
	//					and a connection name, as defined in config.

	#endregion

	/// <summary>
	/// <b>DaoFactory</b> is the factory class for obtaining a Dao.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class DaoFactory
	{
		private const string CONFIG_DAO_SETTINGS		= "daoSettings";
		private const string CONFIG_DEFAULT_DAO_CLASS	= "class";

		/// <summary>
		/// Returns an instance of a <c>IGenericDao</c> with the default db connection.
		/// The concrete class instantiated is defined in the web.config file.
		/// </summary>
		/// <returns>A <c>IGenericDao</c>.</returns>
		public static IGenericDao GetGenericDao()
		{
			return GetGenericDao("*");
		}


		/// <summary>
		/// Returns an instance of a <c>IGenericDao</c> for the specified connection type.
		/// The concrete class instantiated is defined in the web.config file.
		/// </summary>
		/// <returns>A <c>IGenericDao</c>.</returns>
		public static IGenericDao GetGenericDao(
			string connType)
		{
			NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection("daoSettings/IGenericDao");
			string daoClass = config["class"];

			IGenericDao dao = (IGenericDao) Activator.CreateInstance(Type.GetType(daoClass), new object[]{connType});

			return dao;
		}


		/// <summary>
		/// Returns an instance of a <c>IGenericDao</c> for the specified connection type.
		/// The concrete class instantiated is defined in the web.config file.
		/// </summary>
		/// <remarks>
		/// The config setting for the parameterized IGenericDao should look something like
		/// this: "Triton.Model.Dao.MsGenericDao`1[[T]],Model".
		/// </remarks>
		/// <typeparam name="T">The type of the generic Dao to create.</typeparam>
		/// <returns>A <c>IGenericDao&lt;T&gt;</c> for the given type.</returns>
		public static IGenericDao<T> GetGenericDao<T>() where T : new()
		{
			return GetGenericDao<T>("*");
		}


		/// <summary>
		/// Returns an instance of a <c>IGenericDao</c> for the specified connection type.
		/// The concrete class instantiated is defined in the web.config file.
		/// </summary>
		/// <remarks>
		/// The config setting for the parameterized IGenericDao should look something like
		/// this: "Triton.Model.Dao.MsGenericDao`1[[T]],Model".
		/// </remarks>
		/// <typeparam name="T">The type of the generic Dao to create.</typeparam>
		/// <param name="connType">The connection type to use for the Dao.</param>
		/// <returns>A <c>IGenericDao&lt;T&gt;</c> for the given type.</returns>
		public static IGenericDao<T> GetGenericDao<T>(
			string connType) where T : new()
		{
			NameValueCollection config = (NameValueCollection) ConfigurationManager.GetSection("daoSettings/IGenericDaoGen");
			string daoClass = config["class"];

			//  replace the placeholder in the name from the config file with the
			//  the (assembly-qualified) name of the type given
			daoClass = daoClass.Replace("[[T]]", "[[" + typeof (T).AssemblyQualifiedName + "]]");

			//  get the type for the Dao to instantiate
			Type type = Type.GetType(daoClass);

			//  instantiate the Dao
			IGenericDao<T> dao = (IGenericDao<T>) Activator.CreateInstance(type, new object[]{connType});

			return dao;
		}


		/// <summary>
		/// Returns an instance of a Dao that is mapped in the config file.
		/// </summary>
		/// <param name="name">Key name of the Dao to return</param>
		/// <remarks>
		/// The name of the node in the daoSettings section of the config file must be the
		/// class name of "T".
		/// </remarks>
		/// <typeparam name="T">Type of the Dao to return.</typeparam>
		/// <returns>An instance of a Dao that as defined in the config file.</returns>
		public static T GetDao<T>(
			string name)
		{
			T dao = default(T);
			Type tType = typeof(T);
			string configSection = CONFIG_DAO_SETTINGS + "/" + tType.Name;

			NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection(configSection);
			string daoClass = config[name];

			try {
				dao = (T)Activator.CreateInstance(Type.GetType(daoClass), new object[]{});
			} catch (Exception ex) {
				//log the error
				LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("Error occured when trying to create {0}.", tType.FullName), ex);
				throw;
			}

			return dao;
		}


		public static T GetDao<T>(
			string className,
			string connectionName)
		{

			if (string.IsNullOrEmpty(className)) {
				className = CONFIG_DEFAULT_DAO_CLASS;
			}

			T dao = GetDao<T>(className);

			if ((dao != null) && ReflectionUtilities.HasProperty(dao, "ConnectionType", typeof(string))) {
				try {
					ReflectionUtilities.SetPropertyValue(dao, "ConnectionType", connectionName);
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
							msg => msg("Error occured setting {0} on {1}.", "ConnectionType", dao.GetType().FullName), e);
					throw;
				}
			}

			return dao;
		}


		/// <summary>
		/// Returns an instance of a Dao that is mapped in the config file. Defaults to "class" as the key name.
		/// </summary>
		/// <remarks>
		/// The name of the node in the daoSettings section of the config file must be the
		/// class name of "T".
		/// </remarks>
		/// <typeparam name="T">Type of the Dao to return.</typeparam>
		/// <returns>An instance of a Dao that as defined in the config file.</returns>
		public static T GetDao<T>()
		{
			return GetDao<T>(CONFIG_DEFAULT_DAO_CLASS);
		}
	}
}