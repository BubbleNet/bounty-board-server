using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BountyBoardServer.Entities;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BountyBoardServer.Helpers;
using System.IdentityModel.Tokens.Jwt;
using BountyBoardServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BountyBoardServer.Services
{
    public interface IAuthService 
    {
        TokenModel Authenticate(string email, string password);

        TokenModel Refresh(string refreshToken);
    }

    /// <summary>Class <c>AuthService</c> Handles logic related to verifying credentials and authorizing access
    ///via Jwt and refresh tokens.</summary>
    ///
    public class AuthService : IAuthService
    {

        private readonly AppSettings _appSettings;
        private readonly BountyBoardContext _context;

        public AuthService(IOptions<AppSettings> appSettings, BountyBoardContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        /// <summary>method <c>Authenticate</c>Verifies a Username/Password set and returns new tokens.</summary>
        public TokenModel Authenticate(string email, string password)
        {
            // Check to see if user exists
            var user = _context.Users.Include(u => u.Roles).Where(x => x.Email == email).FirstOrDefault();
            if (user == null) return null;

            //TODO: check if hashed passwords are equal
            var hashedPassword = UserHelper.Hash(password, user.Salt);
            if (!(hashedPassword == user.Password)) return null;

            return GenerateJwt(user);
        }

        /// <summary>method <c>Refresh</c>Verifies a refresh token and returns renewed tokens.</summary>
        public TokenModel Refresh(string refreshToken)
        {
            var l = _context.RefreshTokens.ToList();
            // Check for passed token
            if (String.IsNullOrEmpty(refreshToken)) return null;
            // Check if refresh token is valid
            var valid = _context.RefreshTokens.Include(t => t.User).Where(x => x.Token == refreshToken).FirstOrDefault();
            if (valid == null) return null;
            // Get user associated with refresh token
            var user = _context.Users.Where(x => x.Id == valid.User.Id).FirstOrDefault();
            if (user == null) return null;
            // Remove old token
            _context.Remove(valid);
            _context.SaveChanges();
            // Reauthenticate user
            return GenerateJwt(user);
        }

        /// <summary>method <c>GenerateJwt</c>Returns a new Jwt token and refresh token.</summary>
        private TokenModel GenerateJwt(User user)
        {
            var expires = DateTime.UtcNow.AddDays(7);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            List<Claim> claimList = new List<Claim>();
            foreach(Role r in user.Roles)
            {
                claimList.Add(new Claim(ClaimTypes.Role, r.Name));
            }
            claimList.Add(new Claim(ClaimTypes.Name, user.Id.ToString()));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimList),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken { User = user, Token = Guid.NewGuid().ToString() };

            // Store refresh token in DB
            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();

            var newToken = new TokenModel
            { 
                AccessToken = tokenHandler.WriteToken(token), 
                AccessTokenExpiration = expires, 
                RefreshToken = refreshToken.Token
            };
            return newToken;
        }
    }
}
