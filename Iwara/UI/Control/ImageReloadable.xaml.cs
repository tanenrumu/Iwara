using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using static Iwara.Script.Class.Analyser;
using System.ComponentModel;
using static Iwara.Script.Class.Base;
using System.Net;

namespace Iwara.UI.Control
{
    /// <summary>
    /// ImageReloadable.xaml 的交互逻辑
    /// </summary>
    public partial class ImageReloadable : UserControl
    {
        public string imageUrl;
        public string mediaUrl = "";
        public ImageReloadable()
        {
            InitializeComponent();
        }
        public string ImageUrl
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                LoadImage();
            }
        }
        public string MediaUrl
        {
            set
            {
                mediaUrl = value;
            }
        }
        public void LoadImage()
        {
            loading.IsRunning = true;
            error.Visibility = Visibility.Hidden;
            HttpWebRequest request = GetBaseRequest(AnalyesUrl(imageUrl));
            request.BeginGetResponse(new AsyncCallback(OnImageResponse), request);
        }

        private void OnImageResponse(IAsyncResult ar)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                        ImageSource imageSource = (ImageSource)imageSourceConverter.ConvertFrom(response.GetResponseStream());
                        if (imageSource != null)
                        {
                            image.Source = imageSource;
                            loading.IsRunning = false;
                        }
                    }
                    else
                    {
                        loading.IsRunning = false;
                        image.Visibility = Visibility.Hidden;
                        error.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception)
                {
                    loading.IsRunning = false;
                    image.Visibility = Visibility.Hidden;
                    error.Visibility = Visibility.Visible;
                }
            }));
        }

        private void ReloadImage(object sender, MouseButtonEventArgs e)
        {
            image.Visibility = Visibility.Visible;
            LoadImage();
        }

        public delegate void LoadMediaEventHandler();
        public event LoadMediaEventHandler LoadMediaEvent;
        private void LoadMedia(object sender, MouseButtonEventArgs e)
        {
            if (mediaUrl != "")
            {
                MainWindow.CurrentMediaUrl = mediaUrl;
                LoadMediaEvent();
            }
        }
    }
}
