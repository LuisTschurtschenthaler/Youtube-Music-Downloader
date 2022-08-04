using System.ComponentModel;
using YoutubeExplode.Videos;


namespace Youtube_Music_Downloader {
	internal enum Status {
		Waiting,
		Downloading,
		Apply_Metadata,
		Finished,
		Error,
		Error_File_Exists
	}


	internal class Download : INotifyPropertyChanged {
		
		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string propertyName) {
			if(PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _artist = "";
		private string _title = "";
		private string _subfolder = "";
		private Status _status = Status.Waiting;


		public string Artist {
			get { return _artist; }
			set {
				_artist = value;
				OnPropertyChanged("Artist");
			}
		}

		public string Title {
			get { return _title; }
			set {
				_title = value;
				OnPropertyChanged("Title");
			}
		}

        public string Subfolder {
			get { return _subfolder; }
			set {
				_subfolder = value;
				OnPropertyChanged("Subfolder");
			}
		}

		public Status Status {
			get { return _status; }
			set {
				_status = value;
				OnPropertyChanged("Status");
			}
		}

        public Video Video { get; set; }
        public VideoId VideoID { get; set; }
		public string Url { get; set; }

	}
}
