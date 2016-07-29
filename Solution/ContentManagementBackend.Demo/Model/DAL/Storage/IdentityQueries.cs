using Common.Identity2_1.MongoDb;
using Common.MongoDb;
using Common.Utility;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class IdentityQueries : MongoUserQueries<UserAccount>, IIdentityQueries
    {
        //поля
        public ICommonLogger _logger;



        //инициализация
        public IdentityQueries(MongoDbConnectionSettings connection, ICommonLogger logger)
            : base(connection)
        {
            _logger = logger;
        }


        //методы
        public async Task<UserAccount> User_Find(string email, string name)
        {
            UserAccount result = null;

            FilterDefinition<UserAccount> filter = Builders<UserAccount>.Filter.Where(
                p => p.Email == email
                && p.UserName == name);

            try
            {
                result = await _context.Users.Find(filter).FirstOrDefaultAsync();
            }
            catch(Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }
    }
}