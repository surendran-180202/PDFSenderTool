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
    private byte[]? PdfByteData { get; set; }
    private bool ShowPdfView { get; set; }
    private bool ShowUpdateView { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await GetLocalStoreData();
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;

        await using var stream = file.OpenReadStream(10485760);

        await OnConvertToText(stream);

        using var ms = new MemoryStream();
        await file.OpenReadStream(10485760).CopyToAsync(ms);
        var buffer = ms.ToArray();

        PdfByteData = buffer;

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
            var services = new Services();

            foreach (var strPdfFile in strPdfFiles)
            {
                var filePath = strPdfFile.Replace("\\", @"\");
                string[] strPdfText;
                try
                {
                    strPdfText = services.ConvertToText(filePath);
                }
                catch (Exception ex)
                {
                    Js.InvokeVoidAsync("InitiateAlert", $"PDF file reading error - File Name: {Path.GetFileName(strPdfFile)}");
                    Js.InvokeVoidAsync("AppendConsoleError", ex.ToString());
                    continue;
                }

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
        if (!_liEmailConfigurationCollection.Any())
        {
            Js.InvokeVoidAsync("InitiateAlert", "Please select the Pdf file");

            return;
        }

        var services = new Services();
        services.SendEmail(_liEmailConfigurationCollection.ToArray());
    }

    private async Task OnConvertToText(Stream pdfStream)
    {
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);

        var services = new Services();
        _emailConfiguration.Receivers = services.ConvertToText(memoryStream);
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

        SetLocalStoreData();
    }

    private void OnClear()
    {
        this._liEmailConfigurationCollection.Clear();
    }

    private void OnShowPDFView(FileConfiguration? fileConfiguration)
    {
        if (fileConfiguration == null) return;

        ShowPdfView = true;

        PdfFilePath = fileConfiguration.FilePath;
    }

    private void OnShowUpdateView()
    {
        if ((string.IsNullOrEmpty(MainPath) && PdfByteData == null) && Js != null)
        {
            Js.InvokeVoidAsync("InitiateAlert", "Please select the Pdf file");

            return;
        }

        ShowUpdateView = true;
    }

    private void SetLocalStoreData()
    {
        LocalStore.SetAsync("Username", _emailConfiguration.Username);
        LocalStore.SetAsync("Password", _emailConfiguration.Password);
        LocalStore.SetAsync("DisplayName", _emailConfiguration.DisplayName);
        LocalStore.SetAsync("Subject", _emailConfiguration.Subject);
        LocalStore.SetAsync("Content", _emailConfiguration.Content);
    }

    private async Task GetLocalStoreData()
    {
        var username = await LocalStore.GetAsync<string>("Username");
        var password = await LocalStore.GetAsync<string>("Password");
        var displayName = await LocalStore.GetAsync<string>("DisplayName");
        var subject = await LocalStore.GetAsync<string>("Subject");
        var content = await LocalStore.GetAsync<string>("Content");

        _emailConfiguration.Username = username.Success ? username.Value : string.Empty;
        _emailConfiguration.Password = password.Success ? password.Value : string.Empty;
        _emailConfiguration.DisplayName = displayName.Success ? displayName.Value : string.Empty;
        _emailConfiguration.Subject = subject.Success ? subject.Value : string.Empty;
        _emailConfiguration.Content = content.Success ? content.Value : string.Empty;
    }
}