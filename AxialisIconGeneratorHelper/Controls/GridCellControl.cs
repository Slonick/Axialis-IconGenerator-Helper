#region Usings

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace AxialisIconGeneratorHelper.Controls
{
    public class GridCellControl : Decorator
    {
        #region Public Fields

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(SolidColorBrush), typeof(GridCellControl), new FrameworkPropertyMetadata(default(SolidColorBrush), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register(nameof(GridLineBrush), typeof(SolidColorBrush), typeof(GridCellControl), new FrameworkPropertyMetadata(default(SolidColorBrush), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty GridLineThicknessProperty =
            DependencyProperty.Register(nameof(GridLineThickness), typeof(double), typeof(GridCellControl), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnGridLineThicknessChanged));

        public static readonly DependencyProperty GridSizeProperty =
            DependencyProperty.Register(nameof(GridSize), typeof(uint), typeof(GridCellControl), new FrameworkPropertyMetadata(24u, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnGridSizeChanged));

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(GridCellControl), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty AutoPaddingProperty =
            DependencyProperty.Register(nameof(AutoPadding), typeof(bool), typeof(GridCellControl), new PropertyMetadata(true, OnAutoPaddingChanged));

        #endregion

        #region Public Properties

        public bool AutoPadding
        {
            get => (bool) this.GetValue(AutoPaddingProperty);
            set => this.SetValue(AutoPaddingProperty, value);
        }

        public SolidColorBrush Background
        {
            get => (SolidColorBrush) this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        public SolidColorBrush GridLineBrush
        {
            get => (SolidColorBrush) this.GetValue(GridLineBrushProperty);
            set => this.SetValue(GridLineBrushProperty, value);
        }

        public double GridLineThickness
        {
            get => (double) this.GetValue(GridLineThicknessProperty);
            set => this.SetValue(GridLineThicknessProperty, value);
        }

        public uint GridSize
        {
            get => (uint) this.GetValue(GridSizeProperty);
            set => this.SetValue(GridSizeProperty, value);
        }

        public Thickness Padding
        {
            get => (Thickness) this.GetValue(PaddingProperty);
            set => this.SetValue(PaddingProperty, value);
        }

        #endregion

        #region Protected Methods

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.Child == null) return arrangeSize;

            var rect1 = new Rect(arrangeSize);
            var rect2 = HelperDeflateRect(rect1, new Thickness(0));
            var finalRect = HelperDeflateRect(rect2, this.Padding);
            this.Child.Arrange(finalRect);

            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var child = this.Child;
            var size1 = new Size();

            var size2 = HelperCollapseThickness(this.Padding);
            if (child != null)
            {
                var size3 = new Size(size2.Width, size2.Height);
                var availableSize = new Size(Math.Max(0.0, constraint.Width - size3.Width), Math.Max(0.0, constraint.Height - size3.Height));
                child.Measure(availableSize);
                var desiredSize = child.DesiredSize;
                size1.Width = desiredSize.Width + size3.Width;
                size1.Height = desiredSize.Height + size3.Height;
            }
            else
            {
                size1 = size2;
            }

            return size1;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Math.Abs(this.ActualHeight) <= 0 || Math.Abs(this.ActualWidth) <= 0) return;

            if (this.AutoPadding) this.InvalidatePadding();
            dc.DrawRectangle(this.Background, null, new Rect(this.RenderSize));
            this.RenderLines(dc);
        }

        #endregion

        #region Private Methods

        private static Size HelperCollapseThickness(Thickness th) => new Size(th.Left + th.Right, th.Top + th.Bottom);

        private static Rect HelperDeflateRect(Rect rt, Thickness thick) => new Rect(rt.Left + thick.Left, rt.Top + thick.Top, Math.Max(0.0, rt.Width - thick.Left - thick.Right), Math.Max(0.0, rt.Height - thick.Top - thick.Bottom));

        private void InvalidatePadding()
        {
            var cellWidth = this.ActualWidth / this.GridSize;
            var cellHeight = this.ActualHeight / this.GridSize;
            this.SetCurrentValue(PaddingProperty, new Thickness(cellWidth, cellHeight, cellWidth - this.GridLineThickness, cellHeight - this.GridLineThickness));
        }

        private static void OnAutoPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (GridCellControl) d;
            if (self.AutoPadding) self.InvalidatePadding();
        }

        private static void OnGridLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (GridCellControl) d;
            if (self.AutoPadding) self.InvalidatePadding();
        }

        private static void OnGridSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (GridCellControl) d;
            if (self.AutoPadding) self.InvalidatePadding();
        }

        private void RenderLines(DrawingContext dc)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null) return;

            var m = source.CompositionTarget.TransformToDevice;
            var scaleFactor = 1 / m.M11;

            var thickness = this.GridLineThickness * scaleFactor;
            var halfThickness = thickness / 2;

            var pen = new Pen(this.GridLineBrush, thickness);
            pen.Freeze();

            //draw horizontal lines
            var cellWidth = (this.ActualWidth - halfThickness) / this.GridSize;
            for (var x = 0d; x <= this.ActualWidth; x += cellWidth)
            {
                var snapX = new[] {x - halfThickness, x + halfThickness};
                var snapY = new[] {0d, this.ActualHeight};
                dc.PushGuidelineSet(new GuidelineSet(snapX, snapY));
                dc.DrawLine(pen, new Point(x, 0), new Point(x, this.ActualHeight));
                dc.Pop();
            }

            //draw vertical lines
            var cellHeight = (this.ActualHeight - halfThickness) / this.GridSize;
            for (var y = 0d; y <= this.ActualHeight; y += cellHeight)
            {
                var snapX = new[] {0, this.ActualWidth};
                var snapY = new[] {y - halfThickness, y + halfThickness};
                dc.PushGuidelineSet(new GuidelineSet(snapX, snapY));
                dc.DrawLine(pen, new Point(0, y), new Point(this.ActualWidth, y));
                dc.Pop();
            }
        }

        #endregion
    }
}