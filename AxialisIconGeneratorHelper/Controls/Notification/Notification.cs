#region Usings

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using AxialisIconGeneratorHelper.Utils;

#endregion

namespace AxialisIconGeneratorHelper.Controls.Notification
{
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    internal sealed class Notification : ContentControl
    {
        #region Public Fields

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly DependencyProperty CloseOnClickProperty =
            DependencyProperty.RegisterAttached("CloseOnClick", typeof(bool), typeof(Notification), new FrameworkPropertyMetadata(false, CloseOnClickChanged));

        #endregion

        #region Private Fields

        private TimeSpan closingAnimationTime = TimeSpan.Zero;

        #endregion

        #region Public Properties

        public bool IsClosing { get; set; }

        #endregion

        #region Static Constructors

        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Notification),
                                                     new FrameworkPropertyMetadata(typeof(Notification)));
        }

        #endregion

        #region Public Methods

        public async void Close()
        {
            if (this.IsClosing) return;

            this.IsClosing = true;

            this.RaiseEvent(new RoutedEventArgs(NotificationCloseInvokedEvent));
            await Task.Delay(this.closingAnimationTime);
            this.RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }

        public static bool GetCloseOnClick(DependencyObject obj) => (bool) obj.GetValue(CloseOnClickProperty);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_CloseButton") is Button closeButton)
                closeButton.Click += this.OnCloseButtonOnClick;

            var storyboards = this.Template.Triggers.OfType<EventTrigger>().FirstOrDefault(t => t.RoutedEvent == NotificationCloseInvokedEvent)?.Actions.OfType<BeginStoryboard>().Select(a => a.Storyboard);
            this.closingAnimationTime = new TimeSpan(storyboards?.Max(s => Math.Min((s.Duration.HasTimeSpan ? s.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero) : TimeSpan.MaxValue).Ticks, s.Children.Select(ch => ch.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero)).Max().Ticks)) ?? 0);
        }

        public static void SetCloseOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseOnClickProperty, value);
        }

        #endregion

        #region Private Methods

        private static void CloseOnClickChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is Button button)) return;

            var value = (bool) dependencyPropertyChangedEventArgs.NewValue;

            if (value)
                button.Click += (sender, args) =>
                {
                    var notification = UIHelper.GetParent<Notification>(button);
                    notification?.Close();
                };
        }

        private void OnCloseButtonOnClick(object sender, RoutedEventArgs args)
        {
            if (!(sender is Button button)) return;

            button.Click -= this.OnCloseButtonOnClick;
            this.Close();
        }

        #endregion

        public event RoutedEventHandler NotificationCloseInvoked
        {
            add => this.AddHandler(NotificationCloseInvokedEvent, value);
            remove => this.RemoveHandler(NotificationCloseInvokedEvent, value);
        }

        public event RoutedEventHandler NotificationClosed
        {
            add => this.AddHandler(NotificationClosedEvent, value);
            remove => this.RemoveHandler(NotificationClosedEvent, value);
        }
    }
}