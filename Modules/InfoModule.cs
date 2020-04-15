using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;

namespace ReplicatorBot
{
	[Group("replicator")]
	public class ReplicatorCommands : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		public Task PingAsync() => ReplyAsync("pong!");

		[Group("info")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public class BotInfoModule : ModuleBase<SocketCommandContext>
		{
			[Command]
			[RequireUserPermission(GuildPermission.Administrator)]
			public async Task GetInfo()
			{
				DiscordServerInfo info = Program.Replicant.AvailableServers[Context.Guild.Id];
				string builder = $"Replicated User: {(info.TargetUserId == null ? "None" : Context.Client.GetUser((ulong)info.TargetUserId).Username)}\n";
				builder += $"Enabled: {info.Enabled}\n";
				builder += $"Probability: {info.Proability}\n";
				builder += $"Server messages: {info.GuildTotalMessages}\n";
				builder += $"Target messages: {info.TargetTotalMessages}\n";
				builder += $"Last message time: {info.LastMessageReceived} UTC\n";
				await ReplyAsync(builder);
			}
		}
	}
}

