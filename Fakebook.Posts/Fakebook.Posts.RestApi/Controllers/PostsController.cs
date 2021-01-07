﻿﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Fakebook.Posts.Domain.Interfaces;
using Fakebook.Posts.Domain.Models;

namespace Fakebook.Posts.RestApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase {

        private readonly IPostsRepository _postsRepository;

        public PostsController(IPostsRepository postsRepository) {
            _postsRepository = postsRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostAsync(Post postModel) {
            // var email = User.FindFirst(ct => ct.Type.Contains("nameidentifier")).Value; // Get user email from session.
            // verify user creating the post here...
            Post created;
            try {
                created = await _postsRepository.AddAsync(postModel);
            } catch (ArgumentException e) { // Attempted to create a post with invalid arguments.
                return BadRequest(e.Message);
            } catch (DbUpdateException e) { // Attempted to create a post which violated database constraints.
                return BadRequest(e.Message);
            }

            return CreatedAtAction(nameof(GetAsync), new { id = created.Id }, created);
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
