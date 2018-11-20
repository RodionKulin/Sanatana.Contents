using Sanatana.Contents.Objects;
using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Contents
{
    public class ContentUpdateResult
    {
        //properties
        public OperationStatus Status { get; set; }
        public List<string> Messages { get; set; }


        //init       
        public static ContentUpdateResult Error(string message)
        {
            return new ContentUpdateResult()
            {
                Status = OperationStatus.Error,
                Messages = new List<string>() { message }
            };
        }
        
        public static ContentUpdateResult ContentNotFound()
        {
            return new ContentUpdateResult
            {
                Messages = new List<string> { ContentsMessages.Content_NotFound },
                Status = OperationStatus.NotFound
            };
        }

        public static ContentUpdateResult PermissionDenied()
        {
            return new ContentUpdateResult
            {
                Messages = new List<string> { ContentsMessages.Common_AuthorizationRequired },
                Status = OperationStatus.PermissionDenied
            };
        }

        public static ContentUpdateResult VersionChanged()
        {
            return new ContentUpdateResult
            {
                Messages = new List<string> { ContentsMessages.Content_WrongUpdateNonce },
                Status = OperationStatus.VersionChanged
            };
        }

        public static ContentUpdateResult Success()
        {
            return new ContentUpdateResult()
            {
                Status = OperationStatus.Success,
                Messages = new List<string>() { }
            };
        }
        
    }
}
