#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AxialisIconGeneratorHelper.Controls.Notification;

#endregion

namespace AxialisIconGeneratorHelper.Services
{
    public class NotificationService
    {
        #region Private Fields

        private static readonly List<NotificationArea> Areas = new List<NotificationArea>();

        private readonly Dispatcher dispatcher;
        private NotificationWindow window;

        #endregion

        #region Public Properties

        public NotificationPosition Position { get; set; } = NotificationPosition.BottomRight;

        public Size Size { get; set; } = new Size(300, 200);

        public double XOffset { get; set; } = 10;

        public double YOffset { get; set; } = 10;

        #endregion

        #region Not Static Constructors

        public NotificationService(Dispatcher dispatcher = null)
        {
            if (dispatcher == null) dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            this.dispatcher = dispatcher;
        }

        #endregion

        #region Public Methods

        public void Show(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null)
        {
            if (!this.dispatcher.CheckAccess())
            {
                this.dispatcher.BeginInvoke(new Action(() => this.Show(content, areaName, expirationTime, onClick, onClose)));
                return;
            }

            if (areaName == string.Empty && this.window == null)
            {
                var workArea = SystemParameters.WorkArea;

                this.window = new NotificationWindow
                {
                    Left = workArea.Left,
                    Top = workArea.Top,
                    Width = workArea.Width,
                    Height = workArea.Height
                };

                this.window.Show();
            }

            if (expirationTime == null) expirationTime = TimeSpan.FromSeconds(5);

            foreach (var area in Areas.Where(a => a.Name == areaName)) area.Show(content, (TimeSpan) expirationTime, onClick, onClose);
        }

        public static void UpdateAreaPosition(string areaName = "", NotificationPosition notificationPosition = NotificationPosition.BottomRight)
        {
            var area = Areas?.FirstOrDefault(a => a.Name == areaName);
            if (area != null) area.Position = notificationPosition;
        }

        #endregion

        #region Internal Methods

        internal static void AddArea(NotificationArea area) => Areas?.Add(area);

        #endregion
    }
}