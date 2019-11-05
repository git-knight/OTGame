
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TGame
{
    public partial class GameHub
    {
        struct ChatMessage
        {
            public DateTime Time { get; set; }
            public string Sender { get; set; }
            public string Message { get; set; }
        }

        static Queue<ChatMessage> LastMessages = new Queue<ChatMessage>(20);

        public async Task SendChatMessage(string msg)
        {
            var message = new ChatMessage {
                Time = DateTime.UtcNow,
                Sender = CurrentPlayer.Name,
                Message = msg
            };

            if (LastMessages.Count >= 20)
                LastMessages.Dequeue();

            LastMessages.Enqueue(message);

            await Clients.All.SendAsync("InvokeMethod", "Chat.ReceiveMessage", message);
        }
    }
}