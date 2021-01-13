﻿using Fakebook.Posts.Domain.Interfaces;
using Fakebook.Posts.Domain.Models;
using Fakebook.Posts.RestApi.Controllers;
using Fakebook.Posts.RestApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fakebook.Posts.UnitTests.Controllers {
    public class DeletePostTests {

        /// <summary>
        /// Tests the PostsController class' DeleteAsync method. Ensures that a proper id results in status 204NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ValidId_Deletes() {

            // Arrange
            var mockRepo = new Mock<IPostsRepository>();

            mockRepo.Setup(r => r.DeletePostAsync(It.IsAny<int>()))
                .Returns(new ValueTask());

            var controller = new PostsController(mockRepo.Object, new Mock<IFollowsRepository>().Object, new Mock<IBlobService>().Object, new NullLogger<PostsController>());

            // Act
            var actionResult = await controller.DeleteAsync(1);

            // Assert
            var result = Assert.IsAssignableFrom<NoContentResult>(actionResult);
        }

        /// <summary>
        /// Tests the PostsController class' PutAsync method. Ensures that an improper Post object results in status 400BadRequest with an error message in the body.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_InvalidId_NotFound() {
            
            // Arrange
            var mockRepo = new Mock<IPostsRepository>();

            mockRepo.Setup(r => r.DeletePostAsync(It.IsAny<int>()))
                .Throws(new ArgumentException());

            var controller = new PostsController(mockRepo.Object, new Mock<IFollowsRepository>().Object, new Mock<IBlobService>().Object, new NullLogger<PostsController>());

            // Act
            var actionResult = await controller.DeleteAsync(1);

            // Assert
            var result = Assert.IsAssignableFrom<NotFoundObjectResult>(actionResult);
        }
    }
}