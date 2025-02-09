using System.Collections.Generic;
using System.IO;
using PDFSender.Common.Data;
using PDFSender.Common.Handlers;

namespace PDFSender.Common
{
    public class Services
    {
        public void SendEmail(EmailConfiguration[] emailCollection)
        {
            EmailHandler emailHandler = null;

            foreach (EmailConfiguration email in emailCollection)
            {
                if (emailHandler == null) emailHandler = new EmailHandler(email.Username, email.Password, email.DisplayName);

                emailHandler.SendMail(email.Subject, email.Content, email.Receivers, email.Attachment);
            }
        }

        public string[] ConvertToText(string strFilePath)
        {
            var pdfHandler = new PdfHandler();
            string strPdfText = pdfHandler.ConvertToText(strFilePath);

            List<string> liReceiverCollection = pdfHandler.ExtractGmailEmails(strPdfText);

            return liReceiverCollection.ToArray();
        }

        public string[] ConvertToText(MemoryStream stream)
        {
            var pdfHandler = new PdfHandler();
            string strPdfText = pdfHandler.ConvertToText(stream);

            List<string> liReceiverCollection = pdfHandler.ExtractGmailEmails(strPdfText);

            return liReceiverCollection.ToArray();
        }
    }
}