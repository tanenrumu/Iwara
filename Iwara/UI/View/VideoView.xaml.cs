using HTMLConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Iwara.Script.Class.Base;

namespace Iwara.UI.View
{
    /// <summary>
    /// VideoView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoView : UserControl
    {
        public string youtubeUrl;
        public VideoView()
        {
            InitializeComponent();
        }
        public Video Video
        {
            set
            {
                UserShort author = value.author;

                authorAvatar.ImageSource = new BitmapImage(new Uri("https:" + author.avatarUrl));
                authorName.Text = author.userName;

                string xaml = HtmlToXamlConverter.ConvertHtmlToXaml(value.description, true);
                FlowDocument flowDocument = XamlReader.Parse(xaml) as FlowDocument;
                description.Document = flowDocument;

                likes.Text = value.likes;
                views.Text = value.views;
                date.Text = value.date;
                title.Text = value.title;

                if (value.isYouTube)
                {
                    youtubeUrl = "https:" + value.YouTubeUrl;
                    player.Visibility = Visibility.Hidden;
                    youtubeInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    player.VideoUrl = value.videosUrlList;
                }
            }
        }

        private void youtubeInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(youtubeUrl);
        }
    }
}
