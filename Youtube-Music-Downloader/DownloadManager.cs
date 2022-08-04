using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;


namespace Youtube_Music_Downloader {
    internal class DownloadManager {

        public ObservableCollection<Download> Downloads { get; private set; }
        public ICollectionView VideoDataView { get; private set; }

        private YoutubeClient youtube;


        public DownloadManager() {
            Downloads = new ObservableCollection<Download>();
            VideoDataView = CollectionViewSource.GetDefaultView(Downloads);
            youtube = new YoutubeClient();;
        }


        public async Task AddToDownload(string url) {
            var video = await youtube.Videos.GetAsync(url);

            Downloads.Add(
                new Download() {
                    Artist = video.Author.ChannelTitle.Replace(" - Topic", ""),
                    Title = video.Title,
                    Subfolder = "",
                    Video = video,
                    VideoID = VideoId.Parse(url),
                    Description = video.Description
                }
            );
        }

        public async Task StartDownload(Download download, string downloadFolder, string fileName) {
            var file = $"{downloadFolder}{fileName}";
            var audioFilePath = $"{file}.mp3";

            // Download video
            download.Status = Status.Downloading;
            var stream = await youtube.Videos.Streams.GetManifestAsync(download.VideoID);
            var audioStreamInfo = stream.GetAudioStreams().GetWithHighestBitrate();
            await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath);

            // Apply metadata to audio file
            download.Status = Status.Finishing;

            var songInfo = new SongInfo(file, download);
            var tFile = TagLib.File.Create(audioFilePath);
            tFile.Tag.Title = songInfo.Title;
            tFile.Tag.Album = songInfo.Album;
            tFile.Tag.AlbumArtists = new string[] { songInfo.Performers[0] };
            tFile.Tag.Performers = songInfo.Performers.ToArray();
            tFile.Tag.Composers = songInfo.Composers.ToArray();
            tFile.Tag.Year = songInfo.ReleaseYear;
            tFile.Save();

            download.Status = Status.Finished;
        }

    }
}
