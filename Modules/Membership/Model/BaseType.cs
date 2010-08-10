namespace GFW.Membership.Domain
{
	public class BaseType
	{
		public virtual int Id { get; set; }

		public virtual string Name { get; set; }

		public virtual string Description { get; set; }

		public virtual int Version { get; set; }


		public override bool Equals(object obj)
		{
			bool retValue = false;
			// Check for null values and compare run-time types.
			if (obj != null || GetType() == obj.GetType())
			{
				BaseType type = (BaseType) obj;
				if (this.Name == type.Name && this.Id == type.Id)
				{
					retValue = true;
				}
			}
			return retValue;
		}
	}
}