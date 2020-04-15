using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BountyBoardServer.Model;
using BountyBoardServer.Services;
using Newtonsoft.Json;
using BountyBoardServer.Data;
using BountyBoardServer.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BountyBoardServer.Models;
using BountyBoardServer.Helpers;

namespace BountyBoardServer.Controllers
{
    [Route("events/")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly BountyBoardContext _context;
        private IUserService _userService;

        public EventController(BountyBoardContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody]Event newEvent)
        {
            // Return error if event has no name
            if (string.IsNullOrEmpty(newEvent.Name)) return BadRequest(new { message = "Event name cannot be empty" });

            // Get the creating user
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            var user = _context.Users.Where(x => x.Id == userIdInt).FirstOrDefault();

            // Set the event host to the creating user
            newEvent.Host = user;
            _context.Events.Add(newEvent);
            _context.SaveChanges();

            // Return status 200 if sucessful
            return Ok();
        }

        [HttpPost("update")]
        public IActionResult Update([FromBody] Event evUpdate)
        {
            var ev = _context.Events.Where(x => x.Id == evUpdate.Id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (!string.IsNullOrEmpty(evUpdate.Name)) ev.Name = evUpdate.Name;
            else return BadRequest(new { message = "Event name cannot be empty" });
            ev.MinPlayers = evUpdate.MinPlayers;
            ev.MaxPlayers = evUpdate.MaxPlayers;
            ev.StartTime = evUpdate.StartTime;
            ev.EndTime = evUpdate.EndTime;
            ev.RequestNeeded = evUpdate.RequestNeeded;
            ev.RequestsOpen = evUpdate.RequestsOpen;
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("get/{id}")]
        public IActionResult Get(int id)
        {
            var ev = _context.Events
                .Include(h => h.Host)
                .Include(h => h.Requests).ThenInclude(x => x.Requester)
                .Include(p => p.Participants)
                .Where(x => x.Id == id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            var user = _userService.GetCurrentUser(this);
            if(user.Id == ev.Host.Id)
            {
                // If owner, return private view of event
                return Ok(EventToPrivateEventModel(ev));
            }
            // If not owner, return public view of event
            return Ok(EventToPublicEventModel(ev));
        }

        [HttpGet("me")]
        public IActionResult ListMyEvents()
        {
            // Get the creating user
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            // var user = _context.Users.Where(x => x.Id == userIdInt).FirstOrDefault();

            // Get events for that user
            var events = _context.Events
                .Include(h => h.Host)
                .Include(h => h.Requests).ThenInclude(x => x.Requester)
                .Include(p => p.Participants)
                .Where(x => x.Host.Id == userIdInt);

            List<PrivateEventModel> returnEvents = new List<PrivateEventModel>();
            // TODO: Format event data
            foreach(Event i in events)
            {
                returnEvents.Add(EventToPrivateEventModel(i));
            }

            return Ok(returnEvents);
        }

        [HttpGet("all")]
        public IActionResult ListEvents()
        {
            var events = _context.Events
                .Include(h => h.Host)
                .Include(h => h.Requests).ThenInclude(x => x.Requester)
                .Include(p => p.Participants);

            List<PublicEventModel> returnEvents = new List<PublicEventModel>();
            // TODO: Format event data
            foreach (Event i in events)
            {
                returnEvents.Add(EventToPublicEventModel(i));
            }

            return Ok(returnEvents);
        }

        [HttpGet("join/{id}")]
        public IActionResult Join(int id)
        {
            // Get event
            var ev = _context.Events
                .Include(h => h.Requests)
                .Include(p => p.Participants)
                .Where(e => e.Id == id)
                .FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });
            // Get the requesting user
            var user = _userService.GetCurrentUser(this);
            // If a request is needed check to see if request is approved
            if (ev.RequestNeeded)
            {
                var approved = false;
                foreach (Request r in ev.Requests)
                {
                    if (r.Requester.Id == user.Id)
                    {
                        if (r.Approved == true)
                        {
                            approved = true;
                            // Remove the request, but don't save until the user is actually added to the event
                            _context.Requests.Remove(r);
                        }
                        break;
                    } 
                }
                if (approved == false)
                {
                    return BadRequest(new { message = "This event requires a request to join" });
                }
            }
            // Check to see if the current participants exceeds the set maxamum participants
            if (ev.Participants.ToList().Count >= ev.MaxPlayers)
            {
                return BadRequest(new { message = "This event is already at capacity" });
            }
            // If not, add the user to the event
            else
            {
                //Check if user already exists
                foreach (User u in ev.Participants)
                {
                    if (u.Id == user.Id)
                    {
                        return BadRequest(new { message = "This user has already joined." });
                    }
                }
                ev.Participants.Add(user);
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost("leave/{id}")]
        public IActionResult Leave(int id)
        {
            // Get event
            var ev = _context.Events
                .Include(h => h.Requests)
                .Include(p => p.Participants)
                .Where(e => e.Id == id)
                .FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            // Get the requesting user
            var user = _userService.GetCurrentUser(this);
            // Check to see if the user is a participant in the event
            var isParticipant = false;
            foreach (User u in ev.Participants)
            {
                if (u.Id == user.Id)
                {
                    isParticipant = true;
                    break;
                }
            }
            if (!isParticipant) return NotFound(new { message = "User not a participant of event" });

            ev.Participants.Remove(user);
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var ev = _context.Events.Where(e => e.Id == id);
            _context.Remove(ev);
            _context.SaveChanges();
            return Ok();
        }

        private PublicEventModel EventToPublicEventModel(Event ev)
        {
            var part = new List<PublicUserDetailsModel>();
            foreach (User i in ev.Participants)
            {
                part.Add(UserToPublicUserDetailsModel(i));
            }
            var host = UserToPublicUserDetailsModel(ev.Host);
            return new PublicEventModel
            {
                Id = ev.Id,
                Name = ev.Name,
                MinPlayers = ev.MinPlayers,
                MaxPlayers = ev.MaxPlayers,
                Location = ev.Location,
                StartTime = ev.StartTime,
                EndTime = ev.EndTime,
                Participants = part,
                Host = host,
                RequestNeeded = ev.RequestNeeded,
                RequestsOpen = ev.RequestsOpen
            };
        }

        private PrivateEventModel EventToPrivateEventModel(Event ev)
        {
            var part = new List<PublicUserDetailsModel>();
            foreach (User i in ev.Participants)
            {
                part.Add(UserToPublicUserDetailsModel(i));
            }
            var req = new List<HostRequestModel>();
            foreach (Request i in ev.Requests)
            {
                req.Add(RequestToHostRequestModel(i));
            }
            var host = UserToPrivateUserDetailsModel(ev.Host);
            return new PrivateEventModel
            {
                Id = ev.Id,
                Name = ev.Name,
                MinPlayers = ev.MinPlayers,
                MaxPlayers = ev.MaxPlayers,
                Location = ev.Location,
                StartTime = ev.StartTime,
                EndTime = ev.EndTime,
                Participants = part,
                Host = host,
                RequestNeeded = ev.RequestNeeded,
                RequestsOpen = ev.RequestsOpen,
                Requests = req
            };
        }

        private PrivateUserDetailsModel UserToPrivateUserDetailsModel(User user)
        {
            return new PrivateUserDetailsModel
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        private PublicUserDetailsModel UserToPublicUserDetailsModel(User user)
        {
            return new PublicUserDetailsModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Age = UserHelper.GetAge(user.DateOfBirth),
                Gender = user.Gender,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        private HostRequestModel RequestToHostRequestModel(Request request)
        {
            return new HostRequestModel
            {
                Requester = UserToPublicUserDetailsModel(request.Requester),
                Description = request.Description
            };
        }
    }
}