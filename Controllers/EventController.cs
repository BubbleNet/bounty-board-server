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

namespace BountyBoardServer.Controllers
{
    [Route("events/")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly BountyBoardContext _context;

        public EventController(BountyBoardContext context)
        {
            _context = context;
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

        [HttpGet("me")]
        public IActionResult ListMyEvents()
        {
            // Get the creating user
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            // var user = _context.Users.Where(x => x.Id == userIdInt).FirstOrDefault();

            // Get events for that user
            var events = _context.Events.Where(x => x.Host.Id == userIdInt);

            // TODO: Format event data
            return Ok(events);
        }
    }
}