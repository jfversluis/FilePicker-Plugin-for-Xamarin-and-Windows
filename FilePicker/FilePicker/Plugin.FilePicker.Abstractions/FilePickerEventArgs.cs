using System;
namespace Plugin.FilePicker.Abstractions
{
	public class FilePickerEventArgs : EventArgs
	{
		public byte[] FileByte { get; set; }

		public string FileName { get; set; }
		public FilePickerEventArgs()
		{

		}

		public FilePickerEventArgs(byte[] fileByte)
		{
			FileByte = fileByte;
		}

		public FilePickerEventArgs(byte[] fileByte, string fileName)
		{
			FileByte = fileByte;
			FileName = fileName;
		}


	}
}

