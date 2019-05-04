#region Usings

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace AxialisIconGeneratorHelper.Controls
{
    public class GridCellControl : Border
    {
        #region Public Fields

        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register(nameof(CellSize), typeof(Size), typeof(GridCellControl), new PropertyMetadata(new Size(20, 20)));

        #endregion

        #region Public Properties

        public Size CellSize
        {
            get => (Size) this.GetValue(CellSizeProperty);
            set => this.SetValue(CellSizeProperty, value);
        }

        #endregion

        #region Protected Methods

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var pen = new Pen(this.BorderBrush, this.BorderThickness.Left);
            
            //draw horizontal lines
            for (var x = this.CellSize.Width; x < this.ActualWidth; x += this.CellSize.Width)
                dc.DrawLine(pen, new Point(x, 0), new Point(x, this.ActualHeight));

            //draw vertical lines
            for (var y = this.CellSize.Height; y < this.ActualHeight; y += this.CellSize.Height)
                dc.DrawLine(pen, new Point(0, y), new Point(this.ActualWidth, y));
        }

        #endregion
    }
}