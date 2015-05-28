using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OKCupid.Web.Models.Entitites
{
    public class MessageThread
    {
        public MessageThread()
        {
            //User = new OKCupid.Web.Models.Entitites.User();
            Messages = new List<Message>();
        }

        public int MessageThreadId { get; set; }
        public string OKCupidMessageThreadId { get; set; }
        public string Username { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }
    }
}