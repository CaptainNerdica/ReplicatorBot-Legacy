using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;

namespace ReplicatorBot
{
	public class DiscordServerInfo
	{
		[Serializable]
		public class DiscordServerInfoFields
		{
			public ulong GuildId;
			public ulong? TargetUserId;
			public HashSet<ulong> DisabledUserIds;
			public HashSet<string> DisabledSubstrings;
			public Dictionary<ulong, string> AvailableMessages;
			public DateTime LastMessageReceived;
			public bool AutoUpdateMessages;
			public bool Enabled;
			public int GuildTotalMessages;

			public DiscordServerInfoFields()
			{
			}
			public DiscordServerInfoFields(DiscordServerInfo server)
			{
				GuildId = server.GuildId;
				TargetUserId = server.TargetUserId;
				DisabledUserIds = server.DisabledUserIds;
				DisabledSubstrings = server.DisabledSubstrings;
				AvailableMessages = server.AvailableMessages;
				LastMessageReceived = server.LastMessageReceived;
				AutoUpdateMessages = server.AutoUpdateMessages;
				Enabled = server.Enabled;
				GuildTotalMessages = server.GuildTotalMessages;
			}
		}
		public ulong GuildId { get; private set; }
		public ulong? TargetUserId { get; set; }
		public HashSet<ulong> DisabledUserIds { get; private set; }
		public HashSet<string> DisabledSubstrings { get; private set; }
		public Dictionary<ulong, string> AvailableMessages { get; set; }
		public bool Enabled;
		public bool Locked;
		public DateTime LastMessageReceived;
		public bool AutoUpdateMessages;
		public int TargetTotalMessages { get => AvailableMessages.Count; }
		public int GuildTotalMessages;
		public double Proability { get => (double)TargetTotalMessages / GuildTotalMessages; }
		private static readonly string directory = ConfigurationManager.AppSettings["ServerStorageDirectory"];
		private static readonly string fileFormat = ConfigurationManager.AppSettings["ServerStorageFormat"];
		private static readonly string pathFormat = $"{directory}/{fileFormat}";
		public DiscordServerInfo(SocketGuild guild)
		{
			if (!TryRetrieve(guild.Id))
			{
				GuildId = guild.Id;
				TargetUserId = null;
				DisabledUserIds = new HashSet<ulong>();
				DisabledSubstrings = new HashSet<string>();
				AvailableMessages = new Dictionary<ulong, string>();
				AutoUpdateMessages = true;
			}
		}

		private bool TryRetrieve(ulong guildId)
		{
			var path = string.Format(pathFormat, guildId);
			if (File.Exists(path))
			{
				try
				{
					BinaryFormatter formatter = new BinaryFormatter();
					DiscordServerInfoFields serverInfo;
					using (var file = File.OpenRead(path)) serverInfo = (DiscordServerInfoFields)formatter.Deserialize(file);
					GuildId = serverInfo.GuildId;
					TargetUserId = serverInfo.TargetUserId;
					DisabledSubstrings = serverInfo.DisabledSubstrings;
					DisabledUserIds = serverInfo.DisabledUserIds;
					AutoUpdateMessages = serverInfo.AutoUpdateMessages;
					Enabled = serverInfo.Enabled;
					LastMessageReceived = serverInfo.LastMessageReceived;
					AvailableMessages = serverInfo.AvailableMessages;
					GuildTotalMessages = serverInfo.GuildTotalMessages;
					return true;
				}
				catch
				{
					return false;
				}
			}
			else
				return false;
		}

		public void ClearMessages()
		{
			GuildTotalMessages = 0;
			LastMessageReceived = new DateTime();
			Locked = true;
			AvailableMessages = new Dictionary<ulong, string>();
		}

		private void WriteToDisk()
		{
			string path = string.Format(pathFormat, GuildId);
			if (!new DirectoryInfo(directory).Exists)
				Directory.CreateDirectory(directory);
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
				fileInfo.Delete();
			using FileStream file = File.Open(path, FileMode.OpenOrCreate);
			new BinaryFormatter().Serialize(file, this);
		}
		public void Flush() => WriteToDisk();

		public void Clear()
		{
			var path = string.Format(pathFormat, GuildId);
			var f = new FileInfo(path);
			if (f.Exists)
				f.Delete();
			TargetUserId = null;
			DisabledUserIds = null;
			DisabledSubstrings = null;
			GuildTotalMessages = 0;
			LastMessageReceived = new DateTime();
			AvailableMessages = null;
		}
	}
}
