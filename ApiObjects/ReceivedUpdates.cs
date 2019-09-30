using System.Collections.Generic;
using System.Text.Json;

namespace TelegramBotFramework.ApiObjects
{

	public class ReceivedUpdates
	{
		public static ReceivedUpdates FromJson(string json)
		{
			return JsonSerializer.Deserialize<ReceivedUpdates>(json);
		}

		public List<Update> result { get; set; }
	}
}