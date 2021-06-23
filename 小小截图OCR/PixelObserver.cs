using System;
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

namespace 小小截图OCR {
	public class PixelObserver : Control {
		static PixelObserver() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PixelObserver), new FrameworkPropertyMetadata(typeof(PixelObserver)));
		}


		public double X {
			get { return (double) GetValue(XProperty); }
			set { SetValue(XProperty, value); }
		}

		public static readonly DependencyProperty XProperty =
			DependencyProperty.Register("X", typeof(double), typeof(PixelObserver), new PropertyMetadata(0d, NoticeRender));

		public double Y {
			get { return (double) GetValue(YProperty); }
			set { SetValue(YProperty, value); }
		}

		public static readonly DependencyProperty YProperty =
			DependencyProperty.Register("Y", typeof(double), typeof(PixelObserver), new PropertyMetadata(0d, NoticeRender));

		public double ScaleX {
			get { return (double) GetValue(ScaleXProperty); }
			set { SetValue(ScaleXProperty, value); }
		}

		public static readonly DependencyProperty ScaleXProperty =
			DependencyProperty.Register("ScaleX", typeof(double), typeof(PixelObserver), new PropertyMetadata(5d, NoticeRender));

		public double ScaleY {
			get { return (double) GetValue(ScaleYProperty); }
			set { SetValue(ScaleYProperty, value); }
		}

		public static readonly DependencyProperty ScaleYProperty =
			DependencyProperty.Register("ScaleY", typeof(double), typeof(PixelObserver), new PropertyMetadata(5d, NoticeRender));

		public BitmapSource BitmapSource {
			get { return (BitmapSource) GetValue(BitmapSourceProperty); }
			set { SetValue(BitmapSourceProperty, value); }
		}

		public static readonly DependencyProperty BitmapSourceProperty =
			DependencyProperty.Register("BitmapSource", typeof(BitmapSource), typeof(PixelObserver), new PropertyMetadata(null, NoticeRender));

		public Brush SelectorBrush {
			get { return (Brush) GetValue(SelectorBrushProperty); }
			set { SetValue(SelectorBrushProperty, value); }
		}

		public static readonly DependencyProperty SelectorBrushProperty =
			DependencyProperty.Register("SelectorBrush", typeof(Brush), typeof(PixelObserver), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(192, 255, 128, 128)), NoticeRender));

		public Color SelectColor {
			get { return (Color) GetValue(SelectColorProperty); }
			protected set { SetValue(SelectColorPropertyKey, value); }
		}

		private static readonly DependencyPropertyKey SelectColorPropertyKey =
			DependencyProperty.RegisterReadOnly("SelectColor", typeof(Color), typeof(PixelObserver), new PropertyMetadata());

		public static readonly DependencyProperty SelectColorProperty = SelectColorPropertyKey.DependencyProperty;

		private static void NoticeRender(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var @this = d as PixelObserver;
			@this.InvalidateVisual();

			if (e.Property == XProperty || e.Property == YProperty || e.Property == BitmapSourceProperty) {
				var bitmap = @this.BitmapSource;
				int x = (int) @this.X, y = (int) @this.Y;
				bool invaild = bitmap == null || x < 0 || y < 0 || x >= bitmap.PixelWidth || y >= bitmap.PixelHeight;

				if (!invaild) {
					var pixel = new int[1];
					bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
					var b = (byte) (pixel[0] & 0xff);
					var g = (byte) ((pixel[0] >> 8) & 0xff);
					var r = (byte) ((pixel[0] >> 16) & 0xff);
					@this.SetValue(SelectColorPropertyKey, Color.FromRgb(r, g, b));
				} else {
					@this.ClearValue(SelectColorPropertyKey);
				}
			}
		}

		unsafe private Color[,] GetPixels(BitmapSource bitmap, int centerX, int centerY, int rangeX, int rangeY) {
			int width = rangeX * 2 + 1, height = rangeY * 2 + 1;
			var rect = new Rect(centerX - rangeX, centerY - rangeY, width, height);
			var imageRect = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
			imageRect.Intersect(rect);
			var sourceRect = new Int32Rect((int) imageRect.X, (int) imageRect.Y, (int) imageRect.Width, (int) imageRect.Height);
			var pixels = new int[sourceRect.Width * sourceRect.Height];
			bitmap.CopyPixels(sourceRect, pixels, sourceRect.Width * 4, 0);

			int offsetX = rect.X < 0 ? -(int) rect.X : 0;
			int offsetY = rect.Y < 0 ? -(int) rect.Y : 0;
			var result = new Color[width, height];
			for (int y = 0; y < sourceRect.Height; y++) {
				for (int x = 0; x < sourceRect.Width; x++) {
					var p = pixels[y * sourceRect.Width + x];
					var b = (byte) (p & 0xff);
					var g = (byte) ((p >> 8) & 0xff);
					var r = (byte) ((p >> 16) & 0xff);
					result[x + offsetX, y + offsetY] = Color.FromRgb(r, g, b);
				}
			}
			return result;
		}

		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);

			var bitmap = BitmapSource;
			if (bitmap == null) return;

			var width = ActualWidth;
			var height = ActualHeight;
			var scaleX = ScaleX;
			var scaleY = ScaleY;
			int rangeX = (int) Math.Ceiling((width - scaleX) / 2 / scaleX);
			int rangeY = (int) Math.Ceiling((height - scaleY) / 2 / scaleY);

			var pixels = GetPixels(bitmap, (int) X, (int) Y, rangeX, rangeY);
			var offsetX = ((width - scaleX) / 2 / scaleX - rangeX) * scaleX;
			var offsetY = ((height - scaleY) / 2 / scaleY - rangeY) * scaleY;
			var pixelWidth = pixels.GetLength(0);
			var pixelHeight = pixels.GetLength(1);
			var selectorBrush = SelectorBrush.Clone();

			selectorBrush.Freeze();

			for (int y = 0; y < pixelHeight; y++) {
				for (int x = 0; x < pixelWidth; x++) {
					var rect = new Rect(offsetX + x * scaleX, offsetY + y * scaleY, scaleX, scaleY);
					var brush = new SolidColorBrush(pixels[x, y]);
					brush.Freeze();
					dc.DrawRectangle(brush, null, rect);

					if ((x == rangeX && y != rangeY) || (x != rangeX && y == rangeY)) {
						dc.DrawRectangle(selectorBrush, null, rect);
					}
				}
			}
		}
	}
}
