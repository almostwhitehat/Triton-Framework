using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Triton.Controller;

namespace Triton.Configuration
{
	public class ITritonConfiguration
	{
		public string BasePath { get; protected set; }

		public INameTypeConfigurationElement DaoFactoryConfigurationElement { get; private set; }
	}

	public class TritonXmlConfiguration : ITritonConfiguration
	{
		private const string SECTION_NAME = "triton";

		private static TritonConfigurationSection section = ConfigurationManager.GetSection(SECTION_NAME) as TritonConfigurationSection;

		public string BasePath
		{
			get
			{
				//  make sure we have the proper config info
				if (section == null) {
					throw new ConfigurationErrorsException("Load of TritonConfiguration section failed. Need to declare 'triton' as the section name.");
				}

				return section.BasePath;
			}
		}
	}

	#region ConfigurationSections
	/// <summary>
	/// The custom configuration section of web.config for the "triton" section.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class TritonConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("basePath", IsRequired = false)]
		public string BasePath
		{
			get
			{
				string basePath = (string)this["basePath"];

				if (string.IsNullOrEmpty(basePath)) {
					basePath = AppDomain.CurrentDomain.BaseDirectory;
				}

				return basePath;
			}
		}

		//[ConfigurationProperty("daoFactory", IsRequired = true)]
		//public DaoFactoryConfigurationElement DaoFactory
		//{
		//    get
		//    {
		//        return (DaoFactoryConfigurationElement)this["daoFactory"];
		//    }
		//}

		[ConfigurationProperty("contentProviders", IsRequired = true)]
		public ContentProviderElementCollection ContentProviders
		{
			get
			{
				return (ContentProviderElementCollection)this["contentProviders"];
			}
		}

		//[ConfigurationProperty("states", IsRequired = true)]
		//public StatesConfigurationElement States
		//{
		//    get
		//    {
		//        return (StatesConfigurationElement)this["states"];
		//    }
		//}

		[ConfigurationProperty("actionNamespaces", IsRequired = true)]
		public NameValueConfigurationCollection ActionNamespaces
		{
			get
			{
				return (NameValueConfigurationCollection)this["actionNamespaces"];
			}
		}

		[ConfigurationProperty("commandNamespaces", IsRequired = true)]
		public NameValueConfigurationCollection CommandNamespaces
		{
			get
			{
				return (NameValueConfigurationCollection)this["commandNamespaces"];
			}
		}

		[ConfigurationProperty("formatterNamespaces", IsRequired = true)]
		public NameValueConfigurationCollection FormatterNamespaces
		{
			get
			{
				return (NameValueConfigurationCollection)this["formatterNamespaces"];
			}
		}
	}

	#endregion

	public class INameValueConfigurationElement
	{
		public string Name { get; protected set; }
		public string Value { get; protected set; }
	}

	public class INameTypeConfigurationElement
	{
		public string Name { get; protected set; }
		public string Type { get; protected set; }
	}
	public class NameValueXmlConfigurationElement : ConfigurationElement
	{
		public string Name { get; protected set; }
		public string Value { get; protected set; }
	}

	public class NameTypeXmlConfigurationElement : ConfigurationElement
	{
		public string Name { get; protected set; }
		public string Type { get; protected set; }
	}
}
