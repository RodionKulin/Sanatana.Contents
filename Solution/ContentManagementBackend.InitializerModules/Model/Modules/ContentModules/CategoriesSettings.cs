using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.InitializerModules
{
    public class CategoriesSettings
    {
        public List<Category<ObjectId>> Categories { get; set; }


        //инициализация
        public CategoriesSettings()
        {
            Categories = new List<Category<ObjectId>>();
        }

        public static CategoriesSettings FromNames(List<string> names)
        {
            return new CategoriesSettings()
            {
                Categories = names.Select(p => new Category<ObjectId>()
                {
                    Name = p
                }).ToList()
            };
        }
    }
}
