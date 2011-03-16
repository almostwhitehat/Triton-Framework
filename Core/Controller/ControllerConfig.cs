using System;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace Triton.Controller
{

	#region History

	// History:
	//   9/15/09 - SD - Added LogTrace and ManagerTrace properties to StatesConfig for
	//					controlling logging of state transitions to a log file and the
	//					TransitionSessionManager.  Requires "traceLog" and "traceManager"
	//					settings in the app's config file controllerSettings/states section.
	//   2/18/11 - SD - Added DefaultEventName property to StatesConfig for identifying a
	//					default event if no transition is found for the "real" event.

	#endregion

	/// <summary>
	/// The custom configuration section of web.config for the "controllerSettings/content" section.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ControllerConfigSection : ConfigurationSection
	{
		[ConfigurationProperty("contentProviders")]
		public ContentProviderElementCollection ContentProviders
		{
			get {
				return (ContentProviderElementCollection)this["contentProviders"];
			}
		}
	}


	/// <summary>
	/// A ConfigurationElement for the elements in controllerSettings/contentProviders.
	/// </summary>
	public class ContentProviderElement : ConfigurationElement
	{
		public ContentProviderElement() {}


		public ContentProviderElement(
			string name,
			string type,
			string publisher)
		{
			this["name"] = name;
			this["type"] = type;
			this["publisher"] = publisher;
		}


		[ConfigurationProperty("name", IsRequired = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
		public string Name
		{
			get {
				return (string) this["name"];
			}
			//set
			//{
			//    this["name"] = value;
			//}
		}


		[ConfigurationProperty("type")]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
		public string Type
		{
			get {
				return (string) this["type"];
			}
		}


		[ConfigurationProperty("publisher")]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
		public string Publisher
		{
			get {
				return (string) this["publisher"];
			}
		}
	}


	/// <summary>
	/// The collection of <b>ContentProviderElement</b>s.
	/// </summary>
	public class ContentProviderElementCollection : ConfigurationElementCollection
	{
		public ContentProviderElementCollection()
		{
			ContentProviderElement contentProvider = (ContentProviderElement) this.CreateNewElement();

			this.Add(contentProvider);
		}


		public override ConfigurationElementCollectionType CollectionType
		{
			get {
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}


		public new string AddElementName
		{
			get {
				return base.AddElementName;
			}
			set {
				base.AddElementName = value;
			}
		}


		public new string ClearElementName
		{
			get {
				return base.ClearElementName;
			}
			set {
				base.ClearElementName = value;
			}
		}


		public new string RemoveElementName
		{
			get {
				return base.RemoveElementName;
			}
		}


		public new int Count
		{
			get {
				return base.Count;
			}
		}


		public ContentProviderElement this[int index]
		{
			get {
				return (ContentProviderElement) BaseGet(index);
			}
			set {
				if (BaseGet(index) != null) {
					BaseRemoveAt(index);
				}

				BaseAdd(index, value);
			}
		}


		public new ContentProviderElement this[string Name]
		{
			get {
				return (ContentProviderElement) BaseGet(Name);
			}
		}


		protected override ConfigurationElement CreateNewElement()
		{
			return new ContentProviderElement();
		}


		protected override ConfigurationElement CreateNewElement(
			string elementName)
		{
			return new ContentProviderElement(elementName, null, null);
		}


		protected override object GetElementKey(
			ConfigurationElement element)
		{
			return ((ContentProviderElement)element).Name;
		}


		public int IndexOf(
			ContentProviderElement contentProvider)
		{
			return BaseIndexOf(contentProvider);
		}


		public void Add(
			ContentProviderElement contentProvider)
		{
			this.BaseAdd(contentProvider);
		}


		protected override void BaseAdd(
			ConfigurationElement element)
		{
			BaseAdd(element, false);
		}


		public void Remove(
			ContentProviderElement contentProvider)
		{
			if (BaseIndexOf(contentProvider) >= 0) {
				BaseRemove(contentProvider.Name);
			}
		}


		public void RemoveAt(
			int index)
		{
			BaseRemoveAt(index);
		}


		public void Remove(
			string name)
		{
			BaseRemove(name);
		}


		public void Clear()
		{
			BaseClear();
		}
	}


	public class StatesConfigHandler : IConfigurationSectionHandler
	{
		#region IConfigurationSectionHandler Members

		public object Create(
			object parent,
			object configContext,
			XmlNode section)
		{
			StatesConfig config = new StatesConfig();

					//  get the Type for the states DAO
			XmlNode node = section.SelectSingleNode("add[@key=\"dao\"]");
			if (node != null) {
				config.DaoType = node.Attributes["value"].Value;
			}

					//  get the connection for the states DAO
			node = section.SelectSingleNode("add[@key=\"connection\"]");
			if (node != null) {
				config.DaoConnectionName = node.Attributes["value"].Value;
			}

					//  get the default event name
			node = section.SelectSingleNode("add[@key=\"defaultEventName\"]");
			if (node != null) {
				config.DefaultEventName = node.Attributes["value"].Value;
			}

					//  get the setting for tracing transitions in TransitionSessionManager
			node = section.SelectSingleNode("add[@key=\"traceManager\"]");
			if (node != null) {
				config.ManagerTrace = (node.Attributes["value"].Value.ToLower() == "on");
			}

					//  get the setting for tracing transitions in a log file
			node = section.SelectSingleNode("add[@key=\"traceLog\"]");
			if (node != null) {
				config.LogTrace = (node.Attributes["value"].Value.ToLower() == "on");
			}

					//  get the mappings for the type from config to the class Type
			node = section.SelectSingleNode("factoryMappings");
			if (node != null) {
				Hashtable mappings = new Hashtable();
				foreach (XmlNode mapping in node.ChildNodes) {
					if (XmlNodeType.Element == mapping.NodeType) {
						try {
							mappings[mapping.Attributes["name"].Value] = mapping.Attributes["type"].Value;
						} catch (Exception e) {}
					}
				}

				config.StateTypes = mappings;
			}

			return config;
		}

		#endregion


		#region Nested type: StatesConfig

		public class StatesConfig
		{
			protected string daoConnectionName;
			protected string daoType;
			protected string defaultEventName			= null;
			protected Hashtable typeMappings;
			protected bool traceTransitionsInManager	= false;
			protected bool traceTransitionsInLog		= false;


			public string DaoType
			{
				get {
					return this.daoType;
				}
				internal set {
					this.daoType = value;
				}
			}


			public string DaoConnectionName
			{
				get {
					return this.daoConnectionName;
				}
				internal set {
					this.daoConnectionName = value;
				}
			}


			public string DefaultEventName
			{
				get {
					return this.defaultEventName;
				}
				internal set {
					this.defaultEventName = value;
				}
			}


			public Hashtable StateTypes
			{
				get {
					return this.typeMappings;
				}
				internal set {
					this.typeMappings = value;
				}
			}


			public bool LogTrace
			{
				get {
					return this.traceTransitionsInLog;
				}
				internal set {
					this.traceTransitionsInLog = value;
				}
			}


			public bool ManagerTrace
			{
				get {
					return this.traceTransitionsInManager;
				}
				internal set {
					this.traceTransitionsInManager = value;
				}
			}
		}

		#endregion
	}
}