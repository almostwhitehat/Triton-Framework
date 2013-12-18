using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Data;
using Triton.Support;
using Triton.Utilities.Configuration;
using Triton.Utilities.Reflection;
using Triton.Model.Dao;

namespace Triton.Model {

#region History

// History:
//  11/30/10 - SD -	Added Exists property.

#endregion

// TODO: timed automatic refresh, sorting?, background refresh
/// <summary>
/// Parameterized base class for Singletons populated from a DAO.
/// The rules for populating the Singleton are defined in a configuration file identified
/// in the appSettings section of the application config file, with the name "singletonsConfigPath",
/// for example: &lt;add key="singletonsConfigPath" value="singletons.config" /&gt;<p/>
/// The format of the singletons configuration file is:
/// <pre>
/// <singletons>
/// 	<singleton name="[SingletonName]">
/// 		<class name="[SingletonClass], [SingletonAssembly]" table="[TableName]">
/// 			<id name="[IdPropertyName]" column="[ColumnName]" type="[IdPropertyType]" />
/// 			<property name="[PropertyName]" column="[ColumnName]" />
/// 			<property ... />
/// 			...
/// 		</class>
/// 	</singleton>
/// 	...
/// </singletons>
/// </pre>
/// </summary>
/// <remarks>
/// Example usage: public class OrderTypes : IdCodeSingletonBase&lt;OrderTypes, OrderType, Guid&gt;
/// </remarks>
/// <typeparam name="TSingleton">The type of the Singleton itself.</typeparam>
/// <typeparam name="TElement">The type of the object the Singleton contains.</typeparam>
/// <typeparam name="TKey">The type of the key the Singleton is indexed on.</typeparam>
public class SingletonBase<TSingleton, TElement, TKey> : IEnumerable
			where TSingleton : SingletonBase<TSingleton, TElement, TKey>
			where TElement : new()
{
	/// <summary>
	/// Defines the delegate to get the key for an element.
	/// This is the key the singleton is indexed on. It must
	/// be unique.
	/// </summary>
	/// <param name="element"></param>
	public delegate TKey GetKeyDelegate(TElement element);

	private static TSingleton	instance = default(TSingleton);
	private static object		syncLock	= new object();

	protected Dictionary<TKey, TElement> items;


	/// <summary>
	/// Gets or sets the delegate method used to get the key
	/// for an element in the singleton. If no GetKey delegate
	/// is provided, the default field identified in the configuration
	/// file is used.
	/// </summary>
	public GetKeyDelegate GetKey
	{
		get;
		set;
	}


	public TElement this[TKey key]
	{
		get {
			return items.ContainsKey(key) ? items[key] : default(TElement);
		}
	}


	public static TSingleton Instance
	{
		get {
			if (instance == null) {
				lock(syncLock) {
					if (instance == null) {
						TSingleton temp = (TSingleton)Activator.CreateInstance(typeof(TSingleton));
						temp.Init();
						instance = temp;
					}
				}
			}

			return instance;
		}
	}


	/// <summary>
	/// Indicates whether or not the singleton instance exists.
	/// </summary>
	public static bool Exists
	{
		get {
			return (instance != null);
		}
	}


	public virtual TElement[] Items
	{
		get {
			return items.Values.ToArray();
		}
	}


	public virtual bool ContainsKey(
		TKey key)
	{
		return (items != null) && items.ContainsKey(key);
	}


	public virtual bool ContainsValue(
		TElement value)
	{
		return (items != null) && items.ContainsValue(value);
	}


	public virtual void Init()
	{
				// get the configuration settings for this singleton
		XmlConfiguration singletonConfig = GetConfiguration();

				// get the data to populate the singleton
		DataTable data = GetData(singletonConfig);

				// populate the singleton from the data
		Fill(singletonConfig, data);
	}


	protected virtual void Fill(
		XmlConfiguration config,
		DataTable data)
	{
		XmlNode classNode = config.SelectSingleNode("//class");
		XmlNode idNode = classNode.SelectSingleNode("//id");
		XmlNodeList propertyNodes = classNode.SelectNodes("//property");
		string idPropertyName = idNode.Attributes["name"].Value;

		items = new Dictionary<TKey, TElement>();

		for (int i = 0; i < data.Rows.Count; i++) {
			DataRow dr = data.Rows[i];

			TElement obj = new TElement();
			//object obj = Activator.CreateInstance(Type.GetType(className));

			NameValueCollection attributes = new NameValueCollection();

			foreach (XmlNode node in propertyNodes) {
						//  skip it if the value is null
				if (dr[node.Attributes["column"].Value] != DBNull.Value) {
					attributes.Add(node.Attributes["name"].Value, dr[node.Attributes["column"].Value].ToString());
				}
			}
					// add the ID (key) field to the attributes collection
			attributes.Add(idPropertyName, dr[idNode.Attributes["column"].Value].ToString());

			ReflectionUtilities.Deserialize(obj, attributes);

			TKey key;
			if (GetKey != null) {
				key = GetKey(obj);
			} else {
				key = (TKey)ReflectionUtilities.GetPropertyValue(obj, idPropertyName);
			}
			items.Add(key, obj);
		}
	}


	protected virtual DataTable GetData(
		XmlConfiguration config)
	{
		XmlNode classNode = config.SelectSingleNode("//class");
		XmlNode idNode = classNode.SelectSingleNode("//id");
		XmlNodeList propertyNodes = classNode.SelectNodes("//property");
		string tableName = classNode.Attributes["table"].Value;

				// create array to hold the DB field names (properties + ID)
		string[] fields = new string[propertyNodes.Count + 1];

				// fill the field name array from the property nodes
		int i = 0;
		foreach (XmlNode node in propertyNodes) {
			fields.SetValue(node.Attributes["column"].Value, i++);
		}
		fields.SetValue(idNode.Attributes["column"].Value, i);

				// get the data
		IGenericDao dao = DaoFactory.GetGenericDao();
		DataSet ds = dao.GetData(tableName, fields);
		DataTable dt = ds.Tables[tableName];

		return dt;
	}


	protected virtual XmlConfiguration GetConfiguration()
	{
		XmlConfiguration config = new XmlConfiguration();
		string rootPath = AppInfo.BasePath;
		string configPath = ConfigurationManager.AppSettings["singletonsConfigPath"];

		config.Load(rootPath + configPath);
		XmlNode singletonNode = config.SelectSingleNode(string.Format(
				"//singleton[@name='{0}']", typeof(TSingleton).Name));

		if (singletonNode == null) {
			throw new ConfigurationErrorsException(string.Format(
					"Could not find configuration node for singleton '{0}'.", typeof(TSingleton).Name));
		}

		config.LoadXml(singletonNode.OuterXml);

		return config;
	}


	public static void Refresh()
	{
		instance = null;
	}


	#region IEnumerable Members

	public IEnumerator GetEnumerator()
	{
		return items.Values.GetEnumerator();
	}

	#endregion
}
}
