using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReplicatorBot
{
	[Group("replicator disable strings")]
	[Alias("replicator disable strings", "replicator disabled strings")]
	[Summary("Modifies strings that messages cannot contain")]
	[RequireUserPermission(GuildPermission.Administrator)]
	public class DisableSubstringsModule : ModuleBase<SocketCommandContext>
	{
		[Command("add")]
		[Summary("Adds strings to the list of disabled strings")]
		[Priority(2)]
		public async Task AddDisabledAsync(params string[] substrings)
		{
			if (substrings.Length != 0)
			{
				foreach (string sub in substrings)
					Program.Replicant.AvailableServers[Context.Guild.Id].DisabledSubstrings.Add(sub);
				await ReplyAsync("Successfully disabled string(s)");
			}
			else
			{
				await ReplyAsync("You must include at least one string");
			}
		}
		[Command("remove")]
		[Summary("Removes strings from the list of disabled strings")]
		[Priority(2)]
		public async Task RemoveDisabledAsync(params string[] substrings)
		{
			if (substrings.Length != 0)
			{
				foreach (string sub in substrings)
					Program.Replicant.AvailableServers[Context.Guild.Id].DisabledSubstrings.Remove(sub);
				await ReplyAsync("Successfully re-enabled strings");
			}
			else
			{
				await ReplyAsync("You must include at least one string");
			}
		}
		[Command("clear")]
		[Summary("Removes all strings from the list of disabled strings")]
		[Priority(2)]
		public async Task ClearDisabledAsync()
		{
			Program.Replicant.AvailableServers[Context.Guild.Id].DisabledSubstrings.Clear();
			await ReplyAsync("Successfully cleared all disabled strings");
		}
		[Command("list")]
		[Summary("Lists the disabled strings")]
		[Priority(2)]
		public async Task ListDisabled()
		{
			HashSet<string> substrings = Program.Replicant.AvailableServers[Context.Guild.Id].DisabledSubstrings;
			if (substrings.Count != 0)
			{
				string builder = "Disabled Strings:\n";
				foreach (string sub in substrings)
					builder += $"\t\"{sub}\"\n";
				await ReplyAsync(builder);
			}
			else
				await ReplyAsync("There are no disabled strings");
		}
	}
}
