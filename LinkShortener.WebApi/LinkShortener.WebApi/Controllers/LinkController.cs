using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
    
namespace LinkShortener.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LinkController : ControllerBase
    {
        private readonly IMongoCollection<CompressedLinkEntity> _linkCollection;

        public LinkController(IMongoCollection<CompressedLinkEntity> linkCollection)
        {
            _linkCollection = linkCollection;
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
                
                return this.Ok($"{this.Request.Scheme}://{this.Request.Host}/{entityId}");
            }

            return this.BadRequest("Link is not valid");
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
    }
}