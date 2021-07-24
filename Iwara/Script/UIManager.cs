using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using static Iwara.Script.Class.Base;

namespace Iwara.Script
{
    class UIManager
    {
        public static string LogError(string info)
        {
            return Convert.ToString(MessageBoxX.Show(info, "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                MessageBoxStyle = MessageBoxStyle.Classic,
                MessageBoxIcon = MessageBoxIcon.Error,
                YesButton = "Retry",
                NoButton = "Cancel",
                ButtonBrush = "#FF4C4C".ToColor().ToBrush(),
            }));
        }

        public static string LogErrorNoRetry(string info)
        {
            return Convert.ToString(MessageBoxX.Show(info, "Error", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                MessageBoxStyle = MessageBoxStyle.Classic,
                MessageBoxIcon = MessageBoxIcon.Error,
                OKButton = "Yes",
                ButtonBrush = "#FF4C4C".ToColor().ToBrush(),
            }));
        }

        private void UIPreviewAdjust(Grid grid, int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    UI.Preview.MediaPreview tempControl = (UI.Preview.MediaPreview)grid.FindName("MediaPreview" + Convert.ToString((x * j) + i));
                    if (tempControl != null)
                    {
                        tempControl.VerticalAlignment = VerticalAlignment.Top;
                        tempControl.HorizontalAlignment = HorizontalAlignment.Left;
                        tempControl.Margin = new Thickness
                        {
                            Left = i * 205,
                            Top = j * 205
                        };
                    }
                }
            }
        }
    }
}
