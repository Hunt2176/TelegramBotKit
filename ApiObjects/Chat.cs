namespace TelegramBot.ApiObjects
{
	public class Chat
	{
		public long id { get; set; }
		public string type { get; set; }
		
		public string? title { get; set; }
		public string? username { get; set; }
		public string? description { get; set; }
	}
}