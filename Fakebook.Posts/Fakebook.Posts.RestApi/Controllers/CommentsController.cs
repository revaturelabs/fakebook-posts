﻿using Fakebook.Posts.Domain.Interfaces;
using Fakebook.Posts.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fakebook.Posts.RestApi.Controllers {

    [Route("api/comments")]
    [ApiController]
    public class CommentsController : ControllerBase {

        private readonly IPostsRepository _postsRepository;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(IPostsRepository postsRepository, ILogger<CommentsController> logger) {
            _postsRepository = postsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Deletes the post resource with the given id.
        /// </summary>
        /// <param name="id">Id of the post to be deleted</param>
        /// <returns>An IActionResult containing either a:
        /// 204NoContent on success,
        /// 400BadRequest on delete failure,
        /// 404NotFound if the Id did not match an existing post,
        /// or 403Forbidden if the UserEmail on the original post does not match the email on the token of the request sender.</returns>
        [Authorize]
        [HttpDelete("{commentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int commentId) {
            try {
                var sessionEmail = User.FindFirst(ct => ct.Type.Contains("nameidentifier")).Value;
                var post = _postsRepository.AsQueryable().Include(x => x.Comments).First(p => p.Comments.Any(c => c.Id == commentId));
                var comment = post.Comments.First(c => c.Id == commentId);
                if (sessionEmail != post.UserEmail && sessionEmail != comment.UserEmail) {
                    return Forbid();
                }
            } catch (InvalidOperationException e) {
                _logger.LogInformation(e, $"Found no comment entry with Id: {commentId}.");
                return NotFound(e.Message);
            }

            try {
                await _postsRepository.DeleteCommentAsync(commentId);
            } catch (ArgumentException e) {
                _logger.LogInformation(e, $"Found no comment entry with id: {commentId}.");
                return NotFound(e.Message);
            } catch (DbUpdateException e) {
                _logger.LogInformation(e, "Tried to remove comment which resulted in a violation of a database constraint");
                return BadRequest(e.Message);
            }
            return NoContent();
        }
        
        /// Add a new comment to the database. 
        /// </summary>
        /// <param name="comment">
        /// A domain comment. 
        /// </param>
        /// <returns>
        /// The newly created comment
        /// </returns>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostAsync(Comment comment) {
            var email = User.FindFirst(ct => ct.Type.Contains("nameidentifier")).Value;

            if (email != comment.UserEmail) {
                _logger.LogInformation("Authenticated user email did not match user email of the comment.");
                return Forbid();
            }

            Comment created;
            try {
                created = await _postsRepository.AddCommentAsync(comment);
            } catch (ArgumentException e) {
                _logger.LogInformation(e, "Attempted to create a comment with invalid arguments.");
                return BadRequest(e.Message);
            } catch (DbUpdateException e) {
                _logger.LogInformation(e, "Attempted to create a comment which violated database constraints.");
                return BadRequest(e.Message);
            }

            return CreatedAtAction(nameof(GetAsync), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync(int id) {
            if (await _postsRepository.AsQueryable()
                .Include(x => x.Comments).FirstOrDefaultAsync(p => 
                    p.Comments.Any(c => c.Id == id)) is Post post)
            {
                var comment = post.Comments.First(c => c.Id == id);
                return Ok(comment);
            }
            return NotFound();
        }
    }
}
