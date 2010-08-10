using System;
using System.Web.UI.HtmlControls;
using Common.Logging;
using Triton.Support.Error;

namespace Triton.Web.View.Controls
{
	/// <summary> Creates markup for errors if they exists. wraps each error in a tag supplied by the control definition
	/// </summary>
	public class Errors : XmlBasedControl
	{
		/// <summary>
		/// The minimum error level to display.
		/// </summary>
		private Error.ErrorType minLevel = Error.ErrorType.NOERROR;

		/// <summary>
		/// The maximum error level to display.
		/// </summary>
		private Error.ErrorType maxLevel = Error.ErrorType.FATAL;

		/// <summary>
		/// Initializes a new instance of the <see cref="Controls.Errors"/> class.  Creates a default error control with standard initializers
		/// </summary>
		public Errors()
		{
			IncludeFieldName = bool.TrueString;
			IncludeId = bool.TrueString;
		}

		/// <summary>
		/// Gets or sets ErrorList.
		/// </summary>
		public ErrorList ErrorList { get; set; }

		/// <summary>
		/// Gets or sets the name of the CSS class used for rendering
		/// the errors.
		/// </summary>
		public string Class { get; set; }

		/// <summary>
		/// Gets or sets the delimiter to be used to separate error messages.
		/// </summary>
		public string ErrorTag { get; set; }

		/// <summary>
		/// Gets or sets the flag indicating whether or not the error ID is
		/// to be included with the error message.
		/// </summary>
		public string IncludeId { get; set; }

		/// <summary>
		/// Gets or sets the flag indicating whether or not the field name is
		/// to be appended to the error message.
		/// </summary>
		public string IncludeFieldName { get; set; }

		/// <summary>
		/// Gets or sets the minimum error level to be displayed.
		/// </summary>
		public string MinLevel
		{
			get
			{
				return minLevel.ToString();
			}

			set
			{
				try {
					minLevel = (Error.ErrorType)Enum.Parse(minLevel.GetType(), value.ToUpper());
				}
				catch (Exception) {
					LogManager.GetCurrentClassLogger().ErrorFormat("Cannot parse '{0}' into a ErrorType Enumeration.", value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum error level to be displayed.
		/// </summary>
		public string MaxLevel
		{
			get
			{
				return maxLevel.ToString();
			}

			set
			{
				try {
					maxLevel = (Error.ErrorType)Enum.Parse(maxLevel.GetType(), value.ToUpper());
				}
				catch (Exception) {
					LogManager.GetCurrentClassLogger().ErrorFormat("Cannot parse '{0}' into a ErrorType Enumeration.", value);
				}
			}
		}

		protected override void CreateChildControls()
		{
			// get the list of errors from the parent page
			if (this.ErrorList == null) {
				this.ErrorList = Page.Errors;
			}

			// don't do anything if there are no errors
			if (this.ErrorList != null) {
				// build the output for each error message
				foreach (Error err in this.ErrorList) {
					if ((err.Type >= minLevel) && (err.Type <= maxLevel)) {
						// build the message to be displayed
						string msg = err.Message;

						// include the error ID only if directed to do so
						if (bool.Parse(IncludeId)) {
							msg += " [" + err.Id + "]";
						}

						if (bool.Parse(IncludeFieldName)) {
							if (!string.IsNullOrEmpty(err.FormField)) {
								msg += " - Field: " + err.FormField;
							}
						}

						// if a css class name was specified, include the message
						// in <span> tags with the css class
						HtmlGenericControl surroundingTag = new HtmlGenericControl(ErrorTag)
								{
									InnerHtml = msg
								};
						if (Class != null) {
							if (surroundingTag.Attributes["class"] != null) {
								surroundingTag.Attributes.Add("class", Class);
							}
						}

						// append the delimiter
						Controls.Add(surroundingTag);
					}
				}
			}
		}
	}
}
