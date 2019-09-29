using System;

namespace TelegramBot.ApiObjects
{
	public class MessageEntity
	{
		public string type { get; set; }
		public string? url { get; set; }
		public int offset { get; set; }
		public int length { get; set; }
	}
}