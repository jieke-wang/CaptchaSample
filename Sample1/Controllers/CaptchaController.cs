using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hei.Captcha;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sample1.Models;

namespace Sample1.Controllers
{
    public class CaptchaController : Controller
    {
        private readonly ILogger<CaptchaController> _logger;
        private readonly SecurityCodeHelper _securityCode;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _captchaTimeout = TimeSpan.FromMinutes(1);

        public CaptchaController(ILogger<CaptchaController> logger, SecurityCodeHelper securityCode, IMemoryCache memoryCache)
        {
            _logger = logger;
            _securityCode = securityCode;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// 泡泡中文验证码 
        /// </summary>
        /// <returns></returns>
        public IActionResult BubbleCode(string g)
        {
            var code = _securityCode.GetRandomCnText(2);
            _memoryCache.Set(g, code, _captchaTimeout);
            _logger.LogInformation($"验证码: {code}");
            var imgbyte = _securityCode.GetBubbleCodeByte(code);

            return File(imgbyte, "image/png");
        }

        /// <summary>
        /// 数字字母组合验证码
        /// </summary>
        /// <returns></returns>
        public IActionResult HybridCode(string g)
        {
            var code = _securityCode.GetRandomEnDigitalText(4);
            _memoryCache.Set(g, code, _captchaTimeout);
            _logger.LogInformation($"验证码: {code}");
            var imgbyte = _securityCode.GetEnDigitalCodeByte(code);

            return File(imgbyte, "image/png");
        }

        /// <summary>
        /// gif泡泡中文验证码 
        /// </summary>
        /// <returns></returns>
        public IActionResult GifBubbleCode(string g)
        {
            var code = _securityCode.GetRandomCnText(2);
            _memoryCache.Set(g, code, _captchaTimeout);
            _logger.LogInformation($"验证码: {code}");
            var imgbyte = _securityCode.GetGifBubbleCodeByte(code);

            return File(imgbyte, "image/gif");
        }

        /// <summary>
        /// gif数字字母组合验证码
        /// </summary>
        /// <returns></returns>
        public IActionResult GifHybridCode(string g)
        {
            var code = _securityCode.GetRandomEnDigitalText(4);
            _memoryCache.Set(g, code, _captchaTimeout);
            _logger.LogInformation($"验证码: {code}");
            var imgbyte = _securityCode.GetGifEnDigitalCodeByte(code);

            return File(imgbyte, "image/gif");
        }

        public IActionResult Verify(string g, string c)
        {
            if (_memoryCache.TryGetValue(g, out string code) == false)
            {
                return Ok(false);
            }

            _memoryCache.Remove(g);

            if(code != c)
            {
                return Ok(false);
            }

            return Ok(true);
        }
    }
}