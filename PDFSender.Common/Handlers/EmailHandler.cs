using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using PDFSender.Common.Data;

namespace PDFSender.Common.Handlers
{
    internal class EmailHandler
    {
        internal EmailHandler(string strMailSender, string strMailPasskey, string strMailDisplayName)
        {
            SmtpHost = "smtp.gmail.com";
            SmtpPort = 587;

            MailSender = strMailSender;
            MailPasskey = strMailPasskey;
            MailDisplayName = strMailDisplayName;
        }

        internal EmailHandler(string strSmtpHost, int strSmtpPort, string strMailSender, string strPasskey,
            string mailDisplayName)
        {
            SmtpHost = strSmtpHost ?? "smtp.gmail.com";
            SmtpPort = strSmtpPort == 0 ? 587 : strSmtpPort;

            MailSender = strMailSender;
            MailPasskey = strPasskey;

            MailDisplayName = mailDisplayName;
        }

        #region Constants

        private string SmtpHost { get; }
        private int SmtpPort { get; }
        private string MailSender { get; }
        private string MailPasskey { get; }
        private string MailDisplayName { get; }

        #endregion

        #region Privates

        internal bool SendMail(string strSubject, string strContent, string[] strReceivers,
            FileConfiguration fileConfiguration)
        {
            var mailMessage = PrepareMailContent(strSubject, strContent);

            AddMailReceivers(mailMessage, strReceivers);

            AddAttachment(mailMessage, fileConfiguration);

            try
            {
                SendMail(mailMessage);
            }
            catch (Exception ex)
            {
                var strException = ex.ToString();

                return false;
            }

            return true;
        }

        private MailMessage PrepareMailContent(string strSubject, string strEmailContent)
        {
            var mailSender = new MailAddress(MailSender, MailDisplayName);

            var mailMessage = new MailMessage
            {
                From = mailSender,
                Subject = strSubject,
                Body = strEmailContent
            };

            return mailMessage;
        }

        private void AddMailReceivers(MailMessage mailMessage, string[] strReceivers)
        {
            if (!strReceivers.Any()) return;

            foreach (var strReceiver in strReceivers) mailMessage.To.Add(strReceiver);
        }

        private void AddAttachment(MailMessage mailMessage, FileConfiguration fileConfiguration)
        {
            if (fileConfiguration == null) return;

            Attachment attachment = null;
            if (fileConfiguration.FilePath != null)
                attachment = new Attachment(fileConfiguration.FilePath);
            else if (fileConfiguration.Attachment != null)
                attachment = new Attachment(new MemoryStream(fileConfiguration.Attachment), fileConfiguration.FileName,
                    "application/pdf");

            mailMessage.Attachments.Add(attachment);
        }

        private void SendMail(MailMessage mailMessage)
        {
            var networkCred = new NetworkCredential(MailSender, MailPasskey);

            var smtp = new SmtpClient
            {
                Host = SmtpHost,
                Port = SmtpPort,
                EnableSsl = true,
                Credentials = networkCred
            };
            smtp.Send(mailMessage);
        }

        #endregion
    }
}