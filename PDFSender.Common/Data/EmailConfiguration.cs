﻿namespace PDFSender.Common.Data
{
    public class EmailConfiguration
    {
        public bool IsSelected { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string[] Receivers { get; set; } = { };
        public FileConfiguration Attachment { get; set; }
    }
}