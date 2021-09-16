using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int PriorityId { get; set; }
        [Required]
        public string Subject { get; set; }
        public int? AssignerId { get; set; }
        public DateTime? AssignedDateTime { get; set; }
        public int? AssigneeId { get; set; }
        public DateTime? Deadline { get; set; }
        [Required]
        public int StatusId { get; set; }
        [Required]
        public DateTime StatusUpdated { get; set; }
        public string Description { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Project Project { get; set; }
        public virtual Account Assigner { get; set; }
        public virtual Account Assignee { get; set; }
        public virtual Status Status { get; set; }

        public Ticket(string subject)
        {
            Subject = subject;
            StatusId = 1;
            StatusUpdated = DateTime.Now;
        }
        public Ticket()
        {
        }
    }


    public class Status
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string StatusName { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    public class Priority
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PriorityName { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
