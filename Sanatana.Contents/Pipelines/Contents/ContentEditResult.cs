using Sanatana.Contents.Objects;
using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Contents
{
    public class ContentEditResult
    {
        //properties
        public OperationStatus Status { get; set; }
        public List<string> Messages { get; set; }


        //init       
        public static ContentEditResult Error(string message)
        {
            return new ContentEditResult()
            {
                Status = OperationStatus.Error,
                Messages = new List<string>() { message }
            };
        }
        
        public static ContentEditResult ContentNotFound()
        {
            return new ContentEditResult
            {
                Messages = new List<string> { ContentsMessages.Content_NotFound },
                Status = OperationStatus.NotFound
            };
        }

        public static ContentEditResult PermissionDenied()
        {
            return new ContentEditResult
            {
                Messages = new List<string> { ContentsMessages.Common_AuthorizationRequired },
                Status = OperationStatus.PermissionDenied
            };
        }

        public static ContentEditResult VersionChanged()
        {
            return new ContentEditResult
            {
                Messages = new List<string> { ContentsMessages.Content_WrongUpdateNonce },
                Status = OperationStatus.VersionChanged
            };
        }

        public static ContentEditResult Success()
        {
            return new ContentEditResult()
            {
                Status = OperationStatus.Success,
                Messages = new List<string>() { }
            };
        }
        
    }
}
