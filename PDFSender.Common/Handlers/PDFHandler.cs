﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aspose.Pdf;
using Aspose.Pdf.Text;

namespace PDFSender.Common.Handlers
{
    internal class PdfHandler
    {
        internal List<string> ExtractGmailEmails(string strPdfText)
        {
            var liEmailCollection = new List<string>();

            var strEmailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            //string strEmailPattern = new Regex(@"[\w\.-]+@gmail\.com");

            var regex = new Regex(strEmailPattern);

            var matches = regex.Matches(strPdfText);

            foreach (Match match in matches) liEmailCollection.Add(match.Value);

            return liEmailCollection;
        }

        internal string ConvertToText(string strFilePath)
        {
            return GetTextFromPdf(new Document(strFilePath));
        }

        internal string ConvertToText(MemoryStream stream)
        {
            if (stream == null || !stream.CanRead)
                throw new ArgumentException("Invalid PDF stream");

            return GetTextFromPdf(new Document(stream));
        }

        private string GetTextFromPdf(Document pdfDocument)
        {
            var sbText = new StringBuilder();
            for (var page = 1; page <= pdfDocument.Pages.Count; page++)
            {
                var textAbsorber = new TextAbsorber();
                pdfDocument.Pages[page].Accept(textAbsorber);
                sbText.Append(textAbsorber.Text);
            }

            return sbText.ToString();
        }
    }
}