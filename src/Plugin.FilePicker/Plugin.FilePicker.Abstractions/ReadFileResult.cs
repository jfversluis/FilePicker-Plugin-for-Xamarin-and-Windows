namespace Plugin.FilePicker.Abstractions
{
    public class ReadFileResult
    {
        public byte[] Data { get; set; }
        public bool IsFileSizeTooLarge { get; set; }
    }
}