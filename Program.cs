using System;
using ActionLooper;
using System.Net.Http;

namespace TelegramBot
{
	class Program
	{
		public static void Main(string[] args)
		{
			Looper.GetLooper();
		}
	}

	class ClientManager
	{
		private HttpClient client = new HttpClient();
	}
}