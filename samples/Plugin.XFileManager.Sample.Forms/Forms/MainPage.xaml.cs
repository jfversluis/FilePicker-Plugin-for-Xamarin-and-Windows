using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Plugin.XFileManager.Sample.Forms
{
	public partial class MainPage : ContentPage
	{
		string[] imageTypes = null;

		public MainPage()
		{
			InitializeComponent();
			BindingContext = this;



				imageTypes = new string[] { ".jpg" ,".png",".tiff" };

		}

		private async void btn_PickFolder(object sender, EventArgs e)
		{
			(var success, var pickedFolder) = await FileManager.PickFolder().ConfigureAwait(true);
			if (success)
			{
				lbl_PickFolderPath.Text = pickedFolder.FolderPath;
			}

		}

		private async void btn_PickFile(object sender, EventArgs e)
		{
			string[] allowedtypes = null;
			(bool success, FileData currPickedFile) = await FileManager.PickFile(allowedtypes).ConfigureAwait(true);
			if (success)
			{
				updatePickedFile(currPickedFile);

				lbl_PickFilePathNOTUSED.Text = currPickedFile.FilePath;
			}
		}

		private async void btn_OpenPickedFile(object sender, EventArgs e)
		{
			string[] allowedtypes = null;
			(bool success, FileData currPickedFile) = await FileManager.PickFile(allowedtypes).ConfigureAwait(true);
			if (success)
			{
				updatePickedFile(currPickedFile);
				lbl_OpenPickedFile2.Text = currPickedFile.FilePath;
				await FileManager.OpenFile(currPickedFile.FilePath).ConfigureAwait(true);
			}
		}


		private async void btn_PickImageAndSaveLocalAppFolder(object sender, EventArgs e)
		{
			(bool successPickFile, FileData pickedFile) = await FileManager.PickFile(imageTypes).ConfigureAwait(true);
			if (successPickFile)
			{
				updatePickedFile(pickedFile);
				Stream imgStream = pickedFile.GetStream();
				imgStream.Position = 0;
				ImageSource img = ImageSource.FromStream(() => imgStream);

				lbl_PickLoadImageNSaveLocal.Text = pickedFile.FilePath;

				img_PickLoadImage.Source = img;

				//Create a new FileData and input the filepath, folder path, and filename and stream. This will get sent to the save function

				var localFolder = FileManager.GetLocalAppFolder();

				var currMemoryFile = new FileData(localFolder+ pickedFile.FileName, pickedFile.FileName, () => pickedFile.GetStream());
				updateMemoryFile(currMemoryFile);
				bool successSaveFile = await FileManager.SaveFileToLocalAppStorage(currMemoryFile).ConfigureAwait(true);

				if (successSaveFile)
				{
					(bool successGetStream, Stream fileStream) = await FileManager.GetStreamFromPath(currMemoryFile.FilePath).ConfigureAwait(true);
					if (successGetStream)
					{
						updateFileSaved(currMemoryFile.FilePath, fileStream);
					}
					else
					{
						updateFileSaved("Unable to find/read saved file!", new MemoryStream());
					}
				}


			}


		}

		private async void btn_PickImageAndSaveToPickedFolder(object sender, EventArgs e)
		{
			
			(var success2, var pickedFile) = await FileManager.PickFile(imageTypes).ConfigureAwait(true);
			if (success2)
			{


				(var success, var pickedFolder) = await FileManager.PickFolder().ConfigureAwait(true);

				if (success)
				{
					updatePickedFile(pickedFile);
					Stream imgStream = pickedFile.GetStream();
					imgStream.Position = 0;
					ImageSource img = ImageSource.FromStream(() => imgStream);

					lbl_PickLoadImageNSaveLocal.Text = pickedFile.FilePath;

					img_PickLoadImage.Source = img;

					//Create a new FileData and input the filepath, folder path, and filename and stream. This will get sent to the save function


					var currMemoryFile = new FileData(pickedFolder + pickedFile.FileName, pickedFile.FileName, () => pickedFile.GetStream());
					updateMemoryFile(currMemoryFile);
					bool successSaveFile = await FileManager.SaveFileToLocalAppStorage(currMemoryFile).ConfigureAwait(true);

					if (successSaveFile)
					{
						(bool successGetStream, Stream fileStream) = await FileManager.GetStreamFromPath(currMemoryFile.FilePath).ConfigureAwait(true);
						if (successGetStream)
						{
							updateFileSaved(currMemoryFile.FilePath, fileStream);
						}
						else
						{
							updateFileSaved("Unable to find/read saved file!", new MemoryStream());
						}
					}
				}
			}

		}




		private async void updatePickedFile(FileData currPickedFile)
		{
			currPickedFilePath.Text = currPickedFile.FilePath;
			currPickedFileName.Text = currPickedFile.FileName;

			currPickedStreamSize.Text = currPickedFile.DataArray.Length.ToString();
		}

		private async void updateMemoryFile(FileData currMemoryFile)
		{
			currMemoryFileFilePath.Text = currMemoryFile.FilePath;
			currMemoryFileFileName.Text = currMemoryFile.FileName;
			currMemoryFileStreamSize.Text = currMemoryFile.DataArray.Length.ToString();
		}


		private async void updateFileSaved(string filePath, Stream fileStream)
		{
			currFileSavedFolderPath.Text = Path.GetDirectoryName(filePath);
			currFileSavedFilePath.Text = filePath;
			currFileSavedFileName.Text = Path.GetFileName(filePath);
			currFileSavedStreamSize.Text = fileStream.Length.ToString();
		}

	}
}