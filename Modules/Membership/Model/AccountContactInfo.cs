using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GFW.Membership.model
{
	public class AccountContactInfo
	{
		public virtual int Id { get; set; }

		public virtual int Version { get; set; }

		public virtual string Info { get; set; }
	}
}
