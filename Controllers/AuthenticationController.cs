using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BountyBoardServer.Model;
using BountyBoardServer.Services;
using Newtonsoft.Json;

namespace BountyBoardServer.Controllers
{
   
    [Route("auth/")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : Controller
    {
        private IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
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