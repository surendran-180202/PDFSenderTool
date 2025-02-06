using Microsoft.AspNetCore.Components.Forms;
using PDFSender.Common;
using PDFSender.Common.Data;
using System.IO;
using Microsoft.AspNetCore.Components;
using System.Net.Mail;
using System.Net;
using System;

namespace PDFSender.Web.Components.Pages;

public partial class Home
{
    private EmailConfiguration emailConfiguration = new EmailConfiguration();
    private List<EmailConfiguration> liEmailConfigurationCollection = new List<EmailConfiguration>();

    private string MainPath { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        emailConfiguration.Username = "msindialeave@gmail.com";
        emailConfiguration.Password = "wxmq bhll fxap amad";
        emailConfiguration.Subject = "Test Subject";
        emailConfiguration.Content = "Test Content";
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        IBrowserFile file = e.File;

        using Stream stream = file.OpenReadStream(maxAllowedSize: 10485760);

        await OnConvertToText(stream);

        using MemoryStream ms = new MemoryStream();
        await file.OpenReadStream(maxAllowedSize: 10485760).CopyToAsync(ms);
        byte[] buffer = ms.ToArray();

        await OnAddEmailAttachment(buffer, file.Name);

        liEmailConfigurationCollection.Add(emailConfiguration);
    }

    private async Task OnPastePath(ChangeEventArgs args)
    {
        MainPath = args.Value.ToString();

        AddEmailConfigurationCollection();
    }

    private void AddEmailConfigurationCollection()
    {
        if(string.IsNullOrEmpty(MainPath)) return;

        string[] strPdfFiles = Directory.GetFiles(MainPath, "*.pdf", SearchOption.AllDirectories);

        foreach (string strPdfFile in strPdfFiles)
        {
            Services services = new Services();
            string filePath = strPdfFile.Replace("\\", @"\");
            string[] strPdfText = services.ConvertToText(filePath);

            EmailConfiguration email = new EmailConfiguration()
            {
                Username = emailConfiguration.Username,
                Password = emailConfiguration.Password,
                Subject = emailConfiguration.Subject,
                Content = emailConfiguration.Content,
                Receivers = strPdfText,

                Attachment = new FileConfiguration()
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                },
            };

            liEmailConfigurationCollection.Add(email);
        }
    }

    private void OnSubmit()
    {
        Services services = new Services();
       // services.SendEmail(liEmailConfigurationCollection.ToArray());
    }  

    private async Task OnConvertToText(Stream pdfStream)
    {
        using MemoryStream memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);

        Services services = new Services();
        emailConfiguration.Receivers = services.ConvertToText(memoryStream);

        //Using File Path
        //emailConfiguration.Receivers = services.ConvertToText(@"E:\Temp\Test1.pdf");
    }

    private async Task OnAddEmailAttachment(byte[] pdfStream, string strFileName)
    {
        emailConfiguration.Attachment = new FileConfiguration()
        {
            FileName = strFileName,
            Attachment = pdfStream,
        };
    }

    private void OnDelete(EmailConfiguration deletableEmail)
    {
        liEmailConfigurationCollection.Remove(deletableEmail);
    }

    private void OnUpdate()
    {
        this.liEmailConfigurationCollection.Clear();

        AddEmailConfigurationCollection();
    }
}