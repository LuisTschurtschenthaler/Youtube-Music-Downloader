using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;


namespace Youtube_Music_Downloader {
    internal class DownloadManager {

        public ObservableCollection<Download> Downloads { get; private set; }
        public ICollectionView VideoDataView { get; private set; }
        public bool IsDownloading { get; private set; } = false;

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
                    Url = url
                }
            );
        }

        public async Task StartDownload(Download download, string downloadFolder, string fileName) {
            IsDownloading = true;

            var file = $"{downloadFolder}{fileName}";
            var audioFilePath = $"{file}.mp3";

            // Download video as mp3
            download.Status = Status.Downloading;
            await youtube.Videos.DownloadAsync(download.Url, audioFilePath);

            // Apply metadata to audio file
            download.Status = Status.Apply_Metadata;
            var songInfo = new SongInfo(file, download);
            var tFile = TagLib.File.Create(audioFilePath);
            tFile.Tag.Title = songInfo.Title;
            tFile.Tag.Album = songInfo.Album;
            tFile.Tag.AlbumArtists = new string[] { songInfo.Performers[0] };
            tFile.Tag.Performers = songInfo.Performers.ToArray();
            tFile.Tag.Composers = songInfo.Composers.ToArray();
            tFile.Tag.Year = songInfo.ReleaseYear;
            tFile.Save();

            SetAlbumArt(tFile, download);

            download.Status = Status.Finished;
            IsDownloading = false;
        }

        private static void SetAlbumArt(TagLib.File file, Download download) {
            var thumbnail = ThumbnailExtensions.GetWithHighestResolution(download.Video.Thumbnails);
            if(thumbnail == null) 
                return;

            var thumbnailImage = Utils.DownloadThumbnail(thumbnail);
            var cover = Utils.ResizeAndCropImage(thumbnail, thumbnailImage);

            file.Tag.Pictures = new TagLib.IPicture[] {
                new TagLib.Picture(cover)
            };
            file.Save();

            File.Delete(cover);
            //File.Delete(thumbnailImage);
        }
    }
}
