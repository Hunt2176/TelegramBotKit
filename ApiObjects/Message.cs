using System.Collections.Generic;
using TelegramBot.ApiObjects;

namespace TelegramBot.ApiObjects
{
	public class Message
	{
		public long message_id { get; set; }
		public int date { get; set; }
		public Chat chat { get; set; }
		public User? from { get; set; }
		public string? text { get; set; }
		public List<MessageEntity>? entities { get; set; }

		public string? GetCommand()
		{
			if (entities == null) return null;
			foreach (var entity in entities)
			{
				if (entity.type == "bot_command")
				{
					return text?.Substring(entity.offset, entity.length);
				}
			}

			return null;
		}

		
		public override string ToString()
		{
			return $"From: {from?.first_name}: {text}";
		}
	}
}