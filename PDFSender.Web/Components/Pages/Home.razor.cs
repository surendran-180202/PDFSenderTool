using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using PDFSender.Common;
using PDFSender.Common.Data;

namespace PDFSender.Web.Components.Pages;

public partial class Home
{
    private readonly EmailConfiguration _emailConfiguration = new();
    private readonly List<EmailConfiguration> _liEmailConfigurationCollection = new();

    private string? MainPath { get; set; } = string.Empty;

    private string PdfFilePath { get; set; } = string.Empty;
    private bool ShowPdfView { get; set; }
    private bool ShowUpdateView { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _emailConfiguration.Username = "msindialeave@gmail.com";
        _emailConfiguration.Password = "wxmq bhll fxap amad";
        _emailConfiguration.Subject = "Test Subject";
        _emailConfiguration.Content = "Test Content";
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;

        await using var stream = file.OpenReadStream(10485760);

        await OnConvertToText(stream);

        using var ms = new MemoryStream();
        await file.OpenReadStream(10485760).CopyToAsync(ms);
        var buffer = ms.ToArray();

        OnAddEmailAttachment(buffer, file.Name);

        _liEmailConfigurationCollection.Add(_emailConfiguration);
    }

    private async Task OnPastePath(ChangeEventArgs args)
    {
        MainPath = args.Value?.ToString();

        await AddEmailConfigurationCollection();
    }

    private Task AddEmailConfigurationCollection()
    {
        if (!string.IsNullOrEmpty(MainPath))
        {
            string[] strPdfFiles = Directory.GetFiles(MainPath, "*.pdf", SearchOption.AllDirectories);

            foreach (var strPdfFile in strPdfFiles)
            {
                var services = new Services();
                var filePath = strPdfFile.Replace("\\", @"\");
                string[] strPdfText = services.ConvertToText(filePath);

                var email = new EmailConfiguration
                {
                    Username = _emailConfiguration.Username,
                    Password = _emailConfiguration.Password,
                    Subject = _emailConfiguration.Subject,
                    Content = _emailConfiguration.Content,
                    Receivers = strPdfText,

                    Attachment = new FileConfiguration
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath)
                    }
                };

                _liEmailConfigurationCollection.Add(email);
            }
        }

        return Task.CompletedTask;
    }

    private void OnSubmit()
    {
        var services = new Services();
        services.SendEmail(_liEmailConfigurationCollection.ToArray());
    }

    private async Task OnConvertToText(Stream pdfStream)
    {
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);

        var services = new Services();
        _emailConfiguration.Receivers = services.ConvertToText(memoryStream);

        //Using File Path
        //emailConfiguration.Receivers = services.ConvertToText(@"E:\Temp\Test1.pdf");
    }

    private void OnAddEmailAttachment(byte[] pdfStream, string strFileName)
    {
        _emailConfiguration.Attachment = new FileConfiguration
        {
            FileName = strFileName,
            Attachment = pdfStream
        };
    }

    private void OnDelete(EmailConfiguration deletableEmail)
    {
        _liEmailConfigurationCollection.Remove(deletableEmail);
    }

    private void OnUpdate()
    {
        _liEmailConfigurationCollection.Clear();

        AddEmailConfigurationCollection();
    }

    private void OnShowPDFView(FileConfiguration? fileConfiguration)
    {
        if (fileConfiguration == null) return;

        ShowPdfView = true;

        PdfFilePath = fileConfiguration.FilePath;
    }

    private void OnShowUpdateView()
    {
        if (string.IsNullOrEmpty(MainPath) && Js != null)
        {
            Js.InvokeVoidAsync("InitiateAlert", "Please select the Pdf file");

            return;
        }

        ShowUpdateView = true;
    }
}