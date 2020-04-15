using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ReplicatorBot
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public CommandHandler(DiscordSocketClient client, CommandService commands)
		{
			_commands = commands;
			_client = client;
		}

		public async Task InstallCommandsAsync()
		{
			_client.MessageReceived += HandleCommandsAsync;
			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
		}

		private async Task HandleCommandsAsync(SocketMessage messageParam)
		{
			if (!(messageParam is SocketUserMessage message)) return;

			int argPos = 0;

			if (!(message.HasCharPrefix('!', ref argPos)) || message.Author.IsBot) return;

			var context = new SocketCommandContext(_client, message);

			await _commands.ExecuteAsync(context: context, argPos: argPos, services: null);
		}
	}
}
