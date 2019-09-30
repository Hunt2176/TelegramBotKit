using System;
using System.Net.Http;

namespace TelegramBotFramework
{
    class ClientManager
    {
        private TelegramBot _owner;
        private string _url = "";

        public void SetUrl(string url)
        {
            _url = url;
        }

        public async void RunGet(Action<string>? onComplete = null)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(_url);
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