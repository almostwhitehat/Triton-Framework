using System.Runtime.InteropServices;

namespace Triton.Utilities
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Summary description for MvcTimer.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MvcTimer
	{
		private readonly long frequency;
		private double accumulatedTime;
		private long endTime;
		private bool isRunning;
		private long startTime;


		/// <summary>
		/// Construct a new <c>MvcTimer</c>.
		/// </summary>
		public MvcTimer()
		{
			//  get the frequency used by the performance timer
			QueryPerformanceFrequency(ref this.frequency);
		}


		/// <summary>
		/// Get the run time of the timer.  If the timer is still
		/// running (Stop() has not been called), it returns the 
		/// time since the timer was started.  If the timer has
		/// been stopped, it return the time is was running (from
		/// Start() to Stop()).
		/// </summary>
		public double Time
		{
			get
			{
				if (this.isRunning) {
					long nowTime = 0;
					QueryPerformanceCounter(ref nowTime);
					return (nowTime - this.startTime)*1.0/this.frequency;
				} else {
					return this.accumulatedTime;
				}
			}
		}


		[DllImport("kernel32.dll")]
		private static extern short QueryPerformanceCounter(ref long x);


		[DllImport("kernel32.dll")]
		private static extern short QueryPerformanceFrequency(ref long x);


		/// <summary>
		/// Starts the timer running.
		/// </summary>
		public void Start()
		{
			//  get the current performance timer time
			QueryPerformanceCounter(ref this.startTime);
			//  set the flag so we know the timer is running
			this.isRunning = true;
		}


		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			//  only stop the timer if it was previously started
			if (this.isRunning) {
				//  get the current performance timer time
				QueryPerformanceCounter(ref this.endTime);
				//  add the time since the timer was started
				//  to the accumulated time
				this.accumulatedTime += (this.endTime - this.startTime)*1.0/this.frequency;
				//  set the flag so we know the timer is no 
				//  longer running
				this.isRunning = false;
			}
		}


		/// <summary>
		/// Resets the timer so it's accumulated time is reset.
		/// </summary>
		public void Reset()
		{
			this.accumulatedTime = 0.0;
		}
	}
}