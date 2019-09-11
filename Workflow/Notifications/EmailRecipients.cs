using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Notifications
{
    public class EmailRecipients
    {
        public EmailRecipients()
        {
            ToRecipients = new List<string>();
            CCRecipients = new List<string>();
        }
        public List<string> ToRecipients { get; set; }
        public List<string> CCRecipients { get; set; }
    }
}
