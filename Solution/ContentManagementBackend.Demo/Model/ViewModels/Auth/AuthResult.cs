using Common.Utility.Pipelines;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class AuthResult : PipelineResult
    {
        //свойства
        public bool IsPasswordRequired { get; set; }      
        public UserAccount User { get; set; }
        public string AntiForgeryToken { get; set; }



        //инициализация
        public AuthResult()
        {
            Result = true;
        }

        public new static AuthResult Fail(string message)
        {
            return new AuthResult()
            {
                Result = false,
                Message = message
            };
        }

        public new static AuthResult Success(string message)
        {
            return new AuthResult()
            {
                Result = true,
                Message = message
            };
        }
    }
}