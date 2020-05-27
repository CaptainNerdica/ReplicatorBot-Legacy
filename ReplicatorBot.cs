using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReplicatorBot
{
	public class ReplicatorBot
	{
		public Dictionary<ulong, DiscordServerInfo> AvailableServers;
		public CancellationTokenSource CancellationTokenSource;
		public CancellationToken CancellationToken;
		private readonly DiscordSocketClient _client;
		private CommandService _commandService;
		private CommandHandler _commandHandler;
		private readonly string _botToken;

		private static readonly string[] GlobalDisabledSubstrings = new string[] { "@everyone", "@here" };

		public ReplicatorBot()
		{
			var SocketConfig = new DiscordSocketConfig { LogLevel = LogSeverity.Info, DefaultRetryMode = RetryMode.AlwaysRetry, MessageCacheSize = 1000000 };
			AvailableServers = new Dictionary<ulong, DiscordServerInfo>();
			_client = new DiscordSocketClient(SocketConfig);
			_botToken = File.ReadAllText(ConfigurationManager.AppSettings["DiscordToken"]);
			CancellationTokenSource = new CancellationTokenSource();
			CancellationToken = CancellationTokenSource.Token;
			BotStart().GetAwaiter().GetResult();

		}
		public async Task BotStart()
		{
			await BotLog(new LogMessage(LogSeverity.Info, "Replicator", "Starting Bot"));
			_commandService = new CommandService();
			_commandHandler = new CommandHandler(_client, _commandService);
			await _commandHandler.InstallCommandsAsync();
			_client.Log += BotLog;
			_client.MessageReceived += MessageReceivedAsync;
			_client.GuildAvailable += AddAvailableGuild;
			_client.GuildUnavailable += RemoveUnavaiableGuild;
			_client.JoinedGuild += JoinedGuild;
			_client.LeftGuild += LeftGuild;
			await _client.LoginAsync(TokenType.Bot, _botToken);
			await _client.StartAsync();
		}

		public void BotStop()
		{
			BotLog(new LogMessage(LogSeverity.Info, "Replicator", "Stopping Bot"));
			FlushAll();
			_client.StopAsync();
			CancellationTokenSource.Cancel();
			CancellationTokenSource.Dispose();
		}

		public void FlushAll()
		{
			BotLog(new LogMessage(LogSeverity.Info, "Replicator", "Saving data..."));
			foreach (var server in AvailableServers.Values)
				server.Flush();
		}

		public Task BotLog(LogMessage msg)
		{
			Console.WriteLine(msg);
			return Task.CompletedTask;
		}
		private async Task MessageReceivedAsync(SocketMessage message)
		{
			SocketGuild messageGuild = (message.Author as IGuildUser).Guild as SocketGuild;
			AvailableServers.TryGetValue(messageGuild.Id, out DiscordServerInfo serverInfo);
			ChannelPermissions permissions = messageGuild.GetUser(_client.CurrentUser.Id).GetPermissions(message.Channel as IGuildChannel);
			Random rand = new Random();

			if (serverInfo.Enabled && !serverInfo.Locked && (string.IsNullOrEmpty(message.Content) || message.Content[0] != '!') && message.Author.Id != _client.CurrentUser.Id)
			{
				serverInfo.LastMessageReceived = DateTime.UtcNow;
				if (permissions.SendMessages && permissions.ViewChannel)
				{
					if (message.MentionedUsers.Any(u => u.Id == _client.CurrentUser.Id))
					{
						if (serverInfo.TargetTotalMessages != 0)
							await message.Channel.SendMessageAsync(serverInfo.AvailableMessages.Values.ElementAt(rand.Next(0, serverInfo.TargetTotalMessages)));
						else
							return;
					}
				}
				if (permissions.ViewChannel && permissions.ReadMessageHistory)
				{
					serverInfo.GuildTotalMessages += 1;
					if (TestValidMessage(message, serverInfo))
					{
						serverInfo.AvailableMessages.Add(message.Id, message.Content);
					}
				}
				if (permissions.ViewChannel && permissions.SendMessages)
				{
					if (rand.NextDouble() < serverInfo.Proability)
					{
						await message.Channel.SendMessageAsync(serverInfo.AvailableMessages.Values.ElementAt(rand.Next(0, serverInfo.TargetTotalMessages)));
					}
				}
			}
		}

		private bool TestValidMessage(IMessage message, DiscordServerInfo serverInfo)
		{
			if (message.Author.Id == serverInfo.TargetUserId)
			if (message.MentionedUserIds.Count == 0 && message.MentionedRoleIds.Count == 0 && message.MentionedChannelIds.Count == 0)
			if (!message.Content.Contains(GlobalDisabledSubstrings))
			if (message.Embeds.Count == 0)
			if (!message.Content.Contains(serverInfo.DisabledSubstrings))
				return true;
			return false;
		}

		public async void ReadAllMessages(SocketGuild guild, int maxChannelRead, ISocketMessageChannel replyChannel)
		{
			var serverInfo = AvailableServers[guild.Id];
			serverInfo.AvailableMessages = new Dictionary<ulong, string>();
			serverInfo.GuildTotalMessages = 0;
			serverInfo.Locked = true;

			foreach (var channel in guild.TextChannels)
			{
				await replyChannel.SendMessageAsync($"Attempting to read messages in channel {channel.Mention}");
				ChannelPermissions permissions = guild.GetUser(_client.CurrentUser.Id).GetPermissions(channel);
				if (permissions.ReadMessageHistory && permissions.ViewChannel)
				{
					var messageCollection = channel.GetMessagesAsync(maxChannelRead).Flatten();
					foreach (var message in await messageCollection.ToList())
					{
						if (string.IsNullOrEmpty(message.Content) || message.Content[0] != '!')
						{
							serverInfo.GuildTotalMessages += 1;
							if (TestValidMessage(message, serverInfo))
							{
								serverInfo.AvailableMessages.Add(message.Id, message.Content);
							}
						}
					}
				}
				else
					await replyChannel.SendMessageAsync($"No permission to read in channel {channel.Mention}");
			}
			serverInfo.Locked = false;
			serverInfo.Enabled = true;
			serverInfo.LastMessageReceived = DateTime.UtcNow;
			serverInfo.Flush();
			await replyChannel.SendMessageAsync("Read all messages on the server and Replicator is now active");
		}

		public async void ReadSinceTimestamp(SocketGuild guild, DateTime lastReceivedTime, ISocketMessageChannel replyChannel)
		{
			await BotLog(new LogMessage(LogSeverity.Info, "Replicator", "Reading new messages since last login"));
			DiscordServerInfo serverInfo = AvailableServers[guild.Id];
			serverInfo.Locked = true;
			foreach (var channel in guild.TextChannels)
			{
				await replyChannel.SendMessageAsync($"Reading new messages in channel {channel.Mention}");
				ChannelPermissions permissions = guild.GetUser(_client.CurrentUser.Id).GetPermissions(channel);
				if (permissions.ReadMessageHistory && permissions.ViewChannel)
				{
					var lastMessage = (await channel.GetMessagesAsync(1).Flatten().ToList())[0];
					DateTime currentTime = lastMessage.Timestamp.UtcDateTime;
					if (lastMessage != null)
					{
						while (currentTime >= lastReceivedTime)
						{
							var messageCollection = await channel.GetMessagesAsync(lastMessage, Direction.Before, 10).Flatten().ToList();
							foreach (var message in messageCollection)
							{
								if (string.IsNullOrEmpty(message.Content) || message.Content[0] != '!')
								{
									lastMessage = message;
									currentTime = lastMessage.Timestamp.UtcDateTime;
									if (currentTime >= lastReceivedTime)
										break;
									if (serverInfo.AvailableMessages.ContainsKey(lastMessage.Id))
										break;
									serverInfo.GuildTotalMessages += 1;
									if (TestValidMessage(message, serverInfo))
										serverInfo.AvailableMessages.Add(message.Id, message.Content);
								}
							}
						}
					}
				}
				else
					await replyChannel.SendMessageAsync($"No permission to read in channel {channel.Mention}");
			}
			await replyChannel.SendMessageAsync("Read all new messages");
			serverInfo.Locked = false;
			serverInfo.LastMessageReceived = DateTime.UtcNow;
			serverInfo.Flush();
		}

		private async Task AddAvailableGuild(SocketGuild guild)
		{
			AvailableServers.Add(guild.Id, new DiscordServerInfo(guild));
			await BotLog(new LogMessage(LogSeverity.Info, "Replicator", $"Server {guild.Name} became available"));
		}
		private async Task RemoveUnavaiableGuild(SocketGuild guild)
		{
			AvailableServers[guild.Id].Flush();
			AvailableServers.Remove(guild.Id);
			await BotLog(new LogMessage(LogSeverity.Warning, "Replicator", $"Server {guild.Name} became unavailable"));
		}
		private async Task JoinedGuild(SocketGuild guild)
		{
			AvailableServers.Add(guild.Id, new DiscordServerInfo(guild));
			await BotLog(new LogMessage(LogSeverity.Info, "Replicator", $"Successfully joined server {guild.Name}"));
		}
		private async Task LeftGuild(SocketGuild guild)
		{
			AvailableServers[guild.Id].Clear();
			AvailableServers.Remove(guild.Id);
			await BotLog(new LogMessage(LogSeverity.Info, "Replicator", $"Left server {guild.Name}"));
		}
	}
}
