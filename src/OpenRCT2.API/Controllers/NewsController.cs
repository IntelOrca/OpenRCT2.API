using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Authentication;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.Models.Requests;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Controllers
{
    [Route("news")]
    [Authorize(Roles = UserRole.Administrator)]
    public class NewsController : Controller
    {
        private readonly INewsItemRepository _newsItemRepository;
        private readonly ILogger _logger;

        public NewsController(INewsItemRepository newsItemRepository, ILogger<NewsController> logger)
        {
            _newsItemRepository = newsItemRepository;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<object> GetAsync(
            [FromQuery] int skip,
            [FromQuery] int limit = 3)
        {
            var result = await _newsItemRepository.GetLatestAsync(skip, limit);
            return new
            {
                status = JStatus.OK,
                result = result.Select(
                    x => new
                    {
                        x.Id,
                        x.Title,
                        Author = x.AuthorName,
                        Date = (x.Published ?? DateTime.UtcNow).ToString("dd MMMM yyyy"),
                        IsPublished = x.Published.HasValue,
                        x.Html
                    })
            };
        }

        [HttpPost]
        public async Task<JResponse> CreateAsync(
            [FromBody] WriteNewsItemRequest body)
        {
            var currentUser = User.Identity as AuthenticatedUser;
            var now = DateTime.UtcNow;
            var newsItem = new NewsItem()
            {
                Title = body.Title,
                AuthorId = currentUser.Id,
                Created = now,
                Modified = now,
                Html = body.Html
            };
            _logger.LogInformation($"Creating news item: {newsItem.Title} by {currentUser.Name}");
            await _newsItemRepository.UpdateAsync(newsItem);
            return JResponse.OK();
        }

        [HttpPut]
        public async Task<object> UpdateAsync(
            [FromBody] WriteNewsItemRequest body)
        {
            var newsItem = await _newsItemRepository.GetAsync(body.Id);
            if (newsItem == null)
            {
                return NotFound(JResponse.Error("News item not found."));
            }

            newsItem.Modified = DateTime.UtcNow;
            if (body.Title != null)
            {
                newsItem.Title = body.Title;
            }
            if (body.Html != null)
            {
                newsItem.Html = body.Html;
            }
            if (body.Published == true)
            {
                newsItem.Published = DateTime.UtcNow;
            }
            _logger.LogInformation($"Updating news item: {newsItem.Title} (updated by {User.Identity.Name}");
            await _newsItemRepository.UpdateAsync(newsItem);
            return JResponse.OK();
        }

        [HttpDelete]
        public async Task<object> DeleteAsync(
            [FromBody] WriteNewsItemRequest body)
        {
            if (body.Id == null)
            {
                return BadRequest(JResponse.Error("News item id not specified."));
            }

            _logger.LogInformation($"Deleting news item: {body.Id} (deleted by {User.Identity.Name}");
            await _newsItemRepository.DeleteAsync(body.Id);
            return JResponse.OK();
        }
    }
}
