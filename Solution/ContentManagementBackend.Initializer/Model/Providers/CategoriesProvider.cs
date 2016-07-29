using ContentManagementBackend.Demo;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.Initializer
{
    public class CategoriesProvider
    {
        public static List<Category<ObjectId>> GetCategories()
        {
            ObjectId eventsID = ObjectId.GenerateNewId();
            ObjectId hotID = ObjectId.GenerateNewId();
            ObjectId humorID = ObjectId.GenerateNewId();
            ObjectId videoID = ObjectId.GenerateNewId();

            return new List<Category<ObjectId>>()
            {
                CreateCategory("События", IdentityUserRole.Author, null, eventsID),
                CreateCategory("Музыка", IdentityUserRole.Author, null, parentID: eventsID),
                CreateCategory("Кино", IdentityUserRole.Author, null, parentID: eventsID),
                CreateCategory("Афиша", IdentityUserRole.Author, null, parentID: eventsID),

                CreateCategory("Горячее", IdentityUserRole.Author, null, hotID),
                CreateCategory("Политика", IdentityUserRole.Author, null, parentID: hotID),
                CreateCategory("Обсуждения", IdentityUserRole.Author, null, parentID: hotID),

                CreateCategory("Юмор", IdentityUserRole.Author, null, humorID),

                CreateCategory("Видео", IdentityUserRole.Author, null, videoID),
                CreateCategory("Разное", IdentityUserRole.Author, null, parentID: videoID),

                CreateCategory("Руководства", IdentityUserRole.Admin, IdentityUserRole.Author)
            };
        }

        private static Category<ObjectId> CreateCategory(string name
            , IdentityUserRole editRole, IdentityUserRole? viewRole
            , ObjectId? id = null, ObjectId? parentID = null)
        {
            var category = new Category<ObjectId>()
            {
                Name = name,
                Permissions = new List<KeyValuePair<int, string>>()
                {
                    new KeyValuePair<int, string>((int)CategoryPermission.Insert, editRole.ToString()),
                    new KeyValuePair<int, string>((int)CategoryPermission.Edit, editRole.ToString()),
                    new KeyValuePair<int, string>((int)CategoryPermission.Delete, editRole.ToString()),
                }
            };

            if (viewRole != null)
            {
                category.Permissions.Add(
                    new KeyValuePair<int, string>((int)CategoryPermission.View, viewRole.Value.ToString()));
            }

            category.CategoryID = id ?? ObjectId.GenerateNewId();
            category.ParentCategoryID = parentID;
            return category;
        }

    }
}
