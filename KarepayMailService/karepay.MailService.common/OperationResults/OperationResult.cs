using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace karepay.MailService.common.OperationResults
{
    public class OperationResult
    {
        public bool result { get; set; }
        public Exception exception { get; set; }
    }
}
