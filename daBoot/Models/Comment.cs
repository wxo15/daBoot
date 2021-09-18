using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TicketId { get; set; }
        [Required]
        public string CommentString { get; set; }
        [Required]
        public DateTime CommentTimeStamp { get; set; }
        public virtual Account CommentUser { get; set; }
        public virtual Ticket Ticket { get; set; }

        public Comment(int userid, int ticketid, string commentstr)
        {
            UserId = userid;
            TicketId = ticketid;
            CommentString = commentstr;
            CommentTimeStamp = DateTime.Now;
        }
        
        public Comment()
        {
        }
    }
}
