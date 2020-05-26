using BountyBoardServer.Data;
using BountyBoardServer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace BountyBoardServer.Services
{
    public interface IUserService
    {
        User GetCurrentUser(ControllerBase controller);
    }

    /// <summary>Class <c>UserService</c> Provides the ability to get the current user as a service
    /// to allow method reuse.</summary>
    /// TODO: This should probably be a helper instead of a service.
    public class UserService : IUserService
    {
        private readonly BountyBoardContext _context;

        public UserService(BountyBoardContext context)
        {
            _context = context;
        }

        public User GetCurrentUser(ControllerBase controller)
        {
            var claimsIdentity = controller.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var userIdInt = int.Parse(userId);
            var user = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Id == userIdInt)
                .FirstOrDefault();
            return user;
        }
    }
}
