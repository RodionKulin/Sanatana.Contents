using Common.Utility.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentPipelineResult
    {
        //свойства
        public ContentUpdateResult Result { get; set; }
        public string Message { get; set; }
        public string ImageUrl { get; set; }  


        //инициализация       
        public static ContentPipelineResult Fail(
            string message, ContentUpdateResult result = ContentUpdateResult.HasException)
        {
            return new ContentPipelineResult()
            {
                Result = result,
                Message = message
            };
        }

        public static ContentPipelineResult Success(string message)
        {
            return new ContentPipelineResult()
            {
                Result = ContentUpdateResult.Success,
                Message = message
            };
        }
    }
}
