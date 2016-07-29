using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentManagementBackend.Demo
{
    public interface IIdentityQueries
    {
        Task<UserAccount> User_Find(string email, string name);
        Task<List<UserAccount>> SelectUsers(int page, int pageSize);
    }
}