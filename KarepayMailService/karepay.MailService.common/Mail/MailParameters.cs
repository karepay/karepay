using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace karepay.MailService.common.Mail
{
    public class MailParameters
    {
        public int SmtpPort { get; set; }
        public string SmtpAddress { get; set; }
        public bool EnableSSL { get; set; }
        public string From { get; set; }
        public string Username{ get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }

    }
}
