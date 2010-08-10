using Triton.Controller.Request;
using Triton.Membership.Model;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Web.View.Controls;

namespace Triton.Membership.View.Web.Controls
{
	public class AttributeTypeSelect : BaseSelect
	{
		private const string SELECT_NAME = ParameterNames.AttributeType.CODE;


		public AttributeTypeSelect()
		{
			this.UseCode = false;
			base.SelectedValue = 0;
			base.Name = SELECT_NAME;
			
		}


		public string AttributeTypeItemName { get; set; }

		public bool UseCode { get; set; }


		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			MvcRequest request = base.Page.Request;

			if ((request.Items[(this.AttributeTypeItemName ?? ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE)] != null &&
			     (request.Items[(this.AttributeTypeItemName ?? ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE)] is SearchResult<AttributeType>))) {
				SearchResult<AttributeType> results = (SearchResult<AttributeType>)request.Items[(this.AttributeTypeItemName ?? ItemNames.AttributeType.DEFAULT_SEARCH_RESULT_ATTRIBUTE_TYPE)];

				foreach (AttributeType attributeType in results.Items) {
					string text;
					bool selected = false;

					text = this.UseCode ? attributeType.Code : attributeType.Name;

					if ((SelectedValue != 0 && SelectedValue == attributeType.Id)
					    || (SelectedText != null && SelectedText == text)
					    || (request[(Name ?? SELECT_NAME)] != null && request[(Name ?? SELECT_NAME)] == attributeType.Id.ToString())) {
						selected = true;
					}

					Controls.Add(CreateNewOption(attributeType.Code, text, attributeType.Description, selected));
				}
			}
		}
	}
}