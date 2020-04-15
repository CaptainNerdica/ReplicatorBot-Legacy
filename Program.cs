using System;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace ReplicatorBot
{
	public class Program
	{
		public static ReplicatorBot Replicant;
		public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
		public async Task MainAsync()
		{
			Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelInterrupt);
			Replicant = new ReplicatorBot();
			while (true)
			{
				await Task.Delay(new TimeSpan(0,5,0));
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
