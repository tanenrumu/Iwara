using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Iwara.UI.Control
{
    /// <summary>
    /// ImagePlayer.xaml 的交互逻辑
    /// </summary>
    public partial class ImagePlayer : UserControl
    {
        public ImagePlayer()
        {
            InitializeComponent();
        }

        public ArrayList UrlList
        {
            set {
                if (value.Count == 1)
                {
                    next.Visibility = Visibility.Hidden;
                    last.Visibility = Visibility.Hidden;
                }
                foreach(string url in value)
                {
                    carousel.Children.Add(new ImageReloadable
                    {
                        ImageUrl = url
                    });
                }
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            carousel.Index++;
        }

        private void Last_Click(object sender, RoutedEventArgs e)
        {
            carousel.Index--;
        }
    }
}
