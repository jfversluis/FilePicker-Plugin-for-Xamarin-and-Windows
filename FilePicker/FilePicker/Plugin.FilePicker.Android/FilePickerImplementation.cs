using System;
using System;
using Android.Content;
using System.Threading.Tasks;
using System.Threading;

using Plugin.FilePicker.Abstractions;
using Java.IO;

namespace Plugin.FilePicker
{
	/// <summary>
	/// Implementation for Feature
	/// </summary>
	/// 
	[Android.Runtime.Preserve(AllMembers = true)]
	public class FilePickerImplementation : IFilePicker
	{

		private Context context;
		private int requestId;
		private TaskCompletionSource<FileData> completionSource;

		public FilePickerImplementation()
		{
			this.context = Android.App.Application.Context;
		}

		public async Task<FileData> PickFile()
		{
			var media = await TakeMediaAsync("file/*", Intent.ActionGetContent);

			return media;
		}

		private Task<FileData> TakeMediaAsync(string type, string action)
		{
			int id = GetRequestId();

			var ntcs = new TaskCompletionSource<FileData>(id);
			if (Interlocked.CompareExchange(ref this.completionSource, ntcs, null) != null)
				throw new InvalidOperationException("Only one operation can be active at a time");

			try
			{
				Intent pickerIntent = new Intent(this.context, typeof(FilePickerActivity));
				pickerIntent.SetFlags(ActivityFlags.NewTask);

				this.context.StartActivity(pickerIntent);

				EventHandler<FilePickerEventArgs> handler = null;

				handler = (s, e) =>
				{
					var tcs = Interlocked.Exchange(ref this.completionSource, null);

					FilePickerActivity.FilePicked -= handler;

					tcs.SetResult(new FileData()
					{
						DataArray = e.FileByte,
						FileName = e.FileName
					});
				};

				FilePickerActivity.FilePicked += handler;
			}
			catch (Exception exAct)
			{
				System.Diagnostics.Debug.Write(exAct);
			}

			return completionSource.Task;
		}

		private int GetRequestId()
		{
			int id = this.requestId;
			if (this.requestId == Int32.MaxValue)
				this.requestId = 0;
			else
				this.requestId++;

			return id;
		}

		public async Task<bool> SaveFile(FileData fileToSave)
		{
			try
			{
				File myFile = new File(Android.OS.Environment.ExternalStorageDirectory, fileToSave.FileName);

				if (myFile.Exists())
					return true;

				FileOutputStream fos = new FileOutputStream(myFile.Path);

				fos.Write(fileToSave.DataArray);
				fos.Close();

				return true;

			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public void OpenFile(File fileToOpen)
		{
			var uri = Android.Net.Uri.FromFile(fileToOpen);
			var intent = new Intent();
			var mime = IOUtil.GetMimeType(uri.ToString());
			intent.SetAction(Intent.ActionView);
			intent.SetDataAndType(uri, mime);
			intent.SetFlags(ActivityFlags.NewTask);

			context.StartActivity(intent);
		}

		public void OpenFile(string fileToOpen)
		{
			File myFile = new File(Android.OS.Environment.ExternalStorageState, fileToOpen);

			OpenFile(myFile);
		}

		public async void OpenFile(FileData fileToOpen)
		{
			File myFile = new File(Android.OS.Environment.ExternalStorageState, fileToOpen.FileName);

			if (!myFile.Exists())
				await SaveFile(fileToOpen);

			OpenFile(fileToOpen);
		}
	}
}
