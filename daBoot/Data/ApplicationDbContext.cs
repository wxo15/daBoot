using daBoot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Item> Items{ get; set; }
        public DbSet<Account> Users { get; set; }
        public DbSet<Relation> Relations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Priority> Priority { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relation>()
                        .HasKey(k => new { k.UserId, k.TeamMemberId });

            modelBuilder.Entity<Relation>()
                        .HasOne(e => e.User)
                        .WithMany(e => e.OthersTeamMember)
                        .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Relation>()
                        .HasOne(e => e.TeamMember)
                        .WithMany(e => e.TeamMembers)
                        .HasForeignKey(e => e.TeamMemberId);

            modelBuilder.Entity<UserProject>()
                        .HasKey(k => new { k.UserId, k.ProjectId });

            modelBuilder.Entity<UserProject>()
                        .HasOne(e => e.User)
                        .WithMany(e => e.Projects)
                        .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<UserProject>()
                        .HasOne(e => e.Project)
                        .WithMany(e => e.TeamMembers)
                        .HasForeignKey(e => e.ProjectId);

            modelBuilder.Entity<UserProject>()
                        .HasOne(e => e.Role)
                        .WithMany(e => e.UserProject)
                        .HasForeignKey(e => e.RoleId);

            modelBuilder.Entity<Ticket>()
                        .HasOne(e => e.Priority)
                        .WithMany(e => e.Tickets)
                        .HasForeignKey(e => e.PriorityId);

            modelBuilder.Entity<Ticket>()
                        .HasOne(e => e.Project)
                        .WithMany(e => e.Tickets)
                        .HasForeignKey(e => e.ProjectId);

            modelBuilder.Entity<Ticket>()
                        .HasOne(e => e.Submitter)
                        .WithMany(e => e.SubmittedTickets)
                        .HasForeignKey(e => e.SubmitterId);

            modelBuilder.Entity<Ticket>()
                        .HasOne(e => e.Assignee)
                        .WithMany(e => e.AssignedTickets)
                        .HasForeignKey(e => e.AssigneeId);

            modelBuilder.Entity<Ticket>()
                        .HasOne(e => e.Assigner)
                        .WithMany(e => e.TicketsToOthers)
                        .HasForeignKey(e => e.AssignerId);

            modelBuilder.Entity<Ticket>()
                        .HasOne(e => e.Status)
                        .WithMany(e => e.Tickets)
                        .HasForeignKey(e => e.StatusId);

            modelBuilder.Entity<Comment>()
                        .HasOne(e => e.CommentUser)
                        .WithMany(e => e.Comments)
                        .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Comment>()
                        .HasOne(e => e.Ticket)
                        .WithMany(e => e.Comments)
                        .HasForeignKey(e => e.TicketId);

            modelBuilder.Entity<Notification>()
                        .HasOne(e => e.NotificationUser)
                        .WithMany(e => e.Notifications)
                        .HasForeignKey(e => e.UserId);
        }
    }
}
