using System;
using ActionLooper;
using System.Net.Http;
using System.Text.Encodings.Web;
using TelegramBot.ApiObjects;

namespace TelegramBot
{
	class Program
	{
		public static void Main(string[] args)
		{
			TelegramBot bot = new TelegramBot();
			bot.Start();
			bot.Join();
		}
	}

	class TelegramBot
	{
		private readonly Looper _botLooper = new Looper(2000);
		private readonly ClientManager _clientManager;
		
		private string _botUrl = "https://api.telegram.org/bot";
		private long? _currentOffset = null;

		public TelegramBot(string token)
		{
			_botUrl += token;
			_clientManager = new ClientManager(this);
		}
		
		private void GetUpdates()
		{
			var url = _botUrl + "/getUpdates";
			if (_currentOffset != null) url += $"?offset={_currentOffset}";
			
			_clientManager.SetUrl(url);
			_clientManager.RunGet((result) =>
			{
				var updates = ReceivedUpdates.FromJson(result);
				updates.result.ForEach((update =>
				{
					_currentOffset = update.update_id + 1;
					UpdateReceived(update);
				}));
			});
			if (_botLooper.IsRunning())
			{
				_botLooper.Post(101, GetUpdates);
			}
		}
		
		public void Start()
		{
			_botLooper.Post(GetUpdates);
		}

		public void Join()
		{
			_botLooper.Join();
		}

		public void SendMessage(long chatId, string text)
		{
			_botLooper.Post(() =>
			{
				_clientManager.SetUrl($"{_botUrl}/sendMessage?chat_id={chatId}&text={UrlEncoder.Default.Encode(text)}");
				_clientManager.RunGet();
			});
		}

		public void ReplyToMessage(Message replyTo, string text)
		{
			_botLooper.Post(() =>
			{
				_clientManager.SetUrl($"{_botUrl}/sendMessage?chat_id={replyTo.chat.id}" +
				                      $"&text={UrlEncoder.Default.Encode(text)}&reply_to_message_id={replyTo.message_id}");
				
				_clientManager.RunGet();
			});
		}


		public virtual void UpdateReceived(Update? update)
		{
			Console.WriteLine(update?.message?.text);

			switch (update?.message?.GetCommand())
			{
				case "/messagetest":
					SendMessage(update.message.chat.id, "Message Test");
					break;
				
				case "/replytest":
					ReplyToMessage(update?.message, "Reply Test");
					break;
					
				default: return;
			}
		}


	}

	class ClientManager
	{
		private TelegramBot _owner;
		private HttpClient client = new HttpClient();
		private string _Url = "";
		
		private Looper _taskLooper = new Looper(3000);

		public void SetUrl(string url)
		{
			_Url = url;
		}
		
		public async void RunGet(Action<string>? onComplete = null)
		{
				try
				{
					var response = await client.GetStringAsync($"{_Url}");
					onComplete?.Invoke(response);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
		}


		public ClientManager(TelegramBot owner)
		{
			_owner = owner;
		}
	}
}