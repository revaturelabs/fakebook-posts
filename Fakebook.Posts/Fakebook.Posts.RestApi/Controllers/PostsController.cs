﻿using Fakebook.Posts.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fakebook.Posts.RestApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase {
        public PostsController() {

        }

        // [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(Post postModel) {
            throw new NotImplementedException();
        }
    }
}
