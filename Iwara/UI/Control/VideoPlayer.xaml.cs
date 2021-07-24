using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Iwara.UI.Control
{
    /// <summary>
    /// VideoPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        public Dictionary<string, string> UrlList;
        public Dictionary<int, string> UrlIdList = new Dictionary<int, string>();
        public string CurrentUrl;
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public VideoPlayer()
        {
            InitializeComponent();
        }
        public void LoadVideo()
        {
            vlcPlayer.SourceProvider.MediaPlayer.Play(CurrentUrl);
        }
        public void Init()
        {
            DirectoryInfo vlcLibDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "libvlc\\");
            if (MainWindow.Settings.EnableProxy)
            {
                vlcPlayer.SourceProvider.CreatePlayer(vlcLibDirectory, new string[] { "--http-proxy=" + MainWindow.Settings.ProxyServer + ":" + MainWindow.Settings.ProxyPort });
            }
            else
            {
                vlcPlayer.SourceProvider.CreatePlayer(vlcLibDirectory);
            }
            volumeBar.Value = 100;
            InitTimer();
            int i = 0;
            foreach (var keys in UrlList)
            {
                definition.Items.Add(keys.Key);
                UrlIdList.Add(i, keys.Key);
                i++;
            }
            CurrentUrl = UrlList["Source"];
            definition.SelectedIndex = 0;
        }
        public Dictionary<string,string> VideoUrl
        {
            get { return VideoUrl; }
            set {
                UrlList = value;
                Init();
            }
        }
        private void ReloadVideo()
        {
            if (vlcPlayer.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Ended)
            {
                vlcPlayer.SourceProvider.MediaPlayer.ResetMedia();
            }
            try
            {
                vlcPlayer.SourceProvider.MediaPlayer.Play(CurrentUrl);
            }
            catch (Exception)
            {
            }
        }
        public void InitTimer()
        {
            if (!dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
                dispatcherTimer.Tick += new EventHandler((sender, e) =>
                {
                    timeLength.Text = FormatIntToTimeString(vlcPlayer.SourceProvider.MediaPlayer.Length);
                    rateBar.Value = Convert.ToInt32(vlcPlayer.SourceProvider.MediaPlayer.Position * 100000);
                    time.Text = FormatIntToTimeString(vlcPlayer.SourceProvider.MediaPlayer.Time);
                    if (vlcPlayer.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Ended)
                    {
                        pause.Visibility = Visibility.Hidden;
                        play.Visibility = Visibility.Visible;
                        dispatcherTimer.IsEnabled = false;
                    }
                });
                dispatcherTimer.IsEnabled = true;
            }
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (vlcPlayer.SourceProvider.MediaPlayer == null) { return; }
            if (vlcPlayer.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.NothingSpecial ||
            vlcPlayer.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Stopped ||
            vlcPlayer.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Ended)
            {
                ReloadVideo();
            }
            if (vlcPlayer.SourceProvider.MediaPlayer.IsPlaying())
            {
                dispatcherTimer.IsEnabled = false;
                pause.Visibility = Visibility.Hidden;
                play.Visibility = Visibility.Visible;
                vlcPlayer.SourceProvider.MediaPlayer.Pause();
            }
            else
            {
                dispatcherTimer.IsEnabled = true;
                pause.Visibility = Visibility.Visible;
                play.Visibility = Visibility.Hidden; 
                vlcPlayer.SourceProvider.MediaPlayer.Play();
            }
        }
        private void RateBar_DragStarted(object sender, DragStartedEventArgs e)
        {
            dispatcherTimer.IsEnabled = false;
        }
        private void RateBar_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (vlcPlayer.SourceProvider.MediaPlayer == null) { return; }
            dispatcherTimer.IsEnabled = true;
            vlcPlayer.SourceProvider.MediaPlayer.Position = (float)(rateBar.Value / 100000d);
        }
        public static string FormatIntToTimeString(long i)
        {
            int minute = 0;
            int second = (int)i / 1000;

            if (second > 60)
            {
                minute = second / 60;
                second %= 60;
            }
            return minute.ToString() + ":" + second.ToString("00");
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vlcPlayer.SourceProvider.MediaPlayer.Audio.Volume = (int)e.NewValue;
        }

        private void Definition_Changed(object sender, SelectionChangedEventArgs e)
        {
            CurrentUrl = UrlList[UrlIdList[definition.SelectedIndex]];
            LoadVideo();
            dispatcherTimer.IsEnabled = true;
            pause.Visibility = Visibility.Visible;
            play.Visibility = Visibility.Hidden;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.IsEnabled = false;
            vlcPlayer.Dispose();
        }
    }
}
