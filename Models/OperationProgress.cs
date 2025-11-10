namespace FileManagerApp.Models
{
    /// <summary>
    /// Represents progress of a long-running operation
    /// </summary>
    public class OperationProgress
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public string CurrentItem { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;

        public OperationProgress() { }

        public OperationProgress(int current, int total, string currentItem = "", string message = "")
        {
            Current = current;
            Total = total;
            CurrentItem = currentItem;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Current}/{Total} ({Percentage:F1}%) - {Message}";
        }
    }
}
