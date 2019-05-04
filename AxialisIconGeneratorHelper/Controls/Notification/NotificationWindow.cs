#region Usings

using System;
using System.Windows;
using System.Windows.Media;
using AxialisIconGeneratorHelper.Utils;

#endregion

namespace AxialisIconGeneratorHelper.Controls.Notification
{
    public class NotificationWindow : Window
    {
        #region Static Constructors

        static NotificationWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(typeof(NotificationWindow)));
            WindowStyleProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(WindowStyle.None));
            AllowsTransparencyProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(true));
            BackgroundProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(Brushes.CadetBlue));
            TopmostProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(true));
            ShowInTaskbarProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(false));
            ShowActivatedProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(false));
            SizeToContentProperty.OverrideMetadata(typeof(NotificationWindow), new FrameworkPropertyMetadata(SizeToContent.Manual));
        }

        #endregion

        #region Protected Methods

        protected override void OnInitialized(EventArgs e)
        {
            WindowUtils.HideWindowFromAltTab(this);
            base.OnInitialized(e);
        }

        #endregion
    }
}