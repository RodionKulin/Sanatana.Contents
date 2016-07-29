using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend.Recaptcha
{
    public class RecaptchaProvider : ICaptchaProvider
    {
        //свойства
        public string PrivateKeyAppSettingsKey { get; set; }



        //инициализация
        public RecaptchaProvider()
        {
            PrivateKeyAppSettingsKey = Constants.APP_SETTINGS_KEY;            
        }


        //методы
        protected virtual string GetPrivateKey()
        {
            return ConfigurationManager.AppSettings[PrivateKeyAppSettingsKey];
        }

        public virtual bool Validate(string challenge, string response)
        {
            ReCaptchaResponse captchaResponse = null;

            try
            {
                using (var client = new WebClient())
                {
                    string privateKey = GetPrivateKey();
                    string url = string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}"
                        , privateKey, response);

                    string googleReply = client.DownloadString(url);
                    captchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(googleReply);
                }
            }
            catch
            {

            }

            return captchaResponse != null
                && captchaResponse.Success != null
                && captchaResponse.Success.ToLowerInvariant() == "true";
        }
        
    }
}
