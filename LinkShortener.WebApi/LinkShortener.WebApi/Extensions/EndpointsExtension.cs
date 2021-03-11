using System.Threading.Tasks;
using LinkShortener.WebApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace LinkShortener.WebApi.Extensions
{
    public static class EndpointsExtension
    {
        public static async Task RedirectToLink(HttpContext ctx)
        {
            string rpath = ctx.Request.Path.ToString();
            if (rpath != "/")
            {
                string redirect = "/";
                var collection = ctx.RequestServices.GetService<IMongoCollection<CompressedLinkEntity>>();
                string path = ctx.Request.Path.ToUriComponent().Trim('/');
                var cursor = await collection.FindAsync(p => p.Id == path);
                var entry = await cursor.SingleOrDefaultAsync();
                if(entry != null)
                {
                    entry.OpenedCount += 1;
                    await collection.ReplaceOneAsync((p => p.Id == path), entry, new ReplaceOptions {IsUpsert = true});
                    redirect = entry.Link;
                }
                    
                ctx.Response.Redirect(redirect);    
            }
        }
    }
}