namespace ElSentidoDelOido.Web.Models
{
    public class ContactReplyEmailViewModel
    {
        public string ContactName { get; set; } = string.Empty;
        public string OriginalMessage { get; set; } = string.Empty;
        public string ReplyMessage { get; set; } = string.Empty;
        public string ReplyDate { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string? LogoDataUri { get; set; }
    }
}
