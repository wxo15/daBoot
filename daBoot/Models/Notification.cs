using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string NotificationString { get; set; }
        public string PostAction { get; set; }
        public string GetAction { get; set; }
        [Required]
        public bool IsRead { get; set; }
        [Required]
        public int UserId { get; set; }
        public virtual Account NotificationUser { get; set; }

        public Notification(string notificationstr, int userid)
        {
            NotificationString = notificationstr;
            UserId = userid;
            IsRead = false;
        }

        public Notification()
        {
        }

    }
}
