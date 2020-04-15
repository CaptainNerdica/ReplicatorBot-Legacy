using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord;
using System.Linq;
using System.Threading.Tasks;
namespace ReplicatorBot
{
	[Group("replicator")]
	[Summary("Modifies the target player")]
	[RequireUserPermission(GuildPermission.Administrator)]
	public class ReplicatorModule : ModuleBase<SocketCommandContext>
	{
		[Command("set")]
		[Summary("Sets the user to try to replicate")]
		public async Task SetUserAsync(IUser targetUser)
		{
			DiscordServerInfo guild = Program.Replicant.AvailableServers[Context.Guild.Id];
			guild.TargetUserId = targetUser.Id;
			guild.Enabled = false;
			guild.ClearMessages();
			await ReplyAsync($"Set the target replicated player to {Context.Guild.GetUser(targetUser.Id).Nickname}");
			await ReplyAsync("Will require a full re-read of server messages to activate (!replicator readall)");
		}
		[Command("clear")]
		[Summary("Clears the replicated user")]
		public async Task ClearUserAsync()
		{
			DiscordServerInfo guild = Program.Replicant.AvailableServers[Context.Guild.Id];
			guild.TargetUserId = null;
			guild.Locked = true;
			guild.ClearMessages();
			await ReplyAsync("Cleared the replicated user");
		}
		[Command("readall")]
		[Summary("Reads all messages in the server and builds the list of message Replicator can send")]
		public async Task ReadAllAsync(int max = 1000000)
		{
			await Task.Run(() => Program.Replicant.ReadAllMessages(Context.Guild, max, Context.Channel));
		}
		[Command("flush")]
		[Summary("Writes all data to disk")]
		public async Task FlushAync()
		{
			await Task.Run(Program.Replicant.AvailableServers[Context.Guild.Id].Flush);
			await ReplyAsync("Flushed all data to disk");
		}
		[Command("update")]
		[Summary("Attempts to update the last read messages on the server")]
		public async Task UpdateAsync()
		{
			DiscordServerInfo info = Program.Replicant.AvailableServers[Context.Guild.Id];
			Program.Replicant.ReadSinceTimestamp(Context.Guild, info.LastMessageReceived, Context.Channel);
		}
		[Command("nickname")]
		public async Task NicknameAsync([Remainder] string nickname)
		{
			await Context.Client.GetGuild(Context.Guild.Id).GetUser(Context.Client.CurrentUser.Id).ModifyAsync(u => { u.Nickname = nickname; });
		}
	}
}
