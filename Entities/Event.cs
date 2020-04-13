using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string Location { get; set; }
        public DateTime Time { get; set; }
        public ICollection<User> Participants { get; set; }
        public User Host { get; set; }
    }
}
