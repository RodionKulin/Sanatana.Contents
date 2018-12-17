using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Sanatana.Contents.Database;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files.Services;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Pipelines;
using Sanatana.Contents.Pipelines.Contents;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Utilities;

namespace Sanatana.ContentsSpecs.TestTools.Objects
{
    public class InsertTicketPipeline 
        : InsertContentPipeline<ObjectId, Category<ObjectId>, Ticket<ObjectId>>
    {

        public InsertTicketPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<ObjectId, Ticket<ObjectId>> contentQueries
            , IPermissionSelector<ObjectId, Category<ObjectId>> permissionSelector, ISearchQueries<ObjectId> searchQueries
            , IImageFileService imageFileService, IHtmlMediaExtractor htmlMediaExtractor, IUrlEncoder urlEncoder)
            : base(exceptionHandler, contentQueries, permissionSelector
                  , searchQueries, imageFileService, htmlMediaExtractor, urlEncoder)
        {

        }

        public override Task<ContentUpdateResult> Execute(ContentUpdateParams<ObjectId, Ticket<ObjectId>> inputModel, ContentUpdateResult outputModel)
        {
            return base.Execute(inputModel, outputModel);
        }
    }
}
