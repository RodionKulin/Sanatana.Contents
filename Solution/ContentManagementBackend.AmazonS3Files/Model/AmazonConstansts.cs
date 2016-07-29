using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.AmazonS3Files
{
    internal static class AmazonConstansts
    {
        public const string UNKNOWN_CREDENTIALS_REGION = "Unknown";
        public const int MAX_OBJECTS_SELECTED = 10000;
        public const int MAX_OBJECTS_DELETED = 1000;
        public const int MAX_DELETE_ROUNDS = 10;

    }
}
