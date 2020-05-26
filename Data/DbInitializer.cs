using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BountyBoardServer.Entities;
using BountyBoardServer.Helpers;
using BountyBoardServer.Models;

namespace BountyBoardServer.Data
{
    public class DbInitializer
    {
        /*
        Initializes a new database with test data. Clears out any old data. 
        */
        public static void Initialize(BountyBoardContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            if (context.Users.Any())
            {
                return;
            }

            var roles = new List<Role>
            {
                new Role{ Name = "Administrator" }
            };
            foreach (Role r in roles)
            {
                context.Roles.Add(r);
            }
            context.SaveChanges();

            var editions = new List<Edition>
            {
                new Edition {Name = "3.5e"},
                new Edition {Name = "5e"},
                new Edition {Name = "Advanced"},
                new Edition {Name = "1" },
                new Edition {Name = "2"}
            };
            foreach (Edition e in editions)
            {
                context.Editions.Add(e);
            }
            context.SaveChanges();


            var games = new List<Game>
            {
                new Game {Name = "Dungeons & Dragons", Editions = editions.GetRange(0,2), Publisher = "Wizards of the Coast"},
                new Game {Name = "Pathfinder", Editions = editions.GetRange(3, 2), Publisher = "Paizo"}
            };
            foreach (Game g in games)
            {
                context.Games.Add(g);
            }
            context.SaveChanges();


            var schedules = new List<Schedule>
            {
                new Schedule { Day = Weekday.Monday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 10, 0, 0) },
                new Schedule { Day = Weekday.Monday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 22, 0, 0) },
                new Schedule { Day = Weekday.Tuesday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 10, 0, 0) },
                new Schedule { Day = Weekday.Tuesday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 22, 0, 0) },
                new Schedule { Day = Weekday.Wednesday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 10, 0, 0) },
                new Schedule { Day = Weekday.Wednesday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 22, 0, 0) },
                new Schedule { Day = Weekday.Thursday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 10, 0, 0) },
                new Schedule { Day = Weekday.Thursday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 22, 0, 0) },
                new Schedule { Day = Weekday.Friday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 10, 0, 0) },
                new Schedule { Day = Weekday.Friday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 22, 0, 0) },
                new Schedule { Day = Weekday.Saturday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 10, 0, 0) },
                new Schedule { Day = Weekday.Saturday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 22, 0, 0) },
                new Schedule { Day = Weekday.Sunday, Type = ClockType.Open, Time = new DateTime(2000, 1, 1, 11, 0, 0) },
                new Schedule { Day = Weekday.Sunday, Type = ClockType.Close, Time = new DateTime(2000, 1, 1, 17, 0, 0) },
            };
            foreach (Schedule s in schedules)
            {
                context.Schedules.Add(s);
            }
            context.SaveChanges();


            var locations = new List<Location>
            {
                new Location
                {
                    Name = "Online",
                    Description = "Online. Roll 20, Fantasy Grounds, Zoom, Discord, etc."
                },
                new Location
                {
                    Name = "Millennium Games",
                    StreetNumber = "3047",
                    StreetName = "W Henrietta Rd",
                    City = "Rochester",
                    State = "NY",
                    ZipCode = "14623",
                    Country = "USA",
                    Description = "Game Store",
                    Hours = schedules
                },
                 new Location
                {
                    Name = "Rochester Institute of Technology",
                    StreetNumber = "169",
                    StreetName = "Lomb Memorial Drive",
                    City = "Rochester",
                    State = "NY",
                    ZipCode = "14623",
                    Country = "USA",
                    Description = "RWAG at the Student Alumni Union",
                    Hours = schedules
                }
            };
            foreach (Location l in locations)
            {
                context.Locations.Add(l);
            }
            context.SaveChanges();


            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    StartTime =  DateTime.Parse("5/28/2020 19:00:00"),
                    EndTime = DateTime.Parse("5/29/2020 01:00:00")
                },
                new Meeting
                {
                    StartTime =  DateTime.Parse("6/01/2020 19:00:00"),
                    EndTime = DateTime.Parse("6/02/2020 01:00:00")
                }
            };
            foreach (Meeting m in meetings)
            {
                context.Meetings.Add(m);
            }
            var testSalt = Encoding.ASCII.GetBytes("1");
            var users = new List<User>
            {
                new User{
                    Email = "raquasa123@gmail.com", 
                    Password = UserHelper.Hash("admin", testSalt),
                    DisplayName = "Dan", 
                    DateOfBirth = DateTime.Parse("1992-10-02"), 
                    Gender = GenderId.Male, 
                    FirstName = "Dan", 
                    LastName = "Rockefeller",
                    Roles = roles,
                    Salt = testSalt
                },
                new User{
                    Email = "user1@bountyboard.com",
                    Password = UserHelper.Hash("password", testSalt),
                    DisplayName = "John",
                    DateOfBirth = DateTime.Parse("1992-10-02"),
                    Gender = GenderId.Male,
                    FirstName = "John",
                    LastName = "Doe",
                    Salt = testSalt
                },
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();


            var requests = new List<Request>
            {
                new Request 
                {
                    Requester = users[1],
                    Description = "Hi I want to join your game!",
                    Status = RequestStatus.Pending,
                },
                new Request
                {
                    Requester = users[1],
                    Description = "Pls let jon",
                    Status = RequestStatus.Pending,
                }
            };
            foreach (Request r in requests)
            {
                context.Requests.Add(r);
            }
            context.SaveChanges();


            var events = new List<Event>
            {
                new Event{
                    Name = "Adventurers in Windhaven", 
                    Game = games[0],
                    Edition = editions[0],
                    Summary = "You and four other adventurers have a chance encounter in Windhaven leading to many adventures.",
                    Description = "Description Placeholder",
                    MinPlayers = 2,
                    MaxPlayers = 5,
                    Meetings = new List<Meeting>(){ meetings[0], meetings[1] },
                    Repeating = true,
                    RepeatInterval = Interval.Weekly,
                    Host = users[0],
                    RequestNeeded = true,
                    RequestsOpen = true,
                    Requests = new List<Request>(){ requests[0] }
                },
                new Event{
                    Name = "Baulder's Gate, Decent into Avernus",
                    Game = games[0],
                    Edition = editions[1],
                    Summary = "A party of six adventurers decends into the depths of hell to fight against an evil Devil Prince.",
                    Description = "This is a description",
                    MinPlayers = 4,
                    MaxPlayers = 4,
                    Meetings = new List<Meeting>(){ meetings[0], meetings[1] },
                    Repeating = false,
                    RepeatInterval = Interval.Weekly,
                    Host = users[0],
                    RequestNeeded = true,
                    RequestsOpen = true,
                    Requests = new List<Request>(){ requests[1] }
                },
                new Event{
                    Name = "Adventurers on the Sword Coast",
                    Game = games[1],
                    Edition = editions[4],
                    Summary = "Join a group of adventurers protecting the sword coast againt pirates!",
                    Description = "I don't want to write out a whole description for this.",
                    MinPlayers = 4,
                    MaxPlayers = 6,
                    Meetings = new List<Meeting>(){ meetings[0], meetings[1] },
                    Repeating = false,
                    RepeatInterval = Interval.Weekly,
                    Host = users[0],
                    RequestNeeded = false,
                    RequestsOpen = false
                },
            };
            foreach (Event e in events)
            {
                context.Events.Add(e);
            }
            context.SaveChanges();
        }
    }
}
