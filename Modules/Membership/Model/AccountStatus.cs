﻿namespace Triton.Membership.Model
{
	public class AccountStatus
	{
		public virtual int? Id { get; set; }

		public virtual string Code { get; set; }

		public virtual string Description { get; set; }

		public virtual int? Version { get; set; }
	}
}