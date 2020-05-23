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
using System.Data.Common;

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

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <returns>
        /// The event if it was created
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost("create")]
        public IActionResult Create([FromBody]NewEventDto eventModel)
        {
            // Validate
            if (string.IsNullOrEmpty(eventModel.Name)) return BadRequest(new { message = "Event name cannot be empty" });
            if (string.IsNullOrEmpty(eventModel.Game)) return BadRequest(new { message = "Event name game cannot be empty" });
            if (string.IsNullOrEmpty(eventModel.Version)) return BadRequest(new { message = "Event version game cannot be empty" });
            if (eventModel.MinPlayers > eventModel.MaxPlayers) return BadRequest(new { message = "Minimum players cannot be bigger than maximum players" });
            var location = _context.Locations.Where(x => x.Id == eventModel.EventLocationId).FirstOrDefault();
            if (location == null) return BadRequest(new { message = "Location not found" });
            
            // Get current user
            var user = _userService.GetCurrentUser(this);
           
            // Create event with verified location and host as current user
            var newEvent = eventModel.ToEvent(location, user);
            _context.Events.Add(newEvent);
            _context.SaveChanges();

            return Ok(newEvent.ToPrivateEventDto());
        }

        /// <summary>
        /// Updates the properties of an event
        /// </summary>
        /// <param name="event">The event including the Id of the event and any properties to update</param>
        /// <returns>
        /// The updated event
        /// </returns>
        /// <remarks>
        ///
        /// </remarks>
        [HttpPatch("{id}")]
        //TODO: Change this so that it only updates the stuff this user is allowed to update.
        public IActionResult Update(int id, [FromBody] NewEventDto eventModel)
        {
            // Validate
            if (string.IsNullOrEmpty(eventModel.Name)) return BadRequest(new { message = "Event name cannot be empty" });
            if (string.IsNullOrEmpty(eventModel.Game)) return BadRequest(new { message = "Event name game cannot be empty" });
            if (string.IsNullOrEmpty(eventModel.Version)) return BadRequest(new { message = "Event version game cannot be empty" });
            if (eventModel.MinPlayers > eventModel.MaxPlayers) return BadRequest(new { message = "Minimum players cannot be bigger than maximum players" });
            var location = _context.Locations.Where(x => x.Id == eventModel.EventLocationId).FirstOrDefault();
            if (location == null) return BadRequest(new { message = "Location not found" });

            // Get current user
            var user = _userService.GetCurrentUser(this);

            // Get the existing event
            var ev = _context.Events.Where(x => x.Id == id && x.Host.Id == user.Id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            //Update allowed event properties
            // Host can't be changed, other foreign keys cant be changed with patch method.
            ev.Name = eventModel.Name;
            ev.Game = eventModel.Game;
            ev.Version = eventModel.Version;
            ev.Summary = eventModel.Summary;
            ev.Description = eventModel.Description;
            ev.MinPlayers = eventModel.MinPlayers;
            ev.MaxPlayers = eventModel.MaxPlayers;
            ev.EventLocation = location;
            ev.Repeating = eventModel.Repeating;
            ev.RepeatInterval = eventModel.RepeatInterval;
            ev.RequestNeeded = eventModel.RequestNeeded;
            ev.RequestsOpen = eventModel.RequestsOpen;

            _context.SaveChanges();

            return Ok(ev.ToPrivateEventDto());
        }

        /// <summary>
        /// Gets an event
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <returns>
        /// The event if it exists
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("{id}")]
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
                return Ok(ev.ToPrivateEventDto());
            }
            // If not owner, return public view of event
            return Ok(ev.ToPublicEventDto());
        }

        /// <summary>
        /// Gets all events that belong to the current user
        /// </summary>
        /// <returns>
        /// List of events belonging to the current user
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("me")]
        public IActionResult ListMyEvents()
        {
            // Get the creating user
            var user = _userService.GetCurrentUser(this);

            // Get events for that user
            var events = _context.Events
                .Include(h => h.Host)
                .Include(h => h.Requests).ThenInclude(x => x.Requester)
                .Include(p => p.Participants)
                .Where(x => x.Host.Id == user.Id).ToList();
            if (events.Count() < 1) return NotFound(new { message = "No events found" });

            List<PrivateEventDto> returnEvents = new List<PrivateEventDto>();
            foreach(Event i in events) returnEvents.Add(i.ToPrivateEventDto());

            return Ok(returnEvents);
        }

        /// <summary>
        /// Get events based on filter criteria
        /// </summary>
        /// <returns>
        /// a list of events
        /// </returns>
        /// <remarks>
        ///
        /// </remarks>
        [HttpGet("list")]
        public IActionResult ListEvents(
            string game, 
            string version, 
            int minPlayers, 
            int maxPlayers, 
            string repeating, 
            string requestsNeeded, 
            string requestsOpen,
            Double lat,
            Double lon,
            int range
            )
        {
            IQueryable<Event> query = _context.Events
                .Include(h => h.Host)
                .Include(h => h.Requests).ThenInclude(x => x.Requester)
                .Include(p => p.Participants);

            if (!string.IsNullOrEmpty(game)) query = query.Where(x => x.Game == game);
            if (!string.IsNullOrEmpty(version)) query = query.Where(x => x.Version == version);
            if (minPlayers != 0) query = query.Where(x => x.MinPlayers <= minPlayers);
            if (maxPlayers != 0) query = query.Where(x => x.MaxPlayers >= maxPlayers);
            if (!string.IsNullOrEmpty(repeating)) query = query.Where(x => x.Repeating == bool.Parse(repeating));
            if (!string.IsNullOrEmpty(requestsNeeded)) query = query.Where(x => x.RequestNeeded == bool.Parse(requestsNeeded));
            if (!string.IsNullOrEmpty(requestsOpen)) query = query.Where(x => x.RequestsOpen == bool.Parse(requestsOpen));
            //TODO: Implement distance filtering

            var events = query.ToList();
            if (events.Count() < 1) return NotFound(new { message = "No events found" });

            List<PublicEventDto> returnEvents = new List<PublicEventDto>();
            // TODO: Format event data
            foreach (Event i in events)
            {
                returnEvents.Add(i.ToPublicEventDto());
            }

            return Ok(returnEvents);
        }

        /// <summary>
        /// Adds the current user to the event
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <returns>
        /// The event with the user added
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
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
                        if (r.Status == RequestStatus.Approved)
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
                    return BadRequest(new { message = "This event requires an approved request to join" });
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
            return Ok(ev.ToPublicEventDto());
        }

        /// <summary>
        /// Removes the current user from the event
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <returns>
        /// The event without the current user
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
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
            return Ok(ev.ToPublicEventDto());
        }

        /// <summary>
        /// Deletes an Event.
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <returns>
        /// OK if event was deleted
        /// Not Found if event was not found
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ev = _context.Events.Where(e => e.Id == id);
            if (ev == null) return NotFound(new { message = "Event not found" });

            var obj = _context.Remove(ev);
            _context.SaveChanges();
            return Ok();
        }
    }
}