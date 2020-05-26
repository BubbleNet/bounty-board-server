using BountyBoardServer.Data;
using BountyBoardServer.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BountyBoardServer.Helpers
{
    public class UserHelper
    {
        private readonly BountyBoardContext _context;

        public UserHelper(BountyBoardContext context)
        {
            _context = context;
        }

        /// <summary>method <c>IsValidEmail</c>Verifies that the provided email is in proper
        /// email format</summary>
        public static bool IsValidEmail(string email)
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

        /// <summary>method <c>VerifyPasswordStrength</c>Verifies that the user's new password
        /// is within strength constraints</summary>
        public static List<string> VerifyPasswordStrength(string password)
        {
            List<string> problemsList = new List<string>();
            if (password.Length < 8) problemsList.Add("Password must be 8 or more characters.");

            if (!password.Any(char.IsDigit))
                problemsList.Add("Password must contain at least one number.");

            if (!(password.Any(char.IsUpper) && password.Any(char.IsLower)))
                problemsList.Add("Password must contain at least one upper-case and one lower-case letter.");

            var input = password;
            Regex rgx = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
            if (!rgx.IsMatch(input))
                problemsList.Add("Password must contain at least one of the following symbols " +
                    "!,@,#,$,%,^,&,*,?,_,~,-,£,(,)");

            return problemsList;
        }

        /// <summary>method <c>Hash</c>Hashes a password using the user's existing salt</summary>
        public static string Hash(string password, byte[] salt)
        {
            try
            {
                return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            
        }

        /// <summary>method <c>GetAge</c>Calculates the user's current age</summary>
        public static int GetAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;

            return age;

        }
    }


}
