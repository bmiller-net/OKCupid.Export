using System.ComponentModel.DataAnnotations.Schema;

namespace OKCupid.Web.Models.Entitites
{
    public class User
    {
        public int UserId { get; set; }
        public string OKCupidUsername { get; set; }
        public int OKCupidUserId { get; set; }
        //[InverseProperty("User")]
        //public virtual MessageThread MessageThread { get; set; }
    }
}