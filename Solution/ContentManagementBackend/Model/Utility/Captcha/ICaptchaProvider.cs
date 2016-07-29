using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public interface ICaptchaProvider
    {
        bool Validate(string challenge, string response);
    }
}
