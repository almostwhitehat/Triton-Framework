using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Triton.Support.Collections;

namespace Triton.Support.Session
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISessionState : ICollection, IEnumerable
	{
		/// <summary>
		/// Gets or sets the value of an item in the cache with the given key.
		/// </summary>
		object this[string key]
		{
			get; 
			set; 
		}
	}

}
