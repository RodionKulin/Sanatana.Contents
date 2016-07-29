using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.ElasticSearch
{
    public class ESPaging
    {
        public static int ToSkipNumber(int page, int itemsPerPage)
        {
            if (itemsPerPage < 1)
                throw new Exception("Number of items per page must be greater then 0.");

            if (page < 1)
                page = 1;

            return (page - 1) * itemsPerPage;
        }
    }
}
