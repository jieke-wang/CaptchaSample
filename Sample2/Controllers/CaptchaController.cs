using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

using Sample2.Lib;

namespace Sample2.Controllers
{
    public class CaptchaController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public CaptchaController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public IActionResult Index(string g)
        {
            string fontFileName = Path.Combine(Directory.GetCurrentDirectory(), "fonts/Cascadia.ttf");
            using (Captcha captcha = new Captcha(_distributedCache, g, string.Empty, 415, 50, fontFileName))
            {
                //Response.Headers.Add("Content-Type", new Microsoft.Extensions.Primitives.StringValues("image/jpeg"));
                //captcha.Image.Save(Response.Body, System.Drawing.Imaging.ImageFormat.Jpeg);

                using (MemoryStream ms = new MemoryStream())
                {
                    captcha.Image.Save(ms, ImageFormat.Jpeg);
                    return File(ms.ToArray(), "image/jpeg");
                }
            }
        }

        public IActionResult Verify(string g, string c)
        {
            Captcha captcha = new Captcha(_distributedCache, g);
            return Ok(captcha.ValidKey(c));
        }
    }
}
