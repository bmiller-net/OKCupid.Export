using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OKCupid.Web.Models.Entitites
{
    public class Message
    {
        public int MessageId { get; set; }
        public int MessageThreadId { get; set; }
        public string OKCupidMessageId { get; set; }
        public string Text { get; set; }
        public DateTime DateSent { get; set; }
        public bool Received { get; set; }

        [ForeignKey("MessageThreadId")]
        public virtual MessageThread MessageThread { get; set; }
    }
}