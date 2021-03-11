using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace LinkShortener.WebApi
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
                var entry = await collection.FindSync(p => p.Id == path).SingleOrDefaultAsync();
                if(entry != null)
                {
                    entry.OpenedCount += 1;
                    await collection.ReplaceOneAsync((p => p.Id == path), entry, new UpdateOptions {IsUpsert = true});
                    redirect = entry.Link;
                }
                    
                ctx.Response.Redirect(redirect);    
            }
        }
    }
}