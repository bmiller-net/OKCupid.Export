using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using OKCupid.Web.Models.Entitites;

namespace OKCupid.Web.Models.Context
{
    public class OKCupidContext : DbContext
    {
        public IDbSet<Message> Messages { get; set; }
        public IDbSet<MessageThread> MessageThreads { get; set; }
        public IDbSet<User> Users { get; set; }
    }
}