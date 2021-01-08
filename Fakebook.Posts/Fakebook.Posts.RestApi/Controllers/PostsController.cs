﻿using Fakebook.Posts.Domain.Interfaces;
using Fakebook.Posts.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Fakebook.Posts.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase {

        private readonly IPostsRepository _postsRepository;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IPostsRepository postsRepository, ILogger<PostsController> logger) {
            _postsRepository = postsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Updates the properties of the resource at the given id to be those contained within the given post object.
        /// </summary>
        /// <param name="id">The Id of the post to be updated.</param>
        /// <param name="post">A post object containing the properties which are to be updated.</param>
        /// <returns>An IActionResult containing either a:
        /// 204NoContent on success,
        /// 400BadRequest on update failure,
        /// 404NotFound if the Id did not match an existing post,
        /// or 403Forbidden if the UserEmail on the original post does not match the email on the token of the request sender.</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, Post post) {
            try {
                var email = User.FindFirst(ct => ct.Type.Contains("nameidentifier")).Value;
                var currentPost = _postsRepository.First(p => p.Id == id);

                if (email != currentPost.UserEmail) {
                    return Forbid();
                }
            } catch (InvalidOperationException e) {
                _logger.LogInformation(e, $"Found no post entry with Id: {id}");
                return NotFound(e.Message);
            }

            try {
                post.Id = id;
                await _postsRepository.UpdateAsync(post);
            } catch (ArgumentException e) {
                _logger.LogInformation(e, $"Found no post entry with Id: {id}");
                return NotFound(e.Message);
            } catch (DbUpdateException e) {
                _logger.LogInformation(e, "Attempted to update a post which resulted in a violation of database constraints.");
                return BadRequest(e.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Adds a new post to the database.
        /// </summary>
        /// <param name="postModel">The post object to be added.</param>
        /// <returns>201Created on successful add, 400BadRequest on failure, 403Forbidden if post UserEmail does not match the email of the session token.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostAsync(Post postModel) {
            var email = User.FindFirst(ct => ct.Type.Contains("nameidentifier")).Value; // Get user email from session.

            if (email != postModel.UserEmail) {
                _logger.LogInformation("Authenticated user email did not match user email of the post.");
                return Forbid();
            }

            Post created;
            try {
                created = await _postsRepository.AddAsync(postModel);
            } catch (ArgumentException e) {
                _logger.LogInformation(e, "Attempted to create a post with invalid arguments.");
                return BadRequest(e.Message);
            } catch (DbUpdateException e) {
                _logger.LogInformation(e, "Attempted to create a post which violated database constraints.");
                return BadRequest(e.Message);
            }

            return CreatedAtAction(nameof(GetAsync), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id) {
            throw new NotImplementedException();
        }
    }
}
