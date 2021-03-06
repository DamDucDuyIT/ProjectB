﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Person>().ToTable("Person");
            builder.Entity<Question>().ToTable("Question");
            builder.Entity<Answer>().ToTable("Answer");
            builder.Entity<Tag>().ToTable("Tag");
            builder.Entity<Support>().ToTable("Support");
            builder.Entity<Support2>().ToTable("Support2");
            builder.Entity<Vote>().ToTable("Vote");
            builder.Entity<Vote2>().ToTable("Vote2");
        }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Support> Supports { get; set; }
        public DbSet<Support2> Support2s { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Vote2> Vote2s { get; set; }

    }
}
