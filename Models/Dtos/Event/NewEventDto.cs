using BountyBoardServer.Data;
using BountyBoardServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BountyBoardServer.Models
{
    public class NewEventDto
    {
        public string Name { get; set; }
        public string Game { get; set; }
        public string Version { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int EventLocationId { get; set; }
        public bool Repeating { get; set; }
        public Interval RepeatInterval { get; set; }
        public bool RequestNeeded { get; set; } // Indicates if a user needs to request to join an event
        public bool RequestsOpen { get; set; } // Indicates if requests are allowed for this event

        public Event ToEvent(Location location, User host)
        {
            return new Event()
            {
                Name = this.Name,
                Game = this.Game,
                Version = this.Version,
                Summary = this.Summary,
                Description = this.Description,
                MinPlayers = this.MinPlayers,
                MaxPlayers = this.MaxPlayers,
                EventLocation = location,
                Repeating = this.Repeating,
                RepeatInterval = this.RepeatInterval,
                RequestNeeded = this.RequestNeeded,
                RequestsOpen = this.RequestsOpen,
                Host = host
            };


        }
    }

    
}
