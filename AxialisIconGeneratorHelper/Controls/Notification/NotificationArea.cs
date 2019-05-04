#region Usings

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AxialisIconGeneratorHelper.Services;

#endregion

namespace AxialisIconGeneratorHelper.Controls.Notification
{
    public class NotificationArea : Control
    {
        #region Public Fields

        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register("MaxItems", typeof(int), typeof(NotificationArea), new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));

        #endregion

        #region Private Fields

        private IList items;
        private readonly object syncObject = new object();

        #endregion

        #region Public Properties

        public int MaxItems
        {
            get => (int) this.GetValue(MaxItemsProperty);
            set => this.SetValue(MaxItemsProperty, value);
        }

        public NotificationPosition Position
        {
            get => (NotificationPosition) this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }

        #endregion

        #region Static Constructors

        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationArea),
                                                     new FrameworkPropertyMetadata(typeof(NotificationArea)));
        }

        #endregion

        #region Not Static Constructors

        public NotificationArea()
        {
            NotificationService.AddArea(this);
        }

        #endregion

        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var itemsControl = this.GetTemplateChild("PART_Items") as Panel;
            lock (this.syncObject)
            {
                this.items = itemsControl?.Children;
            }
        }

        public async void Show(object content, TimeSpan expirationTime, Action onClick, Action onClose)
        {
            var notification = new Notification
            {
                Content = content
            };

            notification.MouseLeftButtonDown += (sender, args) =>
            {
                if (onClick == null) return;
                onClick.Invoke();
                (sender as Notification)?.Close();
            };
            notification.NotificationClosed += (sender, args) => onClose?.Invoke();
            notification.NotificationClosed += this.OnNotificationClosed;

            if (!this.IsLoaded) return;

            var w = Window.GetWindow(this);
            if (w != null)
            {
                var x = PresentationSource.FromVisual(w);
                if (x == null) return;
            }

            lock (this.syncObject)
            {
                this.items.Add(notification);

                if (this.items.OfType<Notification>().Count(i => !i.IsClosing) > this.MaxItems) this.items.OfType<Notification>().First(i => !i.IsClosing).Close();
            }

            if (expirationTime == TimeSpan.MaxValue) return;
            await Task.Delay(expirationTime);

            notification.Close();
        }

        #endregion

        #region Private Methods

        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            var notification = sender as Notification;
            lock (this.syncObject)
            {
                this.items.Remove(notification);
            }
        }

        #endregion
    }
}