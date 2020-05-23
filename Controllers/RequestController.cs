using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BountyBoardServer.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BountyBoardServer.Entities;
using Microsoft.AspNetCore.Authorization;
using BountyBoardServer.Services;
using BountyBoardServer.Models;

namespace BountyBoardServer.Controllers
{
    [Route("requests/")]
    [ApiController]
    [Authorize]
    public class RequestController : ControllerBase
    {
        private readonly BountyBoardContext _context;
        private IUserService _userService;

        public RequestController(BountyBoardContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }


        /// <summary>
        /// Creates a new request
        /// </summary>
        ///  /// <param name="eventId">The id of the event you are creating your request for</param>
        ///  /// <param name="description">the modified description</param>
        /// <returns>
        /// The modified request
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost("create")]
        public IActionResult Create([FromBody] int eventId, string description)
        {
            // Get event
            var ev = _context.Events
                .Include(h => h.Requests).ThenInclude(r => r.Requester)
                .Include(p => p.Participants)
                .Where(e => e.Id == eventId)
                .FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Event not found" });

            // Get the current user
            var user = _userService.GetCurrentUser(this);

            // Check if user already has request
            foreach (Request r in ev.Requests)
            {
                if (r.Requester.Id == user.Id)
                {
                    return BadRequest(new { message = "request already exists" });
                }
            }

            // Check if requests are allowed for event
            if (!ev.RequestsOpen) return BadRequest(new { message = "This event is not taking requests right now." });

            // If event is full OR if event requires a request, create a request
            Request req;
            if (!ev.RequestNeeded || ev.Participants.ToList().Count >= ev.MaxPlayers)
            {
                req = new Request { Requester = user, Event = ev, Description = description };
                _context.Requests.Add(req);
                _context.SaveChanges();
            }
            // If requests aren't necessary, return bad request.
            else return BadRequest(new { message = "This event does not require a request to join." });
            return Ok(req);
        }

        /// <summary>
        /// Updates a request description
        /// </summary>
        ///  /// <param name="eventId">The id of the event you are modifying your request for</param>
        ///  /// <param name="description">the modified description of your request</param>
        /// <returns>
        /// The modified request
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPatch("{id}")]
        public IActionResult Update(int eventId, [FromBody] string description)
        {
            // Get current user
            var user = _userService.GetCurrentUser(this);

            // Get request
            var req = _context.Requests
                .Include(r => r.Event)
                .Include(r => r.Requester)
                .Where(x => x.Requester.Id == user.Id && x.Event.Id == eventId).FirstOrDefault();
            if (req == null) return NotFound(new { message = "Request not found" });
            req.Description = description;
            _context.SaveChanges();
            return Ok(req.ToHostRequestDto());
        }

        /// <summary>
        /// Sets the status of a request
        /// </summary>
        ///  /// <param name="id">The id of the request you are setting the status of</param>
        /// <returns>
        /// The modified request
        /// </returns>
        /// <remarks>
        /// After a request is approved, a user needs to join the event with the event/join endpoint. It shouldn't auto add them.
        /// </remarks>
        [HttpPatch("{id}/set")]
        public IActionResult Approve(int id, RequestStatus status)
        {
            // Get current user
            var user = _userService.GetCurrentUser(this);
            var query = _context.Requests
                .Include(r => r.Event).ThenInclude(e => e.Host)
                .Where(r => r.Id == id);
            if (status == RequestStatus.Approved || status == RequestStatus.Denied || status == RequestStatus.Pending)
            {
                query = query.Where(r => r.Event.Host.Id == user.Id);
            }
            else if (status == RequestStatus.Deleted)
            {
                query = query.Where(r=> r.Requester.Id == user.Id);
            }
            var req = query.FirstOrDefault();
            if (req == null) return NotFound(new { message = "Request not found" });
            req.Status = status;
            _context.SaveChanges();
            var returnRequest = req.ToHostRequestDto();

            return Ok(returnRequest);
        }

        /// <summary>
        /// Deletes a request
        /// </summary>
        ///  /// <param name="id">The id of the request</param>
        /// <returns>
        /// The deleted request
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _userService.GetCurrentUser(this);
            var req = _context.Requests
                .Include(r => r.Event).ThenInclude(e => e.Host)
                .Where(x => x.Id == id && x.Event.Host.Id == user.Id)
                .FirstOrDefault();
            if (req == null) return NotFound(new { message = "Request not found" });
            _context.Remove(req);
            _context.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Gets a request
        /// </summary>
        ///  /// <param name="id">The id of the request</param>
        /// <returns>
        /// the request with the specified id
        /// </returns>
        /// <remarks>
        /// Only returns the request if the request was made by the current user or the event is owned by the current user.
        /// </remarks>
        [HttpGet("{id}")]
        public IActionResult GetRequest(int id)
        {
            // Get current user
            var user = _userService.GetCurrentUser(this);

            // Get Request if host id or requester id matches
            var ev = _context.Requests
                .Include(r => r.Event).ThenInclude(e => e.Host)
                .Where(r => r.Id == id && (r.Event.Host.Id == user.Id || r.Requester.Id == user.Id)).FirstOrDefault();

            if (ev == null) return NotFound(new { message = "Request not found" });

            return Ok(ev);
        }

        /// <summary>
        /// Gets all request associated with the current user
        /// </summary>
        /// <returns>
        /// A list of requests all requests for the current user
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("me")]
        public IActionResult ListMyRequests()
        {
            // Get current user
            var user = _userService.GetCurrentUser(this);

            // Get requests
            var reqs = _context.Requests
                .Include(r => r.Event)
                .Include(r => r.Requester)
                .Where(x => x.Requester.Id == user.Id).ToList();
            if (reqs.Count < 1) return NotFound(new { message = "Request not found" });

            // TODO: Format return to only return needed data
            return Ok(reqs);
        }

        /// <summary>
        /// Gets all reqests for a given event
        /// </summary>
        ///  /// <param name="id">The event ID that you want to get all requests for</param>
        /// <returns>
        /// A list of requests associated with the coorisponding event
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("event/{id}")]
        public IActionResult ListEventRequests(int id)
        {
            // Get current user
            var user = _userService.GetCurrentUser(this);

            // Get requests as members of the requested event
            var ev = _context.Events
                .Include(e => e.Requests).ThenInclude(r => r.Requester)
                .Include(e => e.Host)
                .Where(e => e.Host.Id == user.Id && e.Id == id)
                .FirstOrDefault();
            if (ev == null) return NotFound(new { message = "Request not found" });

            List<HostRequestDto> returnRequests = new List<HostRequestDto>();
            foreach (Request r in ev.Requests) returnRequests.Add(r.ToHostRequestDto());

            return Ok(returnRequests);
        }
    }
}