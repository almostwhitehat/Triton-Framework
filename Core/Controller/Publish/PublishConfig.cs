using System;
using System.Configuration;

namespace Triton.Controller.Publish {

#region History

// History:
//  3/21/2011	SD	Added support for new publisher rules.

#endregion

/// <summary>
/// Custom configuration information for publishing system.
/// </summary>
///	<author>Scott Dyke</author>
public class PublishConfigSection : ConfigurationSection
{
	public PublishConfigSection()
	{
		this["mode"] = "on";
	}


	public bool Publish
	{
		get {
			return ("on".Equals((string)this["mode"], StringComparison.CurrentCultureIgnoreCase));
		}
	}


	[ConfigurationProperty("mode")]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Mode
	{
		get {
			return (string)this["mode"];
		}
	}


	[ConfigurationProperty("publishers")]
	public PublisherElementCollection Publishers
	{
		get {
			return (PublisherElementCollection)this["publishers"];
		}
	}


	[ConfigurationProperty("settings")]
	public SettingsElementCollection Settings
	{
		get {
			return (SettingsElementCollection)this["settings"];
		}
	}
}


/// <summary>
/// A ConfigurationElement for the elements in triton/publishing/settings.
/// </summary>
public class SettingsElement : ConfigurationElement
{

	public SettingsElement()
	{
	}


	public SettingsElement(
		string name,
		string val)
	{
		this["name"] = name;
		this["value"] = val;
	}


	[ConfigurationProperty("name", IsRequired = true)]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Name
	{
		get {
			return (string) this["name"];
		}
	}


	[ConfigurationProperty("value", IsRequired = true)]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Value
	{
		get {
			return (string) this["value"];
		}
	}
}


/// <summary>
/// A ConfigurationElement for the elements in triton/publishing/publishers.
/// </summary>
public class PublisherElement : ConfigurationElement
{
	//	private string	name	= "";


	public PublisherElement() {}


	public PublisherElement(
		string name,
		string handler)
	{
		this["name"] = name;
		this["handler"] = handler;
	}


	[ConfigurationProperty("name", IsRequired = true)]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Name
	{
		get {
			return (string)this["name"];
		}
	}


	[ConfigurationProperty("handler")]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Handler
	{
		get {
			return (string)this["handler"];
		}
	}


	[ConfigurationProperty("rules")]
	public PublisherRuleCollection Rules
	{
		get {
			return (PublisherRuleCollection)this["rules"];
		}
	}
}


/// <summary>
/// The collection of <b>PublisherElement</b>s.
/// </summary>
public class PublisherElementCollection : ConfigurationElementCollection
{
	public PublisherElementCollection()
	{
		PublisherElement publisher = (PublisherElement)this.CreateNewElement();

		this.Add(publisher);
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


	public PublisherElement this[int index]
	{
		get {
			return (PublisherElement) BaseGet(index);
		}
		set {
			if (BaseGet(index) != null) {
				BaseRemoveAt(index);
			}

			BaseAdd(index, value);
		}
	}


	public new PublisherElement this[string Name]
	{
		get {
			return (PublisherElement)BaseGet(Name);
		}
	}


	protected override ConfigurationElement CreateNewElement()
	{
		return new PublisherElement();
	}


	protected override ConfigurationElement CreateNewElement(
		string elementName)
	{
		return new PublisherElement(elementName, null);
	}


	protected override object GetElementKey(
		ConfigurationElement element)
	{
		return ((PublisherElement)element).Name;
	}


	public int IndexOf(
		PublisherElement publisher)
	{
		return BaseIndexOf(publisher);
	}


	public void Add(
		PublisherElement publisher)
	{
		this.BaseAdd(publisher);
	}


	protected override void BaseAdd(
		ConfigurationElement element)
	{
		BaseAdd(element, false);
	}


	public void Remove(
		PublisherElement publisher)
	{
		if (BaseIndexOf(publisher) >= 0) {
			BaseRemove(publisher.Name);
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


/// <summary>
/// A ConfigurationElement for the elements in triton/publishing/publishers.
/// </summary>
public class PublisherRule : ConfigurationElement
{
	//	private string	name	= "";


	public PublisherRule()
	{
	}


	public PublisherRule(
		string name,
		string clas)
	{
		this["name"] = name;
		this["class"] = clas;
	}


	[ConfigurationProperty("name", IsRequired = true)]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Name
	{
		get {
			return (string)this["name"];
		}
	}


	[ConfigurationProperty("class", IsRequired = true)]
	[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
	public string Class
	{
		get {
			return (string)this["class"];
		}
	}
}


/// <summary>
/// The collection of <b>PublisherRule</b>s.
/// </summary>
public class PublisherRuleCollection : ConfigurationElementCollection
{
	public PublisherRuleCollection()
	{
		//PublisherRule rule = (PublisherRule)this.CreateNewElement();

		//this.Add(rule);
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


	public PublisherRule this[int index]
	{
		get {
			return (PublisherRule)BaseGet(index);
		}
		set {
			if (BaseGet(index) != null) {
				BaseRemoveAt(index);
			}

			BaseAdd(index, value);
		}
	}


	public new PublisherRule this[string Name]
	{
		get {
			return (PublisherRule)BaseGet(Name);
		}
	}


	protected override ConfigurationElement CreateNewElement()
	{
		return new PublisherRule();
	}


	protected override ConfigurationElement CreateNewElement(
		string elementName)
	{
		return new PublisherRule(elementName, null);
	}


	protected override object GetElementKey(
		ConfigurationElement element)
	{
		return ((PublisherRule)element).Name;
	}


	public int IndexOf(
		PublisherRule rule)
	{
		return BaseIndexOf(rule);
	}


	public void Add(
		PublisherRule rule)
	{
		this.BaseAdd(rule);
	}


	protected override void BaseAdd(
		ConfigurationElement element)
	{
		BaseAdd(element, false);
	}


	public void Remove(
		PublisherRule rule)
	{
		if (BaseIndexOf(rule) >= 0) {
			BaseRemove(rule.Name);
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


/// <summary>
/// The collection of <b>SettingsElement</b>s.
/// </summary>
public class SettingsElementCollection : ConfigurationElementCollection
{
	public SettingsElementCollection()
	{
		SettingsElement setting = (SettingsElement)this.CreateNewElement();

		this.Add(setting);
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


	public SettingsElement this[int index]
	{
		get {
			return (SettingsElement)BaseGet(index);
		}
		set {
			if (BaseGet(index) != null) {
				BaseRemoveAt(index);
			}

			BaseAdd(index, value);
		}
	}


	public new SettingsElement this[string Name]
	{
		get {
			return (SettingsElement)BaseGet(Name);
		}
	}


	protected override ConfigurationElement CreateNewElement()
	{
		return new SettingsElement();
	}


	protected override ConfigurationElement CreateNewElement(
		string elementName)
	{
		return new SettingsElement(elementName, null);
	}


	protected override object GetElementKey(
		ConfigurationElement element)
	{
		return ((SettingsElement)element).Name;
	}


	public int IndexOf(
		SettingsElement setting)
	{
		return BaseIndexOf(setting);
	}


	public void Add(
		SettingsElement setting)
	{
		this.BaseAdd(setting);
	}


	protected override void BaseAdd(
		ConfigurationElement element)
	{
		BaseAdd(element, false);
	}


	public void Remove(
		SettingsElement setting)
	{
		if (BaseIndexOf(setting) >= 0) {
			BaseRemove(setting.Name);
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
}