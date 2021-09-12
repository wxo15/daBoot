﻿using daBoot.Models;
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

        }
    }
}
