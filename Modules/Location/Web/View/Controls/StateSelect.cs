using Triton.Controller.Request;
using Triton.Location.Model;
using Triton.Model;
using Triton.Web.View.Controls;

namespace Triton.Location.Web.View.Controls
{
	public class StateSelect : BaseSelect
	{
		private const string ADDRESS_STATES = "address_states";
		private const string SELECT_NAME = "address_state";


		public StateSelect()
		{
			this.UseShortName = false;
			this.UseCode = false;
			base.SelectedValue = 0;
			base.Name = SELECT_NAME;
		}


		public string StatesRequestItemName { get; set; }

		public bool UseShortName { get; set; }

		public bool UseCode { get; set; }


		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			MvcRequest request = base.Page.Request;

			if ((request.Items[(this.StatesRequestItemName ?? ADDRESS_STATES)] != null)
			    && (request.Items[(this.StatesRequestItemName ?? ADDRESS_STATES)] is SearchResult<State>)) {
				SearchResult<State> results = (SearchResult<State>) request.Items[(this.StatesRequestItemName ?? ADDRESS_STATES)];

				foreach (State state in results.Items) {
					string text;
					bool selected = false;

					text = this.UseCode ? state.Code : (this.UseShortName ? state.ShortName : state.LongName);

					if ((SelectedValue != 0 && SelectedValue == state.Id)
					    || (SelectedText != null && SelectedText == text)
					    || (request[(Name ?? SELECT_NAME)] != null && request[(Name ?? SELECT_NAME)] == state.Id.ToString())) {
						selected = true;
					}

					Controls.Add(CreateNewOption(state.Id.ToString(), text, selected));
				}
			}
		}
	}
}