using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

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


        [HttpGet]
        public async Task<IActionResult> GetAllCompressedLink() => this.Ok();
    }
}