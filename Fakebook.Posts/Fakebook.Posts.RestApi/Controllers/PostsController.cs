﻿using Fakebook.Posts.Domain;
using Fakebook.Posts.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fakebook.Posts.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {

        public PostsController(IPostsRepository postsRepository)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(Post postModel)
        {
            throw new NotImplementedException();
        }

    }
}
