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
        //TODO: Remove
        private static List<User> _users = new List<User>
        {
            new User{ Id = 1, Email = "admin@bountyboard.com", Password = "password", DisplayName = "Admin" }
        };

        //TODO: Remove
        private static List<RefreshToken> _refreshTokens = new List<RefreshToken>
        {
            new RefreshToken{ User = _users[0], Token = "1"}
        };

        private readonly AppSettings _appSettings;

        public AuthService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        /// <summary>method <c>Authenticate</c>Verifies a Username/Password set and returns new tokens.</summary>
        public TokenModel Authenticate(string email, string password)
        {
            // Placeholder until user database is running
            var user = _users.SingleOrDefault(x => x.Email == email && x.Password == password);
            if (user == null) return null;

            //TODO: add check for user
            //TODO: add password validation

            return GenerateJwt(user);
        }

        /// <summary>method <c>Refresh</c>Verifies a refresh token and returns renewed tokens.</summary>
        public TokenModel Refresh(string refreshToken)
        {
            //TODO: Check to see if refresh token is valid

            // Place holder until user database is running
            var valid = _refreshTokens.SingleOrDefault(x => x.Token == refreshToken);
            if (valid == null) return null;
            var user = _users.SingleOrDefault(x => x.Id == valid.User.Id);
            if (user == null) return null;

            return GenerateJwt(user);

        }

        /// <summary>method <c>GenerateJwt</c>Returns a new Jwt token and refresh token.</summary>
        private TokenModel GenerateJwt(User user)
        {
            var expires = DateTime.UtcNow.AddDays(7);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken { User = user, Token = Guid.NewGuid().ToString() };

            //TODO: store refresh token in DB

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
