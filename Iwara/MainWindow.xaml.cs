using Panuon.UI.Silver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Iwara.Script.Network.Base;
using static Iwara.Script.Class.Base;
using static Iwara.Script.Class.Analyser;
using static Iwara.Script.UIManager;
using Image = Iwara.Script.Class.Base.Image;
using System.Threading;
using System.Net;
using System.IO;

namespace Iwara
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {
        public int VideosCurrentIndex = 0;
        public int VideosSort = 0;
        public bool VideosInit = true;
        public int ImagesCurrentIndex = 0;
        public int ImagesSort = 0;
        public bool ImagesInit = true;
        public static string CurrentMediaUrl = "";
        public static class Settings
        {
            public static bool EnableDoH = true;
            public static bool EnableProxy = false;
            public static string ProxyServer;
            public static string ProxyPort;
            public static int Site = (int)Script.Class.Base.Site.Iwara;
            public static Dictionary<string, string> HostsList = new Dictionary<string, string>();
        }
        public MainWindow()
        {
            InitializeComponent();
            SetBaseHosts();
            /*
            Settings.HostsList.Add("i", "163.172.85.51");
            Settings.HostsList.Add("www", "66.206.15.50");
            Settings.HostsList.Add("ecchi", "66.206.15.50");
            */
        }
        private void SetBaseHosts()
        {
            SetHosts("www");
            SetHosts("ecchi");
            SetHosts("i");
        }

        #region SettingsUI
        private void Proxy_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)proxy.IsChecked)
            {
                servervalue.IsEnabled = true;
                proxyvalue.IsEnabled = true;
            }
            else
            {
                servervalue.IsEnabled = false;
                proxyvalue.IsEnabled = false;
            }
        }

        private void DoH_Changed(object sender, RoutedEventArgs e)
        {
            Settings.EnableDoH = (bool)doh.IsChecked;
        }

        private void Proxy_Correct(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void Proxy_Value_Changed(object sender, TextChangedEventArgs e)
        {
            if (servervalue.Text != "" && proxyvalue.Text != "")
            {
                Settings.EnableProxy = true;
                Settings.ProxyServer = servervalue.Text;
                Settings.ProxyPort = proxyvalue.Text;
            }
            else
            {
                Settings.EnableProxy = false;
            }
        }

        private void Site_Changed(object sender, SelectionChangedEventArgs e)
        {
            Settings.Site = site.SelectedIndex;

            ImagesInit = true;
            ImagesCurrentIndex = 0;
            ImagesSort = imagesSort.SelectedIndex;
            imagesWaterfall.Children.RemoveRange(0, imagesWaterfall.Children.Count);

            VideosInit = true;
            VideosCurrentIndex = 0;
            VideosSort = videosSort.SelectedIndex;
            videosWaterfall.Children.RemoveRange(0, videosWaterfall.Children.Count);
        }
        private void FlushDNS(object sender, RoutedEventArgs e)
        {
            SetBaseHosts();
        }

        #endregion


        private string GetSite(int i)
        {
            string result = "www";
            switch (i)
            {
                case (int)Site.Iwara:
                    result = "www";
                    break;
                case (int)Site.Ecchi:
                    result = "ecchi";
                    break;
            }
            return result;
        }
        private string GetSort(int i)
        {
            string result = "date";
            switch (i)
            {
                case (int)Sort.Date:
                    result = "date";
                    break;
                case (int)Sort.Views:
                    result = "views";
                    break;
                case (int)Sort.Likes:
                    result = "likes";
                    break;
            }
            return result;
        }

        public UI.Preview.MediaPreview GetMediaPreviewControl(string url, string coverUrl, string title, string views, string likes, bool nocover = false, bool isgallery = false)
        {
            return new UI.Preview.MediaPreview
            {
                Width = 200d,
                Height = 200d,
                NoCover = nocover ? "True" : "False",
                IsGallery = isgallery ? "True" : "False",
                Title = title,
                Views = views,
                Likes = likes,
                Url = url,
                CoverUrl = coverUrl
            };
        }

        #region PreviewUI
        private void VideosWaterfall_LazyLoading(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) { return; }
            AppendVideos();
        }
        private void AppendVideos()
        {
            HttpWebRequest request = GetBaseRequest(AnalyesUrl("https://" + GetSite(Settings.Site) + ".iwara.tv/videos?sort=" + GetSort(VideosSort) + "&page=" + VideosCurrentIndex.ToString()));
            request.BeginGetResponse(new AsyncCallback(AppendVideosOnResponse), request);
        }
        private void AppendVideosOnResponse(IAsyncResult ar)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                        string httpResult = streamReader.ReadToEnd();
                        streamReader.Close();
                        response.Close();
                        if (httpResult.Substring(0, 4) != "error")
                        {
                            ArrayList videoslist = AnalyseVideoPreviewHtml(httpResult);

                            foreach (VideoShort videoShort in videoslist)
                            {
                                UI.Preview.MediaPreview imagePreview = GetMediaPreviewControl(videoShort.url, videoShort.coverUrl, videoShort.title, videoShort.views, videoShort.likes, videoShort.noCover);
                                imagePreview.LoadMediaEvent += new UI.Preview.MediaPreview.LoadMediaEventHandler(LoadMedia);
                                imagePreview.cover.LoadMediaEvent += new UI.Control.ImageReloadable.LoadMediaEventHandler(LoadMedia);
                                videosWaterfall.Children.Add(imagePreview);
                            }
                            VideosCurrentIndex++;
                        }
                        else
                        {
                            if (LogError("Load Videos Error:\n" + response.StatusCode) == "Yes") { AppendVideos(); }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogError("Load Videos Error:\n" + e.Message) == "Yes") { AppendVideos(); }
                }
            }));
        }

        private void AppendImages()
        {
            HttpWebRequest request = GetBaseRequest(AnalyesUrl("https://" + GetSite(Settings.Site) + ".iwara.tv/images?sort=" + GetSort(ImagesSort) + "&page=" + ImagesCurrentIndex.ToString()));
            request.BeginGetResponse(new AsyncCallback(AppendImagesOnResponse), request);
        }
        private void AppendImagesOnResponse(IAsyncResult ar)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                        string httpResult = streamReader.ReadToEnd();
                        streamReader.Close();
                        response.Close();
                        if (httpResult.Substring(0, 4) != "error")
                        {
                            ArrayList imageslist = AnalyseImagePreviewHtml(httpResult);

                            foreach (ImageShort imageShort in imageslist)
                            {
                                UI.Preview.MediaPreview imagePreview = GetMediaPreviewControl(imageShort.url, imageShort.coverUrl, imageShort.title, imageShort.views, imageShort.likes, false, imageShort.isGallery);
                                imagePreview.LoadMediaEvent += new UI.Preview.MediaPreview.LoadMediaEventHandler(LoadMedia);
                                imagePreview.cover.LoadMediaEvent += new UI.Control.ImageReloadable.LoadMediaEventHandler(LoadMedia);
                                imagesWaterfall.Children.Add(imagePreview);
                            }
                            ImagesCurrentIndex++;
                        }
                        else
                        {
                            if (LogError("Load Images Error:\n" + response.StatusCode) == "Yes") { AppendImages(); }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogError("Load Images Error:\n" + e.Message) == "Yes") { AppendImages(); }
                }
            }));
        }
        private void ImagesWaterfall_LazyLoading(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) { return; }
            AppendImages();
        }

        private void VideosSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (videosWaterfall == null) { return; }
            VideosCurrentIndex = 0;
            VideosSort = videosSort.SelectedIndex;
            videosWaterfall.Children.RemoveRange(0, videosWaterfall.Children.Count);
            AppendVideos();
        }
        private void ImagesSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imagesWaterfall == null) { return; }
            ImagesCurrentIndex = 0;
            ImagesSort = imagesSort.SelectedIndex;
            imagesWaterfall.Children.RemoveRange(0, imagesWaterfall.Children.Count);
            AppendImages();
        }
        private void MainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTab.SelectedIndex == 0 && VideosInit)
            {
                AppendVideos();
                VideosInit = false;
            }
            if (mainTab.SelectedIndex == 1 && ImagesInit)
            {
                AppendImages();
                ImagesInit = false;
            }
        }
        #endregion
        public void LoadMedia()
        {
            if (CurrentMediaUrl.Contains("images"))
            {
                LoadImage("https://" + GetSite(Settings.Site) + ".iwara.tv"+CurrentMediaUrl);
            }
            else if (CurrentMediaUrl.Contains("video"))
            {
                LoadVideo("https://" + GetSite(Settings.Site) + ".iwara.tv" + CurrentMediaUrl);
            }
        }

        public void LoadVideo(string videoUrl)
        {
            videosPreview.Visibility = Visibility.Hidden;
            videoMain.Visibility = Visibility.Visible;
            string httpResult = GetTextFromIwara(AnalyesUrl(videoUrl));
            if (httpResult.Substring(0, 4) != "error")
            {
                Video video = AnalyseVideoHtml(httpResult);
                videoMainPlayer.Children.Add(new UI.View.VideoView { Video = video });
            }
            else
            {
                if (LogError("Load video error:\n" + httpResult.Remove(0, 7)) == "Yes") { LoadVideo(videoUrl); }
            }
        }

        private void VideoMainBack(object sender, RoutedEventArgs e)
        {
            videoMainPlayer.Children.RemoveRange(0, videoMainPlayer.Children.Count);
            videosPreview.Visibility = Visibility.Visible;
            videoMain.Visibility = Visibility.Hidden;
        }

        public void LoadImage(string imageUrl)
        {
            imagesPreview.Visibility = Visibility.Hidden;
            imageMain.Visibility = Visibility.Visible;
            string httpResult = GetTextFromIwara(AnalyesUrl(imageUrl));
            if (httpResult.Substring(0, 4) != "error")
            {
                Image image = AnalyseImageHtml(httpResult);
                imageMainPlayer.Children.Add(new UI.View.ImageView { Image = image });
            }
            else
            {
                if (LogError("Load image error:\n" + httpResult.Remove(0, 7)) == "Yes") { LoadImage(imageUrl); }
            }
        }
        private void ImageMainBack(object sender, RoutedEventArgs e)
        {
            imageMainPlayer.Children.RemoveRange(0, imageMainPlayer.Children.Count);
            imagesPreview.Visibility = Visibility.Visible;
            imageMain.Visibility = Visibility.Hidden;
        }
    }
}
