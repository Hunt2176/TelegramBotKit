using System;

namespace TelegramBot.ApiObjects
{
	public class Update
	{
		public int update_id { get; set; }
        
		public Message message { get; set; }
		
	}
}