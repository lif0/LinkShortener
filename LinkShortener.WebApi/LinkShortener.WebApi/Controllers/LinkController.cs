using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
using LinkShortener.WebApi.Entities;
using LinkShortener.WebApi.Extensions;
using LinkShortener.WebApi.ViewModels;

namespace LinkShortener.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LinkController : ControllerBase
    {
        private readonly IMongoCollection<CompressedLinkEntity> _linkCollection;
        private readonly IMongoCollection<NoAuthUserLinkEntity> _usersCollection;

        public LinkController(IMongoCollection<CompressedLinkEntity> linkCollection, IMongoCollection<NoAuthUserLinkEntity> usersCollection)
        {
            _linkCollection = linkCollection;
            _usersCollection = usersCollection;
        }
        
        [HttpPost]
        public async Task<IActionResult> Compress(string link)
        {
            if (LinkExtension.IsValidLink(link))
            {
                var entity = await _linkCollection.Find(_ => _.Link == link).FirstOrDefaultAsync();
                string entityId = entity?.Id;
                if (entity == null)
                {
                    entityId = Generator.GenerateUniqLink();
                    bool exists = true;
                    while (exists)
                    {
                        exists = await _linkCollection.Find(_ => _.Id == entityId).AnyAsync();
                        if (exists) entityId = Generator.GenerateUniqLink();
                    }

                    var newEntity = new CompressedLinkEntity
                    {
                        Link = link,
                        Id = entityId,
                        OpenedCount = 0
                    };
                    await _linkCollection.InsertOneAsync(newEntity);
                }
                
                string compressedUrl = $"{this.Request.Scheme}://{this.Request.Host}/{entityId}";
                
                if (this.HttpContext.Request.Cookies.ContainsKey(Cookies.BrowserUserId))
                {
                    var browserUserId = Guid.Parse(this.HttpContext.Request.Cookies[Cookies.BrowserUserId]);
                    
                    var noAuthUser = await _usersCollection.Find(_ => _.Id == browserUserId).FirstOrDefaultAsync();
                    noAuthUser.Links.Add(compressedUrl);
                    await _usersCollection.ReplaceOneAsync((p => p.Id == browserUserId), noAuthUser, new ReplaceOptions {IsUpsert = true});
                }
                else
                {
                    Guid browserUserId = Guid.NewGuid();
                    var newUser = new NoAuthUserLinkEntity
                    {
                        Id = browserUserId,
                        Links = new List<string>()
                    };
                    newUser.Links.Add(compressedUrl);
                    await _usersCollection.InsertOneAsync(newUser);
                    this.HttpContext.Response.Cookies.Append(Cookies.BrowserUserId,browserUserId.ToString());
                }
                
                return this.Ok(compressedUrl);
            }

            return this.BadRequest("Link is not valid, please use Http or Https scheme");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cursor = await _linkCollection.FindAsync(FilterDefinition<CompressedLinkEntity>.Empty);
            var result = new List<CompressedLinkViewModel>();
            while (await cursor.MoveNextAsync())
            {
                var links = cursor.Current;
                var s = links.Select(l => new CompressedLinkViewModel
                    {CompressedLink = $"{this.Request.Scheme}://{this.Request.Host}/{l.Id}", OpenedCount = l.OpenedCount});
                result.AddRange(s);
            }

            return this.Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMyCompressedLink()
        {
            if (this.HttpContext.Request.Cookies.ContainsKey(Cookies.BrowserUserId))
            {
                var browserUserId = Guid.Parse(this.HttpContext.Request.Cookies[Cookies.BrowserUserId]);
                var cursor = await _usersCollection.FindAsync(u => u.Id == browserUserId);
                var result = await cursor.FirstOrDefaultAsync();
                return this.Ok(result.Links);
            }
            
            return this.Ok(new List<string>());
        }
    }
}