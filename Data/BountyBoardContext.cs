using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BountyBoardServer.Entities;
using Microsoft.Extensions.Configuration;

namespace BountyBoardServer.Data
{
    public class BountyBoardContext : DbContext
    {
        public BountyBoardContext(DbContextOptions<BountyBoardContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
