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
        public int? SubmitterId { get; set; }
        public DateTime? SubmittedDateTime { get; set; }
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
        public virtual Account Submitter { get; set; }
        public virtual Account Assigner { get; set; }
        public virtual Account Assignee { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }


        public Ticket(int projectid, string subject, int priorityid, string description, int? submitterid = -1)
        {
            ProjectId = projectid;
            Subject = subject;
            SubmitterId = submitterid;
            SubmittedDateTime = DateTime.Now;
            StatusId = 4;
            StatusUpdated = DateTime.Now;
            PriorityId = priorityid;
            Description = description;
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
