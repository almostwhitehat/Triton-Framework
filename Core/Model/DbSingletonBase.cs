using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Xml;
using Common.Logging;
using Triton.Model.Dao;
using Triton.Utilities.Configuration;
using Triton.Utilities.Reflection;

namespace Triton.Model
{

	#region History

	// History:
	// 06/05/2009	KP	Changed the logging to Common.Logging.
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	// 10/19/2009	GV	Changed the Init to use ReflectionUtilities to de-serialize the object.

	#endregion

	/// <summary>
	/// Summary description for DbSingletonBase.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class DbSingletonBase
	{
		public ArrayList Init(
			XmlConfiguration config)
		{
			ArrayList objList = new ArrayList();
			
			#region Removing code thats not used anymore
			/*
			MemberInfo[] attrList;
			string attrValue;
			Type aType;
			PropertyInfo aProperty;
			Object propertyValue = null;
			MethodInfo aMethod;
			*/
			#endregion

			string className = config.GetValue("//object");
			string tableName = config.GetValue("//table");
			XmlNodeList columns = config.SelectNodes("//field");

			// Build string array
			string[] fields = new string[columns.Count];
			int i = 0;

			foreach (XmlNode node in columns) {
				fields.SetValue(node.Attributes["name"].Value, i);
				i++;
			}

			// Get data
			IGenericDao dao = DaoFactory.GetGenericDao();
			DataSet ds = dao.GetData(tableName, fields);
			DataTable dt = ds.Tables[tableName];

			// Loop through the data table to build the object list
			for (i = 0; i < dt.Rows.Count; i++) {
				DataRow dr = dt.Rows[i];

				Object anObj = Activator.CreateInstance(Type.GetType(className));

				NameValueCollection attributes = new NameValueCollection();

				foreach (XmlNode node in columns) {
					attributes.Add(node.Attributes["object_attr"].Value, dr[node.Attributes["name"].Value].ToString());
				}

				ReflectionUtilities.Deserialize(anObj, attributes);

				#region Duplicate code from ReflectionUtilities
				/*
				aType = anObj.GetType();

				
				// Loop through the fields in the config file to set properties for the object
				foreach (XmlNode node in columns) {
					attrList = aType.FindMembers(MemberTypes.Property,
					                             BindingFlags.Instance | BindingFlags.Public,
					                             this.FindMembersCanWriteDelegate,
					                             //Type.FilterNameIgnoreCase,
					                             node.Attributes["object_attr"].Value);
					attrValue = dr[node.Attributes["name"].Value].ToString();
					propertyValue = null;




					
					
					// TODO: Add error tracking code here
					if (attrList.Length == 1) {
						aProperty = (PropertyInfo) attrList[0];

						if (aProperty.PropertyType.IsEnum) {
							// Use enumeration values defined in dbMapping if available.
							NameValueCollection enumConfig =
								(NameValueCollection) ConfigurationSettings.GetConfig("dbMappings/" + aProperty.PropertyType.Name);

							if (enumConfig != null && enumConfig[attrValue] != null) {
								attrValue = enumConfig[attrValue];
							}

							aMethod = aProperty.PropertyType.BaseType.GetMethod(
								"Parse",
								new Type[3]{
								           	Type.GetType("System.Type"),
								           	"".GetType(),
								           	true.GetType()
								           });

							if ((aMethod != null) && (aMethod.IsStatic)) {
								propertyValue = aMethod.Invoke(
									null,
									new object[3]{
									             	aProperty.PropertyType,
									             	attrValue,
									             	true
									             });
							}
						} else if (Type.GetTypeCode(aProperty.PropertyType) == Type.GetTypeCode("".GetType())) {
							propertyValue = attrValue;
						} else {
							aMethod = aProperty.PropertyType.GetMethod("Parse", new Type[1]{"".GetType()});

							if ((aMethod != null) && (aMethod.IsStatic)) {
								try {
									propertyValue = aMethod.Invoke(
										null,
										new object[1]{attrValue});
								} catch (Exception e) {
// TODO: Logger name should not be hardcoded here!
									LogManager.GetCurrentClassLogger().Error(
										errorMessage => errorMessage("DbSingletonBase.Init: PropertyType = '{0}', attrValue = '{1}'",
									                                 aProperty.PropertyType,
									                                 attrValue),
																	 e);
								}
							}
						}

						try {
							if (propertyValue != null) {
								aMethod = aProperty.GetSetMethod();
								aMethod.Invoke(anObj, new[]{propertyValue});
							}
						} catch (MissingMethodException e) {
							// TODO: Handle MissingMethodException
							LogManager.GetCurrentClassLogger().Error(
										errorMessage => errorMessage("Could not get the property value from field type on the object. Property Name: {0}, Property Type: {1} ",
																	 aProperty.PropertyType,
																	 attrValue),
																	 e);
						}
					}
					

				}*/
				#endregion

				objList.Add(anObj);
			}
			dt.Dispose();
			ds.Dispose();

			return objList;
		}

		#region Removing code thats not used anymore
		/*
		/// <summary>
		/// A <b>MemberFilter</b> delegate used by FindMembers to filter by
		/// member name and writable properties.
		/// </summary>
		/// <param name="memberInfo">The <b>MemberInfo</b> to filter.</param>
		/// <param name="searchObj">The name of the member to filter for.</param>
		/// <returns><b>True</b> the member currently being inspected matches the 
		///			filter criteria, <b>false</b> otherwise</returns>
		private bool FindMembersCanWriteDelegate(
			MemberInfo memberInfo,
			Object searchObj)
		{
			// Compare the name of the member function with the filter criteria.
			return ((memberInfo.Name.ToLower() == searchObj.ToString().ToLower())
			        && ((PropertyInfo) memberInfo).CanWrite);
		}
		*/
		#endregion
	}
}