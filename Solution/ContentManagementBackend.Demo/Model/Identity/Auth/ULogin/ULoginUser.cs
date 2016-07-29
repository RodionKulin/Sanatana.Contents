using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class ULoginUser
    {
        public string Email { get; set; }
        public string First_Name { get; set; }
        public string Identity { get; set; }
        public string Last_Name { get; set; }
        public string Network { get; set; }
        public string NickName { get; set; }
        public string Phone { get; set; }
        public string Profile { get; set; }
        public string Sex { get; set; }
        public string Uid { get; set; }
        public string Photo { get; set; }
        public string Photo_big { get; set; }
        public string Error { get; set; }

        

        //методы
        public ULoginError GetErrorType()
        {
            ULoginError error = ULoginError.Unknown;

            if (Error == "token expired")
                error = ULoginError.TokenExpired;
            else if (Error == "invalid token")
                error = ULoginError.InvalidToken;
            else if (Error.StartsWith("host is not "))
                error = ULoginError.InvalidToken;

            return error;
        }
    }
}