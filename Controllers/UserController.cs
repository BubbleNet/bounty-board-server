using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BountyBoardServer.Data;
using BountyBoardServer.Services;
using BountyBoardServer.Entities;
using System.Security.Cryptography;
using System.Security.Claims;
using BountyBoardServer.Model;
using Microsoft.EntityFrameworkCore;

namespace BountyBoardServer.Controllers
{
    [Route("users/")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly BountyBoardContext _context;

        public UserController(BountyBoardContext context)
        {
            _context = context;
        }


        [HttpPost("new")]
        [AllowAnonymous]
        /// <summary>method <c>Create</c>Creates a new user.</summary>
        public IActionResult Create([FromBody]AuthenticateModel model)
        {
            // Verify valid email format
            if (!UserService.IsValidEmail(model.Email)) return BadRequest(new { message = "Invalid email address" });

            // Verify password requirements
            var problems = UserService.VerifyPasswordStrength(model.Password);
            if (problems.Count >  0)
            {
                return BadRequest(new { message = problems });
            }

            // Check if email already exists
            var existingEmail = _context.Users.Where(x => EF.Functions.Like(x.Email, $"%{model.Email}%"));
            if (existingEmail.Count() > 0)
                return BadRequest(new { message = "Email address is already associated with an account." });

            // Create user
            // generate 120-bit salt
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            // Create hashed pw
            string hashed = UserService.Hash(model.Password, salt);
            
            // Store hashed pw and salt
            var user = new User { Email = model.Email, Password = hashed, Salt = salt };
            _context.Users.Add(user);
            _context.SaveChanges();


            return Ok();
        }

        [HttpGet("{id}")]
        /// <summary>method <c>Get</c>Gets user infornmation by <c>id</c></summary>
        public IActionResult Get(int id)
        {
            var user = _context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (user == null) return BadRequest(new { message = "User not found" });
            var age = UserService.GetAge(user.DateOfBirth);
            return Ok(new { user.Id, user.DisplayName, age });
        }

        [HttpGet("me")]
        /// <summary>method <c>GetMyInfo</c>Gets user infornmation for the logged in user</summary>
        public IActionResult GetMyInfo()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);

            var user = _context.Users.Where(x => x.Id == userIdInt).FirstOrDefault();
            var age = UserService.GetAge(user.DateOfBirth);
            return Ok(new { user.Id, user.DisplayName, user.Email, age });

        }

        [HttpPost("search")]
        /// <summary>method <c>Search</c>Searches for a user by <c>email</c></summary>
        public IActionResult Search([FromBody]string email)
        {
            var valid = UserService.IsValidEmail(email);
            if (!valid) return BadRequest(new { message = "Not a valid email address" });

            var user = _context.Users.Where(x => x.Email == email).FirstOrDefault();
            var age = UserService.GetAge(user.DateOfBirth);
            return Ok(new { user.Id, user.DisplayName, user.Email, age });

        }
    }
}