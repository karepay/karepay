using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using karepay.MailService.common.Enum;
using karepay.MailService.common.Mail;
using karepay.MailService.common.OperationResults;

namespace karepay.MailService.Interfaces
{
    public interface IMailService
    {
        Mail PrepareMail(MailType mailType);
        OperationResult SaveMail(Mail mail, string activationLink);
        void SendMail(MailType mailType);
    }
}
