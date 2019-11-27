using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EustonLeisureMessageFilteringDesktop
{
    public class EmailMessageService : MessageService
    {
        public string Subject { get; set; }
        public EmailMessageService()
        {
            MessageType = "Email";
            MaxCharacterCount = 1028;
        }

        public override string GetSubject()
        {
            return this.Subject;
        }

        public override bool FilterSender(string messageBody)
        {
            Match match = Regex.Match(messageBody, EmailPattern);
            if (!match.Success || !messageBody.StartsWith(match.Value))
            {
                MessageBody = null;
                return false;
            }

            Sender = match.Value;
            MessageBody = messageBody.Substring(match.Value.Length).TrimStart(new char[] { ' ' });
            FilterSubject(MessageBody);
            return true;
        }

        public void FilterSubject(string message)
        {
            string[] array = message.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length < 1)
                return;
            Subject = array[0];

            this.MessageBody = MessageBody.Substring(Subject.Length).TrimStart(new char[] { ' ', '\r','\n' });
            if (Subject.StartsWith("SIR") && ProcessIncidentReport())
            {
                MessageType += ", Significant Incident Reports";
                ExtractSirList();
            }
            else
            {
                MessageType += ", Standard";
            }

        }

        private void ExtractSirList()
        {
            string[] array = MessageBody.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length < 2)
                return;
            if (!array[0].StartsWith("Sport Centre Code:") || !array[1].StartsWith("Nature of Incident:"))
                return;
            ExtraList.Add("SIR List", new List<string>() { array[0], array[1] });
        }

        private bool ProcessIncidentReport()
        {
            string[] array = Subject.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length < 2)
                return false;
            string date = array[1];
            array = date.Split('/');
            if (array.Length != 3)
                return false;
            int num = -1;
            if (!int.TryParse(array[0], out num) || num > 31)
                return false;
            if (!int.TryParse(array[1], out num) || num > 12)
                return false;
            if (!int.TryParse(array[2], out num))
                return false;
            return true;
        }
    }
}
