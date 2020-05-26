using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BountyBoardServer.Data;
using BountyBoardServer.Services;
using BountyBoardServer.Entities;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Differencing;
using System.Runtime.InteropServices.ComTypes;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using BountyBoardServer.Models.Dtos;
using System.Globalization;

namespace BountyBoardServer.Controllers
{
    [Route("locations")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly BountyBoardContext _context;
        private IUserService _userService;

        public LocationController(BountyBoardContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// Create a new Store
        /// </summary>
        /// <returns>
        /// The store if it was created
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Administrator, StoreOwner")]
        public IActionResult Create([FromBody] Entities.Location locationModel)
        {
            // Validate required params
            (bool valid, BadRequestObjectResult errors) = validate(locationModel);
            if (!valid) return errors;


            // Create a new admin list and add the store owner
            var user = _userService.GetCurrentUser(this);
            locationModel.Admins = new List<User>() { user };

            locationModel.GeoLocation = geocode(locationModel);

            // Initialize Hours
            locationModel.Hours = new List<Schedule>();

            _context.Locations.Add(locationModel);
            _context.SaveChanges();
            return Ok(locationModel.ToPrivateLocationDto());
        }

        /// <summary>
        /// Update an existing Location
        /// </summary>
        /// <returns>
        /// The location if it was updated
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator, StoreOwner")]
        public IActionResult Edit(int id, [FromBody] Entities.Location locationModel)
        {
            // Check permissions
            (bool perm, NotFoundObjectResult error) = hasPermission(id);
            if (!perm) return error;

            // Validate required params
            (bool valid, BadRequestObjectResult errors) = validate(locationModel);
            if (!valid) return errors;

            // Get current Location
            var location = _context.Locations
                .Include(l => l.Hours)
                .Include(l => l.Admins)
                .Where(l => l.Id == id).FirstOrDefault();
            if (location == null) return NotFound(new { message = "Location not found" });

            // Update location
            location.Update(locationModel);
            // Geocode new address
            location.GeoLocation = geocode(location);

            _context.SaveChanges();
            return Ok(location.ToPrivateLocationDto());
        }

        /// <summary>
        /// Gets a location
        /// </summary>
        /// <returns>
        /// The location if it exists
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var location = _context.Locations
               .Include(l => l.Hours)
               .Include(l => l.Admins)
               .Where(l => l.Id == id).FirstOrDefault();
            if (location == null) return NotFound(new { message = "Location not found" });

            // If site Admin or admin of this location, return the private details
            (bool perm, NotFoundObjectResult error) = hasPermission(id);
            if (perm) return Ok(location.ToPrivateLocationDto());

            // If not return the public details (location admins not included)
            return Ok(location.ToPublicLocationDto());
        }

        /// <summary>
        ///  Gets a list of Locations
        /// </summary>
        /// <returns>
        /// Returns the list of locations
        /// </returns>
        /// <remarks>
        /// ONLY returns public location models
        /// TODO: Filter by distance
        /// </remarks>
        [HttpGet("list")]
        public IActionResult List(string name, Double lat, Double lon, int distance)
        {
            IQueryable<Entities.Location> query = _context.Locations.Include(l => l.Hours);

            if (!string.IsNullOrEmpty(name)) query = query.Where(l => l.Name == name);

            // TODO: Filter by distance from lat/lon point

            var locations = query.ToList();
            if (locations.Count() < 1) return NotFound(new { message = "Location not found" });

            List<PublicLocationDto> returnLocations = new List<PublicLocationDto>();
            foreach (Entities.Location l in locations) returnLocations.Add(l.ToPublicLocationDto());

            return Ok(returnLocations);
        }

        /// <summary>
        ///  Delete a location
        /// </summary>
        /// <returns>
        /// Returns Ok if deleted
        /// </returns>
        /// <remarks>
        ///
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator, StoreOwner")]
        public IActionResult Delete(int id)
        {

            var location = _context.Locations.Where(l => l.Id == id).FirstOrDefault();
            if (location == null) return NotFound(new { message = "Location not found" });

            (bool perm, NotFoundObjectResult error) = hasPermission(id);
            if (perm)
            {
                _context.Locations.Remove(location);
                _context.SaveChanges();
            }
            return error;
        }

        /// <summary>
        ///  Create a Schedule
        /// </summary>
        /// <returns>
        /// Returns the new schedule
        /// </returns>
        /// <remarks>
        /// Replaces any existing schedule
        /// </remarks>
        [HttpPost("{id}/schedule")]
        [Authorize(Roles = "Administrator, StoreOwner")]
        public IActionResult CreateSchedule(int id, [FromBody] Schedule[] scheduleList)
        {
            // Check permissions
            (bool perm, NotFoundObjectResult error) = hasPermission(id);
            if (!perm) return error;

            // Get location
            var location = _context.Locations
                .Include(s => s.Hours)
                .Where(l => l.Id == id).FirstOrDefault();
            if (location == null) return NotFound(new { message = "Location not found" });

            // Clear the current Schedule
            location.Hours = new List<Schedule>();
            _context.SaveChanges();

            // Check if this exact configuration already exists
            foreach(Schedule s in scheduleList)
            {
                location.Hours.Add(s);
                
            }
            _context.SaveChanges();

            return Ok(location.ToPrivateLocationDto().Hours);
        }

        /// <summary>
        ///  Delete a schedule
        /// </summary>
        /// <returns>
        /// Ok if deleted
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpDelete("{id}/schedule")]
        [Authorize(Roles = "Administrator, StoreOwner")]
        public IActionResult DeleteSchedule(int id)
        {
            // Check permissions
            (bool perm, NotFoundObjectResult error) = hasPermission(id);
            if (!perm) return error;

            // Get location
            var location = _context.Locations
                .Include(s => s.Hours)
                .Where(l => l.Id == id).FirstOrDefault();
            if (location == null) return NotFound(new { message = "Location not found" });

            // Clear the current Schedule
            location.Hours = new List<Schedule>();
            _context.SaveChanges();
            return Ok();
        }

        /// <summary>
        ///  Checks the current user's permissions based on their assigned roles
        /// </summary>
        /// <returns>
        /// Returns true if the current user has permission to edit the resource
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        private (bool, NotFoundObjectResult) hasPermission(int locationId)
        {
            // If user is not admin, ensure user has permission to edit location
            var user = _userService.GetCurrentUser(this);
            if (!user.IsRole("Administrator"))
            {
                var location = _context.Locations.Include(l => l.Admins).Where(l => l.Id == locationId).FirstOrDefault();
                if (location != null) {
                    foreach (User u in location.Admins) if (u.Id == user.Id) return (true, null);
                    return (false, null);
                }
                return (false, NotFound(new { message = "Location not found" }));

            }
            return (true, null);
        }

        /// <summary>
        ///  validates a location
        /// </summary>
        /// <returns>
        /// bool true if valid, errors if invalid
        /// </returns>
        /// <remarks>
        ///
        /// </remarks>
        private (bool, BadRequestObjectResult) validate(Entities.Location location)
        {
            var errorList = new List<string>();
            if (string.IsNullOrEmpty(location.Name)) errorList.Add("Name is required");
            if (string.IsNullOrEmpty(location.StreetNumber)) errorList.Add("Street Number is required");
            if (string.IsNullOrEmpty(location.StreetName)) errorList.Add("Street Name is required");
            if (string.IsNullOrEmpty(location.City)) errorList.Add("City is required");
            if (string.IsNullOrEmpty(location.State)) errorList.Add("State is required");
            if (string.IsNullOrEmpty(location.ZipCode)) errorList.Add("Zip Code is required");
            if (string.IsNullOrEmpty(location.Country)) errorList.Add("Country is required");

            if (errorList.Count() < 1) return (true, null);
            else return (false, BadRequest(new { message = string.Join(", \n", errorList) }));
        }

        /// <summary>
        ///  Generates a lat/lon point from an address
        /// </summary>
        /// <returns>
        /// returns a lat lon point
        /// </returns>
        /// <remarks>
        /// TODO: Complete this
        /// </remarks>
        private Point geocode(Entities.Location location)
        {
            return null;
        }
    }
}