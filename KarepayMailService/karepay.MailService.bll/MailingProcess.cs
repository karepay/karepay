using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using karepay.MailService.Integration;

namespace karepay.MailService.bll
{
    public class MailingProcess
    {
        public void MailProcess()
        {
            ActivationMail activation = new ActivationMail();
            activation.ActivationMailProcess();
        }
    }
}
