using HtmlAgilityPack;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Pipelines.Contents;
using Sanatana.Contents.RegularJobs;
using Sanatana.Contents.YouTube;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents
{    
    public class ImportYoutubeJob<TKey, TCategory, TContent> : IContentRegularJob
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //fields
        private ImportYoutubeSettings<TKey> _settings;
        private IContentQueries<TKey, YoutubePost<TKey>> _contentQueries;
        private ImportYoutubeContentPipeline<TKey, TCategory, YoutubePost<TKey>> _importPipeline;
        private ICategoryQueries<TKey, TCategory> _categoryQueries;

        
        //init
        public ImportYoutubeJob(ImportYoutubeSettings<TKey> settings
            , IContentQueries<TKey, YoutubePost<TKey>> contentQueries, ICategoryQueries<TKey, TCategory> categoryQueries
            , ImportYoutubeContentPipeline<TKey, TCategory, YoutubePost<TKey>> importPipeline)
        {
            _settings = settings;
            _contentQueries = contentQueries;
            _importPipeline = importPipeline;
            _categoryQueries = categoryQueries;
        }


        //methods    
        public virtual void Execute()
        {
            List<YoutubePost<TKey>> channelVideos = GetChannelVideos();
            if (channelVideos.Count == 0)
            {
                return;
            }

            List<TCategory> categories = _categoryQueries.SelectMany(
                p => EqualityComparer<TKey>.Default.Equals(p.CategoryId, _settings.CategoryId)).Result;
            TCategory category = categories.First();
           
            List<string> existingPostUrls = GetExistingPostUrls(category);
            List<string> excludeTitles = _settings.VideoFilterExcludeByTitle ?? new List<string>();
            excludeTitles.ForEach(x => x = x.ToLower());

            channelVideos = channelVideos
               .Where(p => !existingPostUrls.Contains(p.YoutubeUrl))
               .Where(p => !excludeTitles.Contains(p.Title.ToLower()))
               .Reverse()
               .ToList();
            if(channelVideos.Count == 0)
            {
                return;
            }
            
            foreach (YoutubePost<TKey> post in channelVideos)
            {
                post.CategoryId = category.CategoryId;
                post.PublishTimeUtc = DateTime.UtcNow;
                post.State = _settings.NewContentState;
                post.AuthorId = _settings.AuthorId;

                ContentEditResult importResult = _importPipeline.Execute(new ContentEditParams<TKey, YoutubePost<TKey>>()
                {
                    Content = post,
                    Permission = 0
                }, null).Result;
            }
        }
        
        public virtual List<YoutubePost<TKey>> GetChannelVideos()
        {
            var youtubePosts = new List<YoutubePost<TKey>>();

            HtmlWeb website = new HtmlWeb();
            HtmlDocument doc = website.Load(_settings.ChannelUrl);

            List<HtmlAgilityPack.HtmlNode> titleNodes =
                doc.DocumentNode.Descendants("h3")
                .Where(n => n.GetAttributeValue("class", "").Contains("yt-lockup-title"))
                .ToList();

            if (titleNodes.Count == 0)
            {
                return youtubePosts;
            }

            foreach (HtmlAgilityPack.HtmlNode titleNode in titleNodes)
            {
                if (titleNode.ChildNodes == null)
                {
                    continue;
                }

                HtmlAgilityPack.HtmlNode titleAnchor = titleNode.ChildNodes.FirstOrDefault();
                if (titleAnchor == null)
                {
                    continue;
                }

                string title = titleAnchor.InnerText;
                string href = titleAnchor.GetAttributeValue("href", "");
                Uri hrefUrl;
                if (!Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out hrefUrl))
                {
                    continue;
                }

                youtubePosts.Add(new YoutubePost<TKey>()
                {
                    YoutubeUrl = href,
                    Title = UppercaseFirstChar(title)
                });
            }

            return youtubePosts;
        }

        protected virtual List<string> GetExistingPostUrls(TCategory category)
        {
            List<string> existingPostUrls = new List<string>();

            int page = 1;

            while (true)
            {
                List<YoutubePost<TKey>> existingPosts = _contentQueries.SelectMany(
                    page, YouTubeConstants.EXISTING_POST_QUERY_COUNT, DataAmount.DescriptionOnly, false, 
                    x => EqualityComparer<TKey>.Default.Equals(x.CategoryId, category.CategoryId))
                    .Result;

                IEnumerable<YoutubePost<TKey>> existingYoutubePosts = existingPosts.Cast<YoutubePost<TKey>>();
                IEnumerable<string> urls = existingYoutubePosts.Select(p => p.YoutubeUrl);
                existingPostUrls.AddRange(urls);

                if (existingPosts.Count < YouTubeConstants.EXISTING_POST_QUERY_COUNT)
                {
                    break;
                }
                page++;
            }

            return existingPostUrls;
        }

        protected virtual string UppercaseFirstChar(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return char.ToUpper(input[0]) + input.Substring(1);
        }

    }
}
