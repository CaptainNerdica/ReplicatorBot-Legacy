using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace ReplicatorBot
{
	[Group("replicator disable users")]
	[Alias("replicator disable users", "replicator disabled users")]
	[Summary("Modifies users that cannot be replied to or read from")]
	[RequireUserPermission(GuildPermission.Administrator)]
	public class DisableUsersModule : ModuleBase<SocketCommandContext>
	{
		[Command("add")]
		[Summary("Adds users to the list of disabled users")]
		public async Task AddDisabledAsync(params IUser[] users)
		{
			if (users.Length != 0)
			{
				foreach (IUser user in users)
					Program.Replicant.AvailableServers[Context.Guild.Id].DisabledUserIds.Add(user.Id);
				await ReplyAsync("Successfully disabled user(s)");
			}
			else
			{
				await ReplyAsync("You must include at least one user");
			}
		}
		[Command("remove")]
		[Summary("Removes users from the list of disabled users")]
		public async Task RemoveDisabledAsync(params IUser[] users)
		{
			if (users.Length != 0)
			{
				foreach (IUser user in users)
					Program.Replicant.AvailableServers[Context.Guild.Id].DisabledUserIds.Remove(user.Id);
				await ReplyAsync("Successfully re-eneabled user(s)");
			}
			else
			{
				await ReplyAsync("You must include at least one user");
			}
		}
		[Command("clear")]
		[Summary("Removes all users from the list of disabled users")]
		public async Task ClearDisabledAsync()
		{
			Program.Replicant.AvailableServers[Context.Guild.Id].DisabledUserIds.Clear();
			await ReplyAsync("Successfully cleared all disabled users");
		}
		[Command("list")]
		[Summary("Lists the disabled users")]
		public async Task ListDisabled()
		{
			HashSet<ulong> users = Program.Replicant.AvailableServers[Context.Guild.Id].DisabledUserIds;
			if (users.Count != 0)
			{
				string builder = "Disabled Users:\n";
				foreach (ulong user in users)
					builder += $"\t{Context.Client.GetUser(user).Username}\n";
				await ReplyAsync(builder);
			}
			else
				await ReplyAsync("There are no disabled users");
		}
	}
}
