using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Triton.Web.View.Controls
{
	public class BaseSelect : BaseControl
	{
		private const string SELECT_NAME = "select";


		public BaseSelect()
			: base(HtmlTextWriterTag.Select)
		{
			this.Name = SELECT_NAME;
			this.FirstItemText = "";
			this.FirstItemValue = "0";
		}


		public string FirstItemText { get; set; }

		public string FirstItemValue { get; set; }

		public string Name { get; set; }

		public int SelectedValue { get; set; }

		public string SelectedText { get; set; }


		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Name);
		}


		protected Control CreateNewOption(
			string value,
			string text,
			bool selected)
		{
			return this.CreateNewOption(value, text, null, selected);
		}


		protected Control CreateNewOption(
			string value,
			string text,
			string title,
			bool selected)
		{
			HtmlGenericControl option = new HtmlGenericControl("option");
			option.Attributes.Add("value", value);
			option.InnerText = text;

			if (!string.IsNullOrEmpty(title)) {
				option.Attributes.Add("title", title);
			}

			if (selected) {
				option.Attributes.Add("selected", "selected");
			}
			return option;
		}


		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Add(this.CreateNewOption(this.FirstItemValue, this.FirstItemText, false));
		}
	}
}