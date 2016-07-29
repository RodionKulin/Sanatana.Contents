using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class NoCaptchaProvider : ICaptchaProvider
    {
        //методы
        public bool Validate(string challenge, string response)
        {
            return true;
        }
    }
}
