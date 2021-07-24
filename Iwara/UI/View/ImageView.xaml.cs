using HTMLConverter;
using System;
using System.Collections.Generic;
using System.IO;
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
using Image = Iwara.Script.Class.Base.Image;

namespace Iwara.UI.View
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : UserControl
    {
        public ImageView()
        {
            InitializeComponent();

        }
        public Image Image
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

                image.UrlList = value.imagesUrlList;
            }
        }
    }
}
