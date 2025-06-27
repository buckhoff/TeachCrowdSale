using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class NotificationSettings
    {
        public EmailConfig Email { get; set; } = new();
        public DiscordConfig Discord { get; set; } = new();
        public TelegramConfig Telegram { get; set; } = new();
    }

    public class EmailConfig
    {
        public bool Enabled { get; set; } = false;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
    }

    public class DiscordConfig
    {
        public bool Enabled { get; set; } = false;
        public string WebhookUrl { get; set; } = string.Empty;
    }

    public class TelegramConfig
    {
        public bool Enabled { get; set; } = false;
        public string BotToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
    }
}
