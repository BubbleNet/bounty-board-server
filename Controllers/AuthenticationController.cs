using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BountyBoardServer.Model;
using BountyBoardServer.Services;
using Newtonsoft.Json;
using BountyBoardServer.Data;

namespace BountyBoardServer.Controllers
{
   
    [Route("auth/")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : Controller
    {
        private IAuthService _authService;
        private readonly BountyBoardContext _context;

        public AuthenticationController(IAuthService authService, BountyBoardContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        /// <summary>method <c>Authenticate</c>Returns a new Jwt token and refresh token.</summary>
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var token = _authService.Authenticate(model.Email, model.Password);

            if (token == null)
            {
                return BadRequest(new { message = "Username or Password is incorrect" });
            }

            return Ok(token);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        /// <summary>method <c>Refresh</c>Returns a new Jwt token and refresh token.</summary>
        public IActionResult Refresh([FromBody]RefreshModel model)
        {
            var token = _authService.Refresh(model.RefreshToken);

            if (token == null)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            return Ok(token);
        }
    }
}