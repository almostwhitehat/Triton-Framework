using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Location.Model;
using Triton.Location.Model.Dao;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Location.Logic
{

	#region History

	// History:
	// 08/13/2009	GV	Changed the inherited class to be Action after the rename from BizAction
	//					and change the DaoFactory.GetDao method name
	// 09/09/2009	GV	Changed the Action interface to inherit from IAction

	#endregion

	public class GetStatesAction : IAction
	{
		private const string ADDRESS_STATES = "address_states";

		public string RequestItemName { get; set; }

		#region BizAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = "error";
			try {
				//for now this just returns all the states
				List<State> states = new List<State>(DaoFactory.GetDao<IStateDao>().Get(new State{
				                                                                                 	IsTerritory = false
				                                                                                 }));

                SearchResult<State> results = new SearchResult<State>(states.ToArray());

				switch (results.Items.Length) {
					case 0:
						retEvent = "zero";
						break;
					case 1:
						retEvent = "one";
						break;
					default:
						retEvent = "multiple";
						break;
				}

				context.Request.Items[(this.RequestItemName ?? ADDRESS_STATES)] = results;
			} catch (Exception ex) {
				LogManager.GetLogger(typeof (GetStatesAction)).Error(
							errorMessage => errorMessage("GetPropertyAction: Execute", ex));
			}

			return retEvent;
		}

		#endregion
	}
}