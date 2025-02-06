using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using PDFSender.Common.Data;

namespace PDFSender.Common.Handlers
{
    internal class EmailHandler
    {
        #region Constants

        private const string SMTP_HOST = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        private const string MAIL_SENDER = "msindialeave@gmail.com";
        private const string SENDER_PASSWORD = "wxmq bhll fxap amad";
        private const string DISPLAY_NAME = "MSI Task Reminder";

        private string SmtpHost { get; set; }
        private int SmtpPort { get; set; }
        private string MailSender { get; }
        private string MailPasskey { get; }
        private string MailDisplayName { get; }

        #endregion

        internal EmailHandler(string strMailSender, string strMailPasskey, string strMailDisplayName)
        {
            SmtpHost = SMTP_HOST;
            SmtpPort = SMTP_PORT;

            MailSender = strMailSender;
            MailPasskey = strMailPasskey;
            MailDisplayName = strMailDisplayName;
        }

        internal EmailHandler(string strSmtpHost, int strSmtpPort, string strMailSender, string strPasskey,
            string mailDisplayName)
        {
            SmtpHost = strSmtpHost ?? SMTP_HOST;
            SmtpPort = strSmtpPort == 0 ? SMTP_PORT : strSmtpPort;

            MailSender = strMailSender ?? MAIL_SENDER;
            MailPasskey = strPasskey ?? SENDER_PASSWORD;

            MailDisplayName = mailDisplayName ?? DISPLAY_NAME;
        }
        #region Privates

        internal bool SendMail(string strSubject, string strContent, string[] strReceivers, FileConfiguration fileConfiguration)
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
            }

            return true;
        }

        private MailMessage PrepareMailContent(string strSubject, string strEmailContent)
        {
            MailAddress mailSender = new MailAddress(MailSender, MailDisplayName);

            MailMessage mailMessage = new MailMessage
            {
                From = mailSender,
                Subject = strSubject,
                Body = strEmailContent,
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
            {
                attachment = new Attachment(fileConfiguration.FilePath);
            }
            else if (fileConfiguration.Attachment != null)
            {
                attachment = new Attachment(new System.IO.MemoryStream(fileConfiguration.Attachment), fileConfiguration.FileName, "application/pdf");
            }
            
            mailMessage.Attachments.Add(attachment);
        }

        private void SendMail(MailMessage mailMessage)
        {
            NetworkCredential networkCred = new NetworkCredential(MailSender, MailPasskey);

            SmtpClient smtp = new SmtpClient
            {
                Host = SMTP_HOST,
                Port = SMTP_PORT,
                EnableSsl = true,
                Credentials = networkCred,
            };
            smtp.Send(mailMessage);
        }

        #endregion
    }
}