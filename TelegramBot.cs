using System;
using System.Text.Encodings.Web;
using ActionLooper;
using TelegramBotFramework.ApiObjects;

namespace TelegramBotFramework
{
    class TelegramBot
    {
        private readonly Looper _botLooper = new Looper(2500);
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
        }

        public void Start()
        {
            Console.WriteLine("Bot Started");
            _botLooper.PostToCycleQueue("updates", GetUpdates);
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
}