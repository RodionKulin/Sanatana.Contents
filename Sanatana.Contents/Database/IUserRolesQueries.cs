using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database
{
    public interface IUserRolesQueries<TKey>
        where TKey : struct
    {
        Task<List<TKey>> SelectUserRoles(TKey? userId);
    }
}
