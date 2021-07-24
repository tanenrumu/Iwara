using Panuon.UI.Silver;
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
using System.Windows.Threading;
using static Iwara.Script.Class.Analyser;
using static Iwara.Script.Network.Base;


namespace Iwara.UI.Preview
{
    /// <summary>
    /// MediaPreview.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPreview : UserControl
    {
        public string url;
        public string coverUrl;
        public MediaPreview()
        {
            InitializeComponent();
        }
        public string Likes
        {
            get { return likes.Text; }
            set { likes.Text = value; }
        }
        public string Views
        {
            get { return views.Text; }
            set { views.Text = value; }
        }
        public string Title
        {
            get { return title.Text; }
            set { title.Text = value; }
        }
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        public string CoverUrl
        {
            get { return coverUrl; }
            set
            {
                if (NoCover == "False")
                {
                    cover.ImageUrl = value;
                    cover.MediaUrl = url;
                }
            }
        }
        public string NoCover
        {
            get
            {
                switch (nocover.Visibility)
                {
                    case Visibility.Visible:
                        return "True";
                    case Visibility.Hidden:
                        return "False";
                    default:
                        return "False";
                }
            }
            set
            {
                if (value == "True")
                {
                    cover.Visibility = Visibility.Hidden;
                    nocover.Visibility = Visibility.Visible;
                }
                else
                {
                    cover.Visibility = Visibility.Visible;
                    nocover.Visibility = Visibility.Hidden;
                }
            }
        }
        public string IsGallery
        {
            get
            {
                switch (gallery.Visibility)
                {
                    case Visibility.Visible:
                        return "True";
                    case Visibility.Hidden:
                        return "False";
                    default:
                        return "False";
                }
            }
            set
            {
                gallery.Visibility = value == "True" ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public delegate void LoadMediaEventHandler();
        public event LoadMediaEventHandler LoadMediaEvent;
        private void LoadMedia(object sender, MouseButtonEventArgs e)
        {
            MainWindow.CurrentMediaUrl = url;
            LoadMediaEvent();
        }
    }
}