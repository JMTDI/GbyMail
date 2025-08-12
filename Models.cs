using System;
using System.Windows;

namespace GbyMail
{
    public enum SearchPlatform
    {
        Google,
        DuckDuckGo,
        Ebay,
        Amazon
    }

    public class EmailItem
    {
        public Guid Id { get; set; }
        public string From { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime Date { get; set; }
        public string? PdfAttachment { get; set; }

        public string FormattedDate => Date.ToString("HH:mm");
        public Visibility HasAttachmentVisibility => 
            string.IsNullOrEmpty(PdfAttachment) ? Visibility.Collapsed : Visibility.Visible;
    }
}