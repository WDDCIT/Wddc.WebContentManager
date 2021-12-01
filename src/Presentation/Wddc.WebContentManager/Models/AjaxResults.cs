namespace Wddc.WebContentManager.Models
{
    public class AjaxResults
    {
        public string Message { get; set; }
        public ResultStatus ResultStatus { get; set; }
        public object Results { get; set; }
        public int TotalItemCount { get; set; }
    }

    public enum ResultStatus
    {
        Success = 1,
        Warning = 5,
        Error = 10,
    }
}