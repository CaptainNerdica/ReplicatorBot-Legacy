using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
				this.GuildId = server.GuildId;
				this.TargetUserId = server.TargetUserId;
				this.DisabledUserIds = server.DisabledUserIds;
				this.DisabledSubstrings = server.DisabledSubstrings;
				this.AvailableMessages = server.AvailableMessages;
				this.LastMessageReceived = server.LastMessageReceived;
				this.AutoUpdateMessages = server.AutoUpdateMessages;
				this.Enabled = server.Enabled;
				this.GuildTotalMessages = server.GuildTotalMessages;
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
		internal readonly static string pathformat = "GuildStorage/{0}.dat";
		public DiscordServerInfo(SocketGuild guild)
		{
			if (!TryRetrieve(guild.Id))
			{
				this.GuildId = guild.Id;
				this.TargetUserId = null;
				this.DisabledUserIds = new HashSet<ulong>();
				this.DisabledSubstrings = new HashSet<string>();
				this.AvailableMessages = new Dictionary<ulong, string>();
				this.AutoUpdateMessages = true;
			}
		}

		private bool TryRetrieve(ulong guildId)
		{
			var path = string.Format(pathformat, guildId);
			if (File.Exists(path))
			{
				try
				{
					BinaryFormatter formatter = new BinaryFormatter();
					DiscordServerInfoFields serverInfo;
					using (var file = File.OpenRead(path)) serverInfo = (DiscordServerInfoFields)formatter.Deserialize(file);
					this.GuildId = serverInfo.GuildId;
					this.TargetUserId = serverInfo.TargetUserId;
					this.DisabledSubstrings = serverInfo.DisabledSubstrings;
					this.DisabledUserIds = serverInfo.DisabledUserIds;
					this.AutoUpdateMessages = serverInfo.AutoUpdateMessages;
					this.Enabled = serverInfo.Enabled;
					this.LastMessageReceived = serverInfo.LastMessageReceived;
					this.AvailableMessages = serverInfo.AvailableMessages;
					this.GuildTotalMessages = serverInfo.GuildTotalMessages;
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
			this.GuildTotalMessages = 0;
			this.LastMessageReceived = new DateTime();
			this.Locked = true;
			this.AvailableMessages = new Dictionary<ulong, string>();
		}

		private void WriteToDisk()
		{
			var path = string.Format(pathformat, this.GuildId);
			if (!new DirectoryInfo("GuildStorage").Exists) 
				Directory.CreateDirectory("GuildStorage");
			var formatter = new BinaryFormatter();
			DiscordServerInfoFields fields = new DiscordServerInfoFields(this);
			var fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
				fileInfo.Delete();
			using (var file = File.Open(path, FileMode.OpenOrCreate))
			{
				formatter.Serialize(file, fields);
			}
		}
		public void Flush() => WriteToDisk();

		public void Clear()
		{
			var path = string.Format(pathformat, this.GuildId);
			var f = new FileInfo(path);
			if (f.Exists)
				f.Delete();
			this.TargetUserId = null;
			this.DisabledUserIds = null;
			this.DisabledSubstrings = null;
			this.GuildTotalMessages = 0;
			this.LastMessageReceived = new DateTime();
			this.AvailableMessages = null;
		}
	}
}
