using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Globalization;
using BountyBoardServer.Data;

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


        [HttpPost("create")]
        [AllowAnonymous]
        public IActionResult Create(string email, string password)
        {
            // Verify valid email format
            if (!IsValidEmail(email)) return BadRequest(new { message = "Invalid email address" });

            // Verify password requirements
            var problems = VerifyPasswordStrength(password);
            if (problems.Count >  0)
            {
                return BadRequest(new { message = problems });
            }

            // Check if email already exists
            // TODO: Check DB for email

            // Create user
            // TODO: Create user in db

            return Ok();
        }

        [HttpGet("list")]
        public IActionResult List()
        {
            var users = _context.Users.ToList();

            return Ok(users);
        }
        
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }

            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static List<string> VerifyPasswordStrength(string password)
        {
            List<string> problemsList = new List<string>();
            if (password.Length < 8) problemsList.Add("Password must be 8 or more characters.");

            if (!Regex.Match(password, @"/\d+/", RegexOptions.ECMAScript).Success)
                problemsList.Add("Password must contain at least one number.");

            if (!(Regex.Match(password, @"/[a-z]/", RegexOptions.ECMAScript).Success &&
                Regex.Match(password, @"/[A-Z]/", RegexOptions.ECMAScript).Success))
                problemsList.Add("Password must contain at least one upper-case and one lower-case letter.");

            if (!Regex.Match(password, @"/.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]/",
                RegexOptions.ECMAScript).Success)
                problemsList.Add("Password must contain at least one symbol.");

            return problemsList;
        }
    }
}