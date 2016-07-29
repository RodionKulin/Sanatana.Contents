using ContentManagementBackend.Resources;
using Common.Utility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class CustomContentManager : ContentManager<ObjectId>
    {

        //инициализация
        public CustomContentManager(ICacheProvider cacheProvider, ICommonLogger logger, IContentQueries<ObjectId> postQueries
            , ISearchQueries<ObjectId> searchQueries, ICommentQueries<ObjectId> commentQueries, ICategoryQueries<ObjectId> categoryQueries
            , PreviewImageQueries previewImageQueries, ContentImageQueries contentQueries
            , CommentImageQueries commentImageQueries, AvatarImageQueries avatarQueries
           , ICategoryManager<ObjectId> categoryManager, ICommentManager<ObjectId> commentManager)
            :base(cacheProvider, logger, postQueries, categoryQueries, commentQueries, searchQueries
                 , previewImageQueries, contentQueries, commentImageQueries, avatarQueries, categoryManager, commentManager)
        {
        }


        

    }
}