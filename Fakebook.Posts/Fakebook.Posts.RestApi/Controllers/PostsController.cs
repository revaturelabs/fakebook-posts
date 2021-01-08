﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

using Fakebook.Posts.Domain.Interfaces;
using Fakebook.Posts.Domain.Models;


namespace Fakebook.Posts.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {

        private readonly IPostsRepository _postsRepository;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IPostsRepository postsRepository, ILogger<PostsController> logger) {
            _postsRepository = postsRepository;
            _logger = logger;
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
            return BadRequest();
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync(int id) {
            if (await _postsRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == id) is Post post)
            {
                return Ok(post);
            }
            return NotFound();
        }
    }
}
