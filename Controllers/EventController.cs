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
        /// <param name="eventModel">The new event to create</param>
        /// <returns>
        /// The event if it was created
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost]
        public IActionResult Create([FromBody]NewEventDto eventModel)
        {
            // Validate
            if (string.IsNullOrEmpty(eventModel.Name)) return BadRequest(new { message = "Event name cannot be empty" });
            if (eventModel.GameId == 0) return BadRequest(new { message = "Event gameId cannot be empty" });
            if (eventModel.EditionId == 0) return BadRequest(new { message = "Event editionId game cannot be empty" });
            if (eventModel.MinPlayers > eventModel.MaxPlayers) return BadRequest(new { message = "Minimum players cannot be bigger than maximum players" });
           
            var location = _context.Locations.Where(x => x.Id == eventModel.EventLocationId).FirstOrDefault();
            if (location == null) return BadRequest(new { message = "Location not found" });
            
            var game = _context.Games.Include(g => g.Editions).Where(g => g.Id == eventModel.GameId).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });

            Edition edition = null;
            foreach (Edition e in game.Editions) if (e.Id == eventModel.EditionId) edition = e;
            if (edition == null) return BadRequest(new { message = "Edition not found" });

            // Get current user
            var user = _userService.GetCurrentUser(this);
           
            // Create event with verified location and host as current user
            var newEvent = eventModel.ToEvent(location, user, game, edition);
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
            if (eventModel.GameId == 0) return BadRequest(new { message = "Event name game cannot be empty" });
            if (eventModel.EditionId == 0) return BadRequest(new { message = "Event version game cannot be empty" });
            if (eventModel.MinPlayers > eventModel.MaxPlayers) return BadRequest(new { message = "Minimum players cannot be bigger than maximum players" });
            
            var location = _context.Locations.Where(x => x.Id == eventModel.EventLocationId).FirstOrDefault();
            if (location == null) return BadRequest(new { message = "Location not found" });

            var game = _context.Games.Where(g => g.Id == eventModel.GameId).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });

            var edition = _context.Editions.Where(e => e.Id == eventModel.EditionId).FirstOrDefault();
            if (edition == null) return BadRequest(new { message = "Edition not found" });

            // Get current user
            var user = _userService.GetCurrentUser(this);

            // Get the existing event
            var ev = _context.Events.Where(x => x.Id == id && x.Host.Id == user.Id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            //Update allowed event properties
            // Host can't be changed, other foreign keys cant be changed with patch method.
            ev.Name = eventModel.Name;
            ev.Game = game;
            ev.Edition = edition;
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
                .Include(e => e.Game)
                .Include(e => e.Edition)
                .Include(e => e.EventLocation)
                .Include(e => e.Meetings)
                .Include(e => e.Host)
                .Include(e => e.Requests).ThenInclude(r => r.Requester)
                .Include(e => e.Participants)
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
                .Include(e => e.Game)
                .Include(e => e.Edition)
                .Include(e => e.EventLocation)
                .Include(e => e.Meetings)
                .Include(e => e.Host)
                .Include(e => e.Requests).ThenInclude(x => x.Requester)
                .Include(e => e.Participants)
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
            int gameId, 
            int editionId, 
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
                .Include(e => e.Game)
                .Include(e => e.Edition)
                .Include(e => e.EventLocation)
                .Include(e => e.Meetings)
                .Include(e => e.Host)
                .Include(e => e.Requests).ThenInclude(r => r.Requester)
                .Include(e => e.Participants);

            if (gameId != 0) query = query.Where(x => x.Game.Id == gameId);
            if (editionId != 0) query = query.Where(x => x.Edition.Id == editionId);
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
        [HttpPost("join/{id}")]
        public IActionResult Join(int id)
        {
            // Get event
            var ev = _context.Events
                .Include(e => e.Game)
                .Include(e => e.Edition)
                .Include(e => e.EventLocation)
                .Include(e => e.Meetings)
                .Include(e => e.Requests)
                .Include(e => e.Participants)
                .Include(e => e.Host)
                .Where(e => e.Id == id)
                .FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });
            // Get the requesting user
            var user = _userService.GetCurrentUser(this);

            // Ensure the requesting user is not the host
            if (ev.Host.Id == user.Id) return BadRequest( new { message = "Host cannot join event" });

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
                .Include(e => e.Game)
                .Include(e => e.Edition)
                .Include(e => e.EventLocation)
                .Include(e => e.Meetings)
                .Include(e => e.Requests)
                .Include(e => e.Participants)
                .Include(e => e.Host)
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
            // Get event
            var ev = _context.Events
                .Include(e => e.Meetings)
                .Include(e => e.Requests)
                .Where(e => e.Id == id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            // Allow deletion if current user is host or Admin
            var user = _userService.GetCurrentUser(this);
            if (ev.Host.Id == user.Id || user.IsRole("Administrator"))
            {
                var obj = _context.Remove(ev);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound(new { message = "Event not found" });

        }

        /// <summary>
        /// Create a new meeting for a specified event
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <returns>
        /// The meeting
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost("{id}/meeting")]
        public IActionResult CreateMeeting(int id, [FromBody] Meeting meeting)
        {
            // Get event
            var ev = _context.Events
                .Include(e => e.Meetings)
                .Include(e => e.Host)
                .Where(e => e.Id == id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            // Check if user is host
            var user = _userService.GetCurrentUser(this);
            if (user.Id != ev.Host.Id) return NotFound(new { message = "Event not found" });

            // Check if start time is earlier than complete time
            if(DateTime.Compare(meeting.StartTime, meeting.EndTime) <= 0) 
                return BadRequest(new { message = "Start time is later than or equal to end time" });

            // Save the meeting
            ev.Meetings.Add(meeting);
            _context.SaveChanges();

            return Ok(meeting);
        }

        /// <summary>
        /// Delete an existing meeting
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <param name="meetingId">The id of the meeting.</param>
        /// <returns>
        /// Ok if deleted
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpDelete("{id}/meeting/{meetingId}")]
        public IActionResult DeleteMeeting(int id, int meetingId)
        {
            // Get event
            var ev = _context.Events
                .Include(e => e.Meetings)
                .Include(e => e.Host)
                .Where(e => e.Id == id).FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            // Check if user is host
            var user = _userService.GetCurrentUser(this);
            if (user.Id != ev.Host.Id) return NotFound(new { message = "Event not found" });

            // Find the meeting and delete it
            foreach(Meeting m in ev.Meetings)
            {
                if (m.Id == meetingId)
                {
                    _context.Meetings.Remove(m);
                    _context.SaveChanges();
                    return Ok();
                }
            }
            return NotFound(new { message = "Meeting not found" });
        }
    }
}