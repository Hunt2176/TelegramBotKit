using System.IO;

namespace TelegramBotFramework
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var token = File.ReadAllText(Directory.GetCurrentDirectory() + "token.txt");
			
			TelegramBot bot = new TelegramBot(token);
			bot.Start();
			bot.Join();
		}
	}
}