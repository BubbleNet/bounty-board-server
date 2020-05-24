using System;
using System.Collections.Generic;
using BountyBoardServer.Models;
using NetTopologySuite.Geometries;

namespace BountyBoardServer.Entities
{
    public enum Interval
    {
        Daily,
        Weekly,
        Monthly,
    }
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Game Game { get; set; }
        public Edition Edition { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public Location EventLocation { get; set; }
        public ICollection<Meeting> Meetings { get; set; }
        public bool Repeating { get; set; }
        public Interval RepeatInterval { get; set; }
        public ICollection<User> Participants { get; set; }
        public User Host { get; set; }
        public bool RequestNeeded { get; set; } // Indicates if a user needs to request to join an event
        public bool RequestsOpen { get; set; } // Indicates if requests are allowed for this event
        public ICollection<Request> Requests { get; set; } // Collection of requests to join the event

        public PrivateEventDto ToPrivateEventDto()
        {
            var part = new List<PublicUserDetailsDto>();
            if ((this.Participants != null) && (this.Participants.Count > 0))
            {
                foreach (User i in this.Participants) part.Add(i.ToPublicUserDetailsDto());
            }

            var req = new List<HostRequestDto>();
            if ((this.Requests != null) && (this.Requests.Count > 0))
            {
                foreach (Request i in this.Requests) req.Add(i.ToHostRequestDto());
            }

            return new PrivateEventDto
            {
                Id = this.Id,
                Name = this.Name,
                Game = this.Game.ToBasicGameDto(),
                Edition = this.Edition,
                Summary = this.Summary,
                Description = this.Description,
                MinPlayers = this.MinPlayers,
                MaxPlayers = this.MaxPlayers,
                EventLocation = this.EventLocation,
                Meetings = this.Meetings,
                Repeating = this.Repeating,
                RepeatInterval = this.RepeatInterval,
                Participants = part,
                Host = this.Host.ToPrivateUserDetailsDto(),
                RequestNeeded = this.RequestNeeded,
                RequestsOpen = this.RequestsOpen,
                Requests = req
            };
        }

        public PublicEventDto ToPublicEventDto()
        {
            var part = new List<PublicUserDetailsDto>();
            if ((this.Participants != null) && (this.Participants.Count > 0))
            {
                foreach (User i in this.Participants) part.Add(i.ToPublicUserDetailsDto());
            }

            return new PublicEventDto
            {
                Id = this.Id,
                Name = this.Name,
                Game = this.Game.ToBasicGameDto(),
                Edition = this.Edition,
                Summary = this.Summary,
                Description = this.Description,
                MinPlayers = this.MinPlayers,
                MaxPlayers = this.MaxPlayers,
                EventLocation = this.EventLocation,
                Meetings = this.Meetings,
                Repeating = this.Repeating,
                RepeatInterval = this.RepeatInterval,
                Participants = part,
                Host = this.Host.ToPublicUserDetailsDto(),
                RequestNeeded = this.RequestNeeded,
                RequestsOpen = this.RequestsOpen
            };
        }
    }
}
