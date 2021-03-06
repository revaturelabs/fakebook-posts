using Fakebook.Posts.DataAccess.Mappers;
using Fakebook.Posts.Domain.Interfaces;
using Fakebook.Posts.Domain.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using Fakebook.Posts.DataAccess.Models;

namespace Fakebook.Posts.DataAccess.Repositories
{
    public class PostsRepository : IPostsRepository
    {
        private readonly FakebookPostsContext _context;

        public PostsRepository(FakebookPostsContext context) {
            _context = context;
        }
        public async Task<IEnumerable<Post>> NewsfeedAsync(string email, int count)
        {
            var posts = await _context.Posts.FromSqlInterpolated(
                $"SELECT * FROM ( SELECT *, ROW_NUMBER() OVER ( PARTITION BY \"UserEmail\" ORDER BY \"CreatedAt\" DESC ) AS \"RowNum\" FROM \"Fakebook\".\"Post\" WHERE \"UserEmail\" = {email} OR \"UserEmail\" IN ( SELECT \"FollowedEmail\" FROM \"Fakebook\".\"UserFollows\" WHERE \"FollowerEmail\" = {email} ) ) AS \"RecentPosts\" WHERE \"RecentPosts\".\"RowNum\" <= {count}"
            ).ToListAsync();
            return posts.Select(p => p.ToDomain());
        }
/*
SELECT *
FROM (
    SELECT *, 
    ROW_NUMBER() OVER (
        PARTITION BY "UserEmail" 
        ORDER BY "CreatedAt" DESC
    ) AS "RowNum"
    FROM "Fakebook"."Post"
    WHERE "UserEmail" = @email
    OR "UserEmail" IN (
        SELECT "FollowedEmail" 
        FROM "Fakebook"."UserFollows" 
        WHERE "FollowerEmail" = @email
    )
) AS "RecentPosts"
WHERE "RecentPosts"."RowNum" <= @count
*/

        public async ValueTask<Fakebook.Posts.Domain.Models.Post> AddAsync(Fakebook.Posts.Domain.Models.Post post)
        {
            var postDb = post.ToDataAccess();
            await _context.Posts.AddAsync(postDb);
            await _context.SaveChangesAsync();
            return postDb.ToDomain();
        }
        public async ValueTask<Fakebook.Posts.Domain.Models.Comment> AddCommentAsync(Fakebook.Posts.Domain.Models.Comment comment)
        {
            if (await _context.Posts.FirstOrDefaultAsync(p => p.Id == comment.Post.Id) is DataAccess.Models.Post post)
            {
                var commentDb = comment.ToDataAccess(post);
                await _context.Comments.AddAsync(commentDb);
                await _context.SaveChangesAsync();
                return commentDb.ToDomain(null);
            }
            else
            {
                throw new ArgumentException($"Post { comment.Post.Id } not found.", nameof(comment.Post.Id));
            }

        }

        public async ValueTask DeletePostAsync(int id) {
            if (await _context.Posts.FindAsync(id) is DataAccess.Models.Post post) {
                _context.Remove(post);
                await _context.SaveChangesAsync();
            } else {
                throw new ArgumentException("Post with given id not found.", nameof(id));
            }
        }

        public async ValueTask DeleteCommentAsync(int id) {
            if (await _context.Comments.FindAsync(id) is DataAccess.Models.Comment comment) {
                _context.Remove(comment);
                await _context.SaveChangesAsync();
            } else {
                throw new ArgumentException("Comment with given id not found.", nameof(id));
            }
        }

        /// <summary>
        /// Updates the content property of the given post in the database. Db column remains unchanged if property value is null.
        /// </summary>
        /// <param name="post">The domain post model containing the updated property values.</param>
        /// <exception cref="ArgumentException">ArgumentException</exception>
        public async ValueTask UpdateAsync(Domain.Models.Post post) {
            if (await _context.Posts.FindAsync(post.Id) is DataAccess.Models.Post current) {
                current.Content = post.Content ?? current.Content;

                await _context.SaveChangesAsync(); // Will throw DbUpdateException if a database constraint is violated.
            } else {
                throw new ArgumentException("Post with given Id not found.", nameof(post.Id));
            }
        }

        /// <summary>
        ///  Returns an enumerator that iterates asynchronously through the collection.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>
        /// An enumerator that can be used to iterate asynchronously through the collection, 
        /// where Posts do NOT contain their comments 
        /// </returns>
        public IAsyncEnumerator<Post> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => _context.Posts.Include(p => p.PostLikes).Select(x => x.ToDomain()).AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection,
        /// where Posts do NOT contain their comments.
        /// </returns>
        public IEnumerator<Post> GetEnumerator()
            => _context.Posts.Include(p => p.PostLikes).Select(x => x.ToDomain()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); 

        public async Task<bool> LikePostAsync(int postId, string userEmail)
        {
            try {
                await _context.PostLikes.AddAsync(new Models.PostLike { PostId = postId, LikerEmail = userEmail });
                await _context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> UnlikePostAsync(int postId, string userEmail)
        {
            if (await _context.PostLikes.FindAsync(userEmail, postId) is Models.PostLike like)
            {
                _context.PostLikes.Remove(like);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> LikeCommentAsync(int commentId, string userEmail)
        {
            try {
                await _context.CommentLikes.AddAsync(new Models.CommentLike { CommentId = commentId, LikerEmail = userEmail });
                await _context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> UnlikeCommentAsync(int commentId, string userEmail)
        {
            if (await _context.CommentLikes.FindAsync(userEmail, commentId) is Models.CommentLike like)
            {
                _context.CommentLikes.Remove(like);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
