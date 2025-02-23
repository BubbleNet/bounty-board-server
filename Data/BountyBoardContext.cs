﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BountyBoardServer.Entities;
using Microsoft.Extensions.Configuration;
using BountyBoardServer.Models;

namespace BountyBoardServer.Data
{
    public class BountyBoardContext : DbContext
    {
        public BountyBoardContext(DbContextOptions<BountyBoardContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Edition> Editions { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
