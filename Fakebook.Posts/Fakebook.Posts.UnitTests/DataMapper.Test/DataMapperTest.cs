﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fakebook.Posts.Domain;
using Xunit;
using Fakebook.Posts.DataAccess.Mappers;

namespace Fakebook.Posts.UnitTests.DataMapper_Testing
{
    public class DataMapperTest
    {
        [Fact]
        public void DomainPostToDbPost()
        {
            //Arrange
            var domainPost = new Fakebook.Posts.Domain.Models.Post("person1@domain.net", "Content");
            domainPost.CreatedAt = DateTime.Now;

            var domainComment = new Fakebook.Posts.Domain.Models.Comment("person1@domain.net", "Comment Content");
            domainComment.Post = domainPost;
            domainComment.CreatedAt = DateTime.Now;
               
            domainPost.Comments.Add(domainComment);
            domainPost.Likes.Add("a@b.d");

            //Act
            var dbPost = domainPost.ToDataAccess();

            //Assert

            Assert.True(dbPost.UserEmail == domainPost.UserEmail);
            Assert.True(dbPost.Content == domainPost.Content);
            Assert.True(dbPost.CreatedAt == domainPost.CreatedAt);
            Assert.True(dbPost.Comments.Count == 1);
            Assert.True(dbPost.PostLikes.Count == 1);

        }

        [Fact]
        public void DbPostToDomainPost()
        {
            //Arrange
            var dbPost = new Fakebook.Posts.DataAccess.Models.Post
            {

                UserEmail = "person1@domain.net",
                Content = "Content",
                CreatedAt = DateTime.Now,
                Comments = new HashSet<Fakebook.Posts.DataAccess.Models.Comment>(),
                PostLikes = new HashSet<DataAccess.Models.PostLike>()

            };

            var dbComent = new Fakebook.Posts.DataAccess.Models.Comment
            {
                Content = "Comment Content",
                Post = dbPost,
                CreatedAt = DateTime.Now,
                UserEmail = "person2@domain.net",
            };

            dbPost.Comments.Add(dbComent);
            dbPost.PostLikes.Add(new DataAccess.Models.PostLike { LikerEmail = "a@b.d", Post = dbPost });

            //Act
            Fakebook.Posts.Domain.Models.Post domainPost = dbPost.ToDomain();

            //Assert
            Assert.True(dbPost.UserEmail == domainPost.UserEmail);
            Assert.True(dbPost.Content == domainPost.Content);
            Assert.True(dbPost.CreatedAt == domainPost.CreatedAt);
            Assert.True(dbPost.Comments.Count == 1);
            Assert.True(dbPost.PostLikes.Count == 1);
        }


        [Fact]
        public void DomainCommentToDbComment()
        {
            //Arrange
            var dbPost = new Fakebook.Posts.DataAccess.Models.Post
            {
                Id = 0,
                UserEmail = "person1@domain.net",
                Content = "Content",
                CreatedAt = DateTime.Now,
                Comments = new HashSet<Fakebook.Posts.DataAccess.Models.Comment>()
            };

            var domainComment = new Fakebook.Posts.Domain.Models.Comment("person1@domain.net", "Comment Content");
            domainComment.CreatedAt = DateTime.Now;
            domainComment.Likes.Add("a@b.d");

            //Act
            var dbComment = domainComment.ToDataAccess(dbPost);

            //Assert
            Assert.True(dbComment.Content == domainComment.Content);
            Assert.Equal(dbPost, dbComment.Post);
            Assert.True(dbComment.CreatedAt == domainComment.CreatedAt);
            Assert.True(dbComment.UserEmail == domainComment.UserEmail);
            Assert.True(dbComment.CommentLikes.Count == 1);
        }

        [Fact]
        public void DbCommentToDomainComment()
        {
            //Arrange
            var domainPost = new Fakebook.Posts.Domain.Models.Post("person1@domain.net", "Content");
            domainPost.CreatedAt = DateTime.Now;

            var dbComment = new Fakebook.Posts.DataAccess.Models.Comment
            {
                Content = "Comment Content",
                CreatedAt = DateTime.Now,
                UserEmail = "person2@domain.net",
                CommentLikes = new HashSet<DataAccess.Models.CommentLike>{ new DataAccess.Models.CommentLike { LikerEmail = "a@b.d" } }
            };

            //Act
            var domainComment = dbComment.ToDomain(domainPost);

            //Assert
            Assert.True(dbComment.Content == domainComment.Content);
            Assert.Equal(domainPost, domainComment.Post);
            Assert.True(dbComment.CreatedAt == domainComment.CreatedAt);
            Assert.True(dbComment.UserEmail == domainComment.UserEmail);
            Assert.True(domainComment.Likes.Count == 1);
        }

        [Fact] 
        public void DomainUsertoDbUser()
        {
            //Arrange
            var domainUser = new Fakebook.Posts.Domain.Models.Follow
            {
                FollowerEmail = "user@email.net",
                FollowedEmail = "followee@email.net"
            };

            //Act
            var dbUser = domainUser.ToDataAccess();

            //Assert
            Assert.True(domainUser.FollowerEmail == dbUser.FollowerEmail);
            Assert.True(domainUser.FollowedEmail == domainUser.FollowedEmail);
        }

        [Fact]
        public void DbUsertoDomainUser()
        {
            //Arrange
            var dbUser = new Fakebook.Posts.DataAccess.Models.Follow
            {
                FollowerEmail = "user@email.net",
                FollowedEmail = "followee@email.net"
            };

            //Act
            var domainUser = dbUser.ToDomain();

            //Assert
            Assert.True(dbUser.FollowerEmail == domainUser.FollowerEmail);
            Assert.True(dbUser.FollowedEmail == domainUser.FollowedEmail);
        }
    }
}
