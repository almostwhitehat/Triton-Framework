namespace Triton.Location.Model
{
	public class PersistedAddress : Address
	{
		public virtual long? Id { get; set; }

		public virtual long? Version { get; set; }
	}
}