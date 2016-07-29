using Common.Utility;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace ContentManagementBackend.Demo
{
    public class SmtpEmailService : IIdentityMessageService
    {
        //поля
        private ICommonLogger _logger;



        //инициализация
        public SmtpEmailService(ICommonLogger logger)
        {
            _logger = logger;
        }


        //методы
        public Task SendAsync(IdentityMessage message)
        {
            try
            {
                Configuration configurationFile = WebConfigurationManager.OpenWebConfiguration("~/web.config");
                MailSettingsSectionGroup mailSettings = configurationFile
                    .GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            
                string from = mailSettings.Smtp.Network.UserName;
                MailMessage msg = new MailMessage(from, message.Destination, message.Subject, message.Body);
                msg.IsBodyHtml = true;
                msg.BodyEncoding = Encoding.UTF8;
                msg.SubjectEncoding = Encoding.UTF8;

                using (SmtpClient client = new SmtpClient())
                {
                    client.Timeout = 10 * 1000;
                    client.Send(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return Task.FromResult(0);
        }
    }
}