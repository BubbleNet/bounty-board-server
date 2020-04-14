using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BountyBoardServer.Entities;

namespace BountyBoardServer.Data
{
    public class DbInitializer
    {
        public static void Initialize(BountyBoardContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            if (context.Users.Any())
            {
                return;
            }

            var users = new User[]
            {
                new User{ Email = "admin@bountyboard.com", Password = "admin", DisplayName = "Admin", DateOfBirth = DateTime.Parse("1992-10-02") },
                new User{ Email = "dan@bountyboard.com", Password = "dan", DisplayName = "Dan", DateOfBirth = DateTime.Parse("1992-10-02") }
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            /*var events = new Event[]
            {
                new Event{Name = "Adventurers in Windhaven", MinPlayers = 2, MaxPlayers = 6, StartTime = DateTime.Parse("2020-10-10")},
                new Event{Name = "Friday Night Magic", MinPlayers = 2, MaxPlayers = 100, StartTime = DateTime.Parse("2020-10-12")}
            };
            foreach (Event e in events)
            {
                context.Events.Add(e);
            }
            context.SaveChanges();*/
        }
    }
}
