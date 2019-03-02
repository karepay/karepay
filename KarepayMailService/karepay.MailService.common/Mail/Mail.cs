using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using karepay.MailService.common.Enum;

namespace karepay.MailService.common.Mail
{
    public class Mail
    {
        public int MailID { get; set; }
        public string DisplayName { get; set; }
        public string Body{ get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public MailType MailType { get; set; }
        public MailParameters MailParameters { get; set; }
    }
}
