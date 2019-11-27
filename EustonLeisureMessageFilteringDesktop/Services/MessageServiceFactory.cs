using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EustonLeisureMessageFilteringDesktop
{
    public class MessageServiceFactory
    {
        public static MessageService CreateMessageService(int type)
        {
            MessageService service = null;
            switch (type)
            {
                case 1:
                    service = new SmsMessageService();
                    break;
                case 2:
                    service = new EmailMessageService();
                    break;
                case 3:
                    service = new TweetMessageService();
                    break;
                default:
                    //service = new MessageService();
                    break;
            }
            return service;
        }
    }
}
