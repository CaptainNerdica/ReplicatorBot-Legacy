using System;
using System.Threading.Tasks;

namespace ReplicatorBot
{
	public class Program
	{
		public static ReplicatorBot Replicant;
		public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();
		public async Task MainAsync()
		{
			Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelInterrupt);
			Replicant = new ReplicatorBot();
			while (true)
			{
				await Task.Delay(new TimeSpan(0, 5, 0));
				Replicant.FlushAll();
			}
		}

		public void CancelInterrupt(object sender, ConsoleCancelEventArgs args)
		{
			Replicant.BotStop();
			Environment.Exit(0);
		}
	}
}
