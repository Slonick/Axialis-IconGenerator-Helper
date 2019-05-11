#region Usings

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AxialisIconGeneratorHelper.ViewModels.Base;

#endregion

namespace AxialisIconGeneratorHelper.Controls
{
    public class VideoPlayer : Control
    {
        #region Public Fields

        public static readonly DependencyProperty HideControlsTimeProperty =
            DependencyProperty.Register(nameof(HideControlsTime), typeof(TimeSpan), typeof(VideoPlayer), new PropertyMetadata(TimeSpan.FromSeconds(5), OnHideControlsTimeChanged));

        public static readonly DependencyProperty NaturalDurationProperty =
            DependencyProperty.Register(nameof(NaturalDuration), typeof(TimeSpan), typeof(VideoPlayer), new PropertyMetadata(default(TimeSpan)));

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(TimeSpan), typeof(VideoPlayer), new PropertyMetadata(default(TimeSpan), OnPositionChanged));

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(VideoPlayer), new PropertyMetadata(default(bool), OnIsPlayingChanged));


        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(Uri), typeof(VideoPlayer), new PropertyMetadata(default(Uri)));

        #endregion

        #region Private Fields

        private DispatcherTimer hideControlsTimer;
        private FrameworkElement partControls;
        private MediaElement partMediaElement;
        private DispatcherTimer timerVideoTime;

        #endregion

        #region Public Properties

        public TimeSpan HideControlsTime
        {
            get => (TimeSpan) this.GetValue(HideControlsTimeProperty);
            set => this.SetValue(HideControlsTimeProperty, value);
        }

        public bool IsPlaying
        {
            get => (bool) this.GetValue(IsPlayingProperty);
            set => this.SetValue(IsPlayingProperty, value);
        }

        public TimeSpan NaturalDuration
        {
            get => (TimeSpan) this.GetValue(NaturalDurationProperty);
            private set => this.SetValue(NaturalDurationProperty, value);
        }

        public ICommand PlayPauseCommand { get; private set; }

        public TimeSpan Position
        {
            get => (TimeSpan) this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }

        public Uri Source
        {
            get => (Uri) this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        #endregion

        #region Static Constructors

        static VideoPlayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoPlayer), new FrameworkPropertyMetadata(typeof(VideoPlayer)));
        }

        #endregion

        #region Public Methods

        public override void BeginInit()
        {
            base.BeginInit();
            this.PlayPauseCommand = new RelayCommand(this.PlayPauseExecute);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.partControls = this.GetTemplateChild("PART_Controls") as Grid;
            this.partMediaElement = this.GetTemplateChild("PART_MediaElement") as MediaElement;

            if (this.partMediaElement != null)
            {
                this.partMediaElement.MediaOpened += this.OnMediaOpened;
                this.partMediaElement.MediaEnded += this.OnMediaEnded;
                this.IsPlaying = true;
            }

            this.hideControlsTimer = new DispatcherTimer {Interval = this.HideControlsTime};
            this.hideControlsTimer.Tick += this.OnHideControlsTimerTick;
            this.hideControlsTimer.Start();
        }

        #endregion

        #region Protected Methods

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.SetControlsState(false);
            this.hideControlsTimer.Stop();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.SetControlsState(true);
            this.hideControlsTimer.Stop();
            this.hideControlsTimer.Start();
        }

        #endregion

        #region Private Methods

        private static void OnHideControlsTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VideoPlayer) d;
            self.hideControlsTimer.Stop();
            self.hideControlsTimer.Interval = self.HideControlsTime;
        }


        private void OnHideControlsTimerTick(object sender, EventArgs e)
        {
            this.hideControlsTimer.Stop();
            this.SetControlsState(false);
        }

        private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VideoPlayer) d;
            if (self.IsPlaying)
                self.partMediaElement.Play();
            else
                self.partMediaElement.Pause();
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            this.timerVideoTime.Stop();
            this.IsPlaying = false;
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            this.NaturalDuration = this.partMediaElement.NaturalDuration.TimeSpan;
            this.timerVideoTime = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1000 / 60.0)};
            this.timerVideoTime.Tick += this.OnTimerVideoTimeTick;
            this.timerVideoTime.Start();
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VideoPlayer) d;
            if (self.partMediaElement.Position.Seconds != self.Position.Seconds)
                self.partMediaElement.Position = self.Position;
        }

        private void OnTimerVideoTimeTick(object sender, EventArgs e)
        {
            if (this.IsPlaying && this.NaturalDuration.TotalSeconds > 0)
                this.Position = this.partMediaElement.Position;
        }

        private void PlayPauseExecute()
        {
            this.IsPlaying = !this.IsPlaying;
        }

        private void SetControlsState(bool isVisible)
        {
            var animation = new DoubleAnimation
            {
                To = isVisible ? 1d : 0d,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };

            this.partControls.BeginAnimation(OpacityProperty, animation);
        }

        #endregion
    }
}