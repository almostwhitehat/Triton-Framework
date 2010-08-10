namespace Triton.Location.Model
{
	public class State
	{
		public virtual string Code { get; set; }

		public virtual string ShortName { get; set; }

		public virtual string LongName { get; set; }

		public virtual int? Id { get; set; }

		public virtual bool IsTerritory { get; set; }

		public virtual int? Version { get; set; }

		public virtual Region Region { get; set; }
	}
}