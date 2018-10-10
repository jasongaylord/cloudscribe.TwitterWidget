﻿using cloudscribe.TwitterWidget.Models;
using cloudscribe.TwitterWidget.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cloudscribe.TwitterWidget.Controllers
{
    public class TwitterWidgetController : Controller
    {
        protected IHostingEnvironment HostingEnvironment { get; private set; }
        protected ITwitterService TwitterService { get; private set; }
        protected TwitterOptions TwitterOptions { get; private set; }
        protected ILogger Log { get; private set; }
        private MemoryCache _cache;
        private static readonly string CacheKey = "TwitterCache_";

        public TwitterWidgetController(IHostingEnvironment appEnv, ITwitterService twitterService, ILogger<TwitterWidgetController> logger, TwitterCache cache, IOptions<TwitterOptions> options = null)
        {
            HostingEnvironment = appEnv;
            TwitterService = twitterService;
            Log = logger;
            _cache = cache.Cache;

            if (options != null)
                TwitterOptions = options.Value;
            else
                TwitterOptions = new TwitterOptions();
        }

        [HttpPost]
        [Route("twitter/gettweets")]
        public virtual async Task<IActionResult> RetrieveTweets()
        {
            var key = CacheKey + TwitterOptions.Username + "_RetrieveTweets";
            var result = new List<TweetStruct>();

            if (!_cache.TryGetValue(key, out result))
            {
                result = await TwitterService.RetrieveTweets(TwitterOptions, key);
                _cache.Set(key, result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(TwitterOptions.CacheMinutes));
            }

            List<TweetStruct> results = _cache.Get<List<TweetStruct>>(key) ?? await TwitterService.RetrieveTweets(TwitterOptions, key);

            return new JsonResult(results);
        }
    }
}
