using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using BountyBoardServer.Entities;
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

            //var permissions = new List<Permission>
            //{
            //    new Permission { Name = "admin.all", Description = "Superuser permission", Resource = PermissionResource.Any, Action = PermissionAction.Any, Relationship = PermissionRelationship.Any, Attr = PermissionAttr.Any },

            //    new Permission { Name = "user/Event/Create", Description = "", Resource = PermissionResource.Event, Action = PermissionAction.Create},
            //    new Permission { Name = "user/Event/View", Description = "", Resource = PermissionResource.Event, Action = PermissionAction.View, Relationship = PermissionRelationship.Any, Attr = PermissionAttr.Any },
            //    new Permission { Name = "user/Event/Edit", Description = "", Resource = PermissionResource.Event, Action = PermissionAction.Edit, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },
            //    new Permission { Name = "user/Event/Delete", Description = "", Resource = PermissionResource.Event, Action = PermissionAction.Delete, Relationship = PermissionRelationship.Owner},

            //    //new Permission { Name = "user/Location/Create", Description = "", Resource = PermissionResource.Location, Action = PermissionAction.Create, Relationship = PermissionRelationship.Any, Attr = PermissionAttr.Any },
            //    new Permission { Name = "user/Location/View", Description = "", Resource = PermissionResource.Location, Action = PermissionAction.View, Relationship = PermissionRelationship.Any, Attr = PermissionAttr.Any },
            //    //new Permission { Name = "user/Location/Edit", Description = "", Resource = PermissionResource.Location, Action = PermissionAction.Create, Relationship = PermissionRelationship.Any, Attr = PermissionAttr.Any },
            //    //new Permission { Name = "user/Location/Delete", Description = "", Resource = PermissionResource.Location, Action = PermissionAction.Create, Relationship = PermissionRelationship.Any, Attr = PermissionAttr.Any },

            //    // Not sure if we need these as they can just inherit Event permissions
            //    //new Permission { Name = "user/Meeting/Create", Description = "", Resource = PermissionResource.Meeting, Action = PermissionAction.Create},
            //    //new Permission { Name = "user/Meeting/View", Description = "", Resource = PermissionResource.Meeting, Action = PermissionAction.View, Relationship = PermissionRelationship.Member, Attr = PermissionAttr.Any },
            //    //new Permission { Name = "user/Meeting/Edit", Description = "", Resource = PermissionResource.Meeting, Action = PermissionAction.Edit, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },
            //    //new Permission { Name = "user/Meeting/Delete", Description = "", Resource = PermissionResource.Meeting, Action = PermissionAction.Delete, Relationship = PermissionRelationship.Owner},

            //    new Permission { Name = "user/Request/Create", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.Create},
            //    new Permission { Name = "user/Request/View", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.View, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },
            //    new Permission { Name = "user/Request/View", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.Create, Relationship = PermissionRelationship.Member, Attr = PermissionAttr.Any },
            //    new Permission { Name = "user/Request/Edit", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.View, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Description },
            //    new Permission { Name = "user/Request/Edit", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.Edit, Relationship = PermissionRelationship.Member, Attr = PermissionAttr.Approved },
            //    new Permission { Name = "user/Request/Delete", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.Delete, Relationship = PermissionRelationship.Owner},
            //    new Permission { Name = "user/Request/Delete", Description = "", Resource = PermissionResource.Request, Action = PermissionAction.Delete, Relationship = PermissionRelationship.Member},

            //    // Not sure if we need these as they can just inherit Event permissions
            //    //new Permission { Name = "user/Schedule/Create", Description = "", Resource = PermissionResource.Schedule, Action = PermissionAction.Create },
            //    //new Permission { Name = "user/Schedule/View", Description = "", Resource = PermissionResource.Schedule, Action = PermissionAction.View, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },
            //    //new Permission { Name = "user/Schedule/Edit", Description = "", Resource = PermissionResource.Schedule, Action = PermissionAction.Edit, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },
            //    //new Permission { Name = "user/Schedule/Delete", Description = "", Resource = PermissionResource.Schedule, Action = PermissionAction.Delete, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },

            //    new Permission { Name = "user/User/Edit", Description = "", Resource = PermissionResource.User, Action = PermissionAction.Edit, Relationship = PermissionRelationship.Owner, Attr = PermissionAttr.Any },

            //};
            //foreach (Permission p in permissions)
            //{
            //    context.Permissions.Add(p);
            //}
            //context.SaveChanges();


            //var groups = new List<Group>
            //{
            //    new Group { Name = "admin", Description = "program administrators, access to everything", Permissions = new List<Permission>(){ permissions[0] } },
            //    new Group { Name = "user", Description = "Normal user group", Permissions = permissions.GetRange(1, permissions.Count())},
            //};
            //foreach (Group g in groups)
            //{
            //    context.Groups.Add(g);
            //}
            //context.SaveChanges();
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
                new Schedule { Day = Weekday.Monday, Type = ClockType.Open, Time = TimeSpan.FromHours(10) },
                new Schedule { Day = Weekday.Monday, Type = ClockType.Close, Time = TimeSpan.FromHours(22) },
                new Schedule { Day = Weekday.Tuesday, Type = ClockType.Open, Time = TimeSpan.FromHours(10) },
                new Schedule { Day = Weekday.Tuesday, Type = ClockType.Close, Time = TimeSpan.FromHours(22) },
                new Schedule { Day = Weekday.Wednesday, Type = ClockType.Open, Time = TimeSpan.FromHours(10) },
                new Schedule { Day = Weekday.Wednesday, Type = ClockType.Close, Time = TimeSpan.FromHours(22) },
                new Schedule { Day = Weekday.Thursday, Type = ClockType.Open, Time = TimeSpan.FromHours(10) },
                new Schedule { Day = Weekday.Thursday, Type = ClockType.Close, Time = TimeSpan.FromHours(22) },
                new Schedule { Day = Weekday.Friday, Type = ClockType.Open, Time = TimeSpan.FromHours(10) },
                new Schedule { Day = Weekday.Friday, Type = ClockType.Close, Time = TimeSpan.FromHours(22) },
                new Schedule { Day = Weekday.Saturday, Type = ClockType.Open, Time = TimeSpan.FromHours(10) },
                new Schedule { Day = Weekday.Saturday, Type = ClockType.Close, Time = TimeSpan.FromHours(22) },
                new Schedule { Day = Weekday.Sunday, Type = ClockType.Open, Time = TimeSpan.FromHours(11) },
                new Schedule { Day = Weekday.Sunday, Type = ClockType.Close, Time = TimeSpan.FromHours(17) },
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
            var users = new List<User>
            {
                new User{
                    Email = "raquasa123@gmail.com", 
                    Password = "admin",
                    DisplayName = "Dan", 
                    DateOfBirth = DateTime.Parse("1992-10-02"), 
                    Gender = GenderId.Male, 
                    FirstName = "Dan", 
                    LastName = "Rockefeller" 
                },
                new User{
                    Email = "user1@bountyboard.com",
                    Password = "user1",
                    DisplayName = "John",
                    DateOfBirth = DateTime.Parse("1992-10-02"),
                    Gender = GenderId.Male,
                    FirstName = "John",
                    LastName = "Doe"
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
                    RequestNeeded = false,
                    RequestsOpen = false,
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
                    RequestNeeded = false,
                    RequestsOpen = false,
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
