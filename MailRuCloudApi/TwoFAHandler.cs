using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailRuCloudApi
{
    public interface ITwoFaHandler
    {
        string Get(string login, bool isAutoRelogin);
    }
}
