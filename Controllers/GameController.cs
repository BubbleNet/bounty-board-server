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

namespace BountyBoardServer.Controllers
{
    [Route("games/")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class GameController : ControllerBase
    {
        private readonly BountyBoardContext _context;


        public GameController(BountyBoardContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <returns>
        /// The game if it was created
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost]
        public IActionResult Create([FromBody] Game gameModel)
        {
            var game = _context.Games.Where(g => g.Name == gameModel.Name).FirstOrDefault();
            if (game != null) return BadRequest(new { message = "Game already exists" });
            if (string.IsNullOrEmpty(gameModel.Name)) return BadRequest(new { message = "Game must have name" });
            _context.Add(gameModel);
            _context.SaveChanges();
            return Ok(gameModel);
        }

        /// <summary>
        /// Get a game
        /// </summary>
        /// <param name="id">The id of the game to retrieve</param>
        /// <returns>
        /// The game with the requested Id
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var game = _context.Games.Include(g => g.Editions).Where(g => g.Id == id).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });
            return Ok(game);
        }

        /// <summary>
        /// Get the list of all available games
        /// </summary>
        /// <returns>
        /// All games
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet]
        public IActionResult List()
        {
            var games = _context.Games.ToList();
            return Ok(games);
        }

        /// <summary>
        /// Update an existing game
        /// </summary>
        /// <param name="id">The id of the game to update</param>
        /// <returns>
        /// The game if it was updated
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Game gameModel)
        {
            var game = _context.Games.Where(g => g.Id == id).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });
            if (string.IsNullOrEmpty(gameModel.Name)) return BadRequest(new { message = "Game must have name" });
            game.Name = gameModel.Name;
            game.Publisher = gameModel.Publisher;
            game.Description = gameModel.Description;
            _context.SaveChanges();
            return Ok(game);
        }

        /// <summary>
        /// Delete a game
        /// </summary>
        /// <param name="id">The id of the game</param>
        /// <returns>
        /// Ok if game was deleted
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var game = _context.Games.Where(g => g.Id == id).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });
            _context.Remove(game);
            _context.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Adds an edition to a game
        /// </summary>
        /// <param name="id">The id of the game</param>
        /// <returns>
        /// The edition if it was added
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPost("{id}/editions")]
        public IActionResult CreateEdition(int id, [FromBody] Edition editionModel)
        {
            // Confirm Edition has name
            if (string.IsNullOrEmpty(editionModel.Name)) return BadRequest(new { message = "Edition must have name" });

            // Make sure Game exists
            var game = _context.Games.Include(g => g.Editions).Where(g => g.Id == id).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });

            // Make sure edition does not already exist
            foreach (Edition e in game.Editions) if (editionModel.Name == e.Name) return BadRequest(new { message = "Edition already exists" });

            // Add the new edition
            var edition = new Edition { Name = editionModel.Name, Description = editionModel.Description };
            game.Editions.Add(edition);
            _context.SaveChanges();

            return Ok(edition);
        }

        /// <summary>
        /// Updates an edition
        /// </summary>
        /// <param name="id">The id of the game</param>
        /// <param name="editionId">The id of the edition</param>
        /// <returns>
        /// The modified edition
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpPatch("{id}/editions/{editionId}")]
        public IActionResult EditEdition(int id, int editionId, [FromBody] Edition editionModel)
        {
            // Confirm Edition has name
            if (string.IsNullOrEmpty(editionModel.Name)) return BadRequest(new { message = "Edition must have name" });

            // Make sure Game exists
            var game = _context.Games.Include(g => g.Editions).Where(g => g.Id == id).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });

            // Make sure edition exists
            foreach (Edition e in game.Editions) if (editionId == e.Id)
                {
                    e.Name = editionModel.Name;
                    e.Description = editionModel.Description;
                    _context.SaveChanges();
                    return Ok(e);
                }
            return BadRequest(new { message = "Edition does not exist" });
        }

        /// <summary>
        /// Delete an edition from a game
        /// </summary>
        /// <param name="id">The id of the game</param>
        /// <param name="editionId">The id of the edition</param>
        /// <returns>
        /// Ok if the edition was deleted
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpDelete("{id}/editions/{editionId}")]
        public IActionResult DeleteEdition(int id, int editionId)
        {
            // Make sure Game exists
            var game = _context.Games.Include(g => g.Editions).Where(g => g.Id == id).FirstOrDefault();
            if (game == null) return BadRequest(new { message = "Game not found" });

            // Make sure edition exists
            foreach (Edition e in game.Editions) if (editionId == e.Id)
                {
                    _context.Remove(e);
                    _context.SaveChanges();
                    return Ok();
                }
            return BadRequest(new { message = "Edition does not exist" });
        }
    }
}