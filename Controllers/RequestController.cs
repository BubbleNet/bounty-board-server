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

        [HttpPost("create")]
        /// <summary>method <c>Create</c>Creates a new Request</summary>
        public IActionResult Create([FromBody] int eventId, string description)
        {
            // Get event
            var ev = _context.Events
                .Include(h => h.Requests)
                .Include(p => p.Participants)
                .Where(e => e.Id == eventId)
                .FirstOrDefault();

            // Check if requests are allowed for event
            if (!ev.RequestsOpen) BadRequest(new { message = "This event is not taking requests right now." });

            // Get the current user
            /*var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            var user = _context.Users.Where(u => u.Id == userIdInt).FirstOrDefault();*/
            var user = _userService.GetCurrentUser(this);

            // If event is full OR if event requires a request, create a request
            if (!ev.RequestNeeded || ev.Participants.ToList().Count >= ev.MaxPlayers)
            {
                var req = new Request { Requester = user, Event = ev, Description = description };
                _context.Requests.Add(req);
                _context.SaveChanges();
            }
            // If requests aren't necessary, just add the user to the event.
            else
            {
                // Add user
                ev.Participants.Add(user);
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpGet("me")]
        /// <summary>method <c>ListMyRequests</c>Lists requests for the current user</summary>
        public IActionResult ListMyRequests()
        {
            // Get current user
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            var user = _context.Users.Where(u => u.Id == userIdInt).FirstOrDefault();

            // Get requests
            var reqs = _context.Requests
                .Include(r => r.Event)
                .Include(r => r.Requester)
                .Where(x => x.Requester.Id == user.Id).ToList();
            if (reqs.Count < 1) return NotFound(new { message = "Request not found" });

            // TODO: Format return to only return needed data
            return Ok(reqs);
        }

        [HttpPost("update")]
        /// <summary>method <c>Edit</c>Updates an existing request</summary>
        public IActionResult Update([FromBody]int eventId, string description)
        {
            // Get current user
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            var user = _context.Users.Where(u => u.Id == userIdInt).FirstOrDefault();

            // Get request
            var req = _context.Requests
                .Include(r => r.Event)
                .Include(r => r.Requester)
                .Where(x => x.Requester.Id == user.Id && x.Event.Id == eventId).FirstOrDefault();
            if (req == null) return NotFound(new { message = "Request not found" });
            req.Description = description;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        /// <summary>method <c>Delete</c>Deletes an existing request</summary>
        public IActionResult Delete(int id)
        {
            var req = _context.Requests.Where(x => x.Id == id).FirstOrDefault();
            if (req == null) return NotFound(new { message = "Request not found" });
            _context.Remove(req);
            _context.SaveChanges();
            return Ok();
        }
    }
}