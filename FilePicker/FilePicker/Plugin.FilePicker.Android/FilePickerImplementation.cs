using System;
using System;
using Android.Content;
using System.Threading.Tasks;
using System.Threading;

using Plugin.FilePicker.Abstractions;

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
    }
}