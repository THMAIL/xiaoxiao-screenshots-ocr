using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Diagnostics;

namespace 小小截图OCR {
	[TemplatePart(Name = BackCanvasName, Type = typeof(Canvas))]
	public class ImageEditor : Control, ISelector {
		const string BackCanvasName = "PART_BackCanvas";

		static ImageEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEditor), new FrameworkPropertyMetadata(typeof(ImageEditor)));
		}

		private Canvas selector;

		public FrameworkElement Selector => selector;


		public Brush SelectorBorderBrush {
			get { return (Brush) GetValue(SelectorBorderBrushProperty); }
			set { SetValue(SelectorBorderBrushProperty, value); }
		}

		public static readonly DependencyProperty SelectorBorderBrushProperty =
			DependencyProperty.Register("SelectorBorderBrush", typeof(Brush), typeof(ImageEditor), new PropertyMetadata(Brushes.Pink));



		public double SelectX {
			get { return (double) GetValue(SelectXProperty); }
			set { SetValue(SelectXProperty, value); }
		}

		public static readonly DependencyProperty SelectXProperty =
			DependencyProperty.Register("SelectX", typeof(double), typeof(ImageEditor), new PropertyMetadata(0d, NoticeRender));

		public double SelectY {
			get { return (double) GetValue(SelectYProperty); }
			set { SetValue(SelectYProperty, value); }
		}

		public static readonly DependencyProperty SelectYProperty =
			DependencyProperty.Register("SelectY", typeof(double), typeof(ImageEditor), new PropertyMetadata(0d, NoticeRender));

		public double SelectWidth {
			get { return (double) GetValue(SelectWidthProperty); }
			set { SetValue(SelectWidthProperty, value); }
		}

		public static readonly DependencyProperty SelectWidthProperty =
			DependencyProperty.Register("SelectWidth", typeof(double), typeof(ImageEditor), new PropertyMetadata(0d, NoticeRender));

		public double SelectHeight {
			get { return (double) GetValue(SelectHeightProperty); }
			set { SetValue(SelectHeightProperty, value); }
		}

		public static readonly DependencyProperty SelectHeightProperty =
			DependencyProperty.Register("SelectHeight", typeof(double), typeof(ImageEditor), new PropertyMetadata(0d, NoticeRender));

		public static void NoticeRender(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if (obj is ImageEditor @this) {
				@this.InvalidateVisual();

				if (e.Property == SelectWidthProperty) {
					if (@this.SelectX + @this.SelectWidth + 1 > @this.ActualWidth) {
						@this.SelectWidth = @this.ActualWidth - @this.SelectX - 1;
					}
				} else if (e.Property == SelectHeightProperty) {
					if (@this.SelectY + @this.SelectHeight + 1 > @this.ActualHeight) {
						@this.SelectHeight = @this.ActualHeight - @this.SelectY - 1;
					}
				}
			}
		}

		public static void SelectorDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if (obj is ImageEditor @this) {
				bool isFixed = @this.SelectState == SelectState.Fixed,
					selectorVisible = @this.SelectorEnabled,
					canResize = @this.SelectorCanResize,
					canMove = @this.SelectorCanMove;
				if (e.Property == SelectStateProperty) {
					isFixed = (SelectState) e.NewValue == SelectState.Fixed;
				} else if (e.Property == SelectorEnabledProperty) {
					selectorVisible = (bool) e.NewValue;
					@this.InvalidateVisual();
				} else if (e.Property == SelectorCanResizeProperty) {
					canResize = (bool) e.NewValue;
				} else if (e.Property == SelectorCanMoveProperty) {
					canMove = (bool) e.NewValue;
				}

				bool freedomVisible = !isFixed && canResize && selectorVisible;
				bool fixedVisible = !freedomVisible && selectorVisible;
				@this.FreedomSelectorVisible = freedomVisible;
				@this.FixedSelectorVisible = fixedVisible;
			}
		}


		public SelectState SelectState {
			get { return (SelectState) GetValue(SelectStateProperty); }
			protected set { SetValue(SelectStatePropertyKey, value); }
		}

		void ISelector.SetSelectState(SelectState state) => SelectState = state;

		public static readonly DependencyPropertyKey SelectStatePropertyKey =
			DependencyProperty.RegisterReadOnly("SelectState", typeof(SelectState), typeof(ImageEditor), new PropertyMetadata(SelectState.Fixed, SelectorDependencyPropertyChanged));

		public static readonly DependencyProperty SelectStateProperty = SelectStatePropertyKey.DependencyProperty;



		public bool SelectorEnabled {
			get { return (bool) GetValue(SelectorEnabledProperty); }
			set { SetValue(SelectorEnabledProperty, value); }
		}

		public static readonly DependencyProperty SelectorEnabledProperty =
			DependencyProperty.Register("SelectorVisible", typeof(bool), typeof(ImageEditor), new PropertyMetadata(true, SelectorDependencyPropertyChanged));


		public bool FreedomSelectorVisible {
			get { return (bool) GetValue(FreedomSelectorVisibleProperty); }
			protected set { SetValue(FreedomSelectorVisiblePropertyKey, value); }
		}

		public static readonly DependencyPropertyKey FreedomSelectorVisiblePropertyKey =
			DependencyProperty.RegisterReadOnly("FreedomSelectorVisible", typeof(bool), typeof(ImageEditor), new PropertyMetadata());

		public static readonly DependencyProperty FreedomSelectorVisibleProperty = FreedomSelectorVisiblePropertyKey.DependencyProperty;

		public bool FixedSelectorVisible {
			get { return (bool) GetValue(FixedSelectorVisibleProperty); }
			protected set { SetValue(FixedSelectorVisiblePropertyKey, value); }
		}

		public static readonly DependencyPropertyKey FixedSelectorVisiblePropertyKey =
			DependencyProperty.RegisterReadOnly("FixedSelectorVisible", typeof(bool), typeof(ImageEditor), new PropertyMetadata(true));

		public static readonly DependencyProperty FixedSelectorVisibleProperty = FixedSelectorVisiblePropertyKey.DependencyProperty;

		public bool SelectorCanMove {
			get { return (bool) GetValue(SelectorCanMoveProperty); }
			set { SetValue(SelectorCanMoveProperty, value); }
		}

		public static readonly DependencyProperty SelectorCanMoveProperty =
			DependencyProperty.Register("SelectorCanMove", typeof(bool), typeof(ImageEditor), new PropertyMetadata(true, SelectorDependencyPropertyChanged));

		public bool SelectorCanResize {
			get { return (bool) GetValue(SelectorCanResizeProperty); }
			set { SetValue(SelectorCanResizeProperty, value); }
		}

		public static readonly DependencyProperty SelectorCanResizeProperty =
			DependencyProperty.Register("SelectorCanResize", typeof(bool), typeof(ImageEditor), new PropertyMetadata(true, SelectorDependencyPropertyChanged));

		public BitmapSource BackgroundBitmap {
			get { return (BitmapSource) GetValue(BackgroundBitmapProperty); }
			set { SetValue(BackgroundBitmapProperty, value); }
		}

		public static readonly DependencyProperty BackgroundBitmapProperty =
			DependencyProperty.Register("BackgroundBitmap", typeof(BitmapSource), typeof(ImageEditor), new PropertyMetadata(null, NoticeRender));




		public static readonly RoutedEvent AcceptSelectEvent = EventManager.RegisterRoutedEvent("AcceptSelect", RoutingStrategy.Direct, typeof(AcceptSelectHandler), typeof(ImageEditor));

		public event AcceptSelectHandler AcceptSelect {
			add { AddHandler(AcceptSelectEvent, value); }
			remove { RemoveHandler(AcceptSelectEvent, value); }
		}

		public static readonly RoutedEvent CancelSelectEvent = EventManager.RegisterRoutedEvent("CancelSelect", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ImageEditor));

		public event RoutedEventHandler CancelSelect {
			add { AddHandler(CancelSelectEvent, value); }
			remove { RemoveHandler(CancelSelectEvent, value); }
		}

		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ImageEditor));

		public event RoutedEventHandler Click {
			add { AddHandler(ClickEvent, value); }
			remove { RemoveHandler(ClickEvent, value); }
		}

		public static readonly RoutedEvent CloseEvent = EventManager.RegisterRoutedEvent("Close", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ImageEditor));

		public event RoutedEventHandler Close {
			add { AddHandler(CloseEvent, value); }
			remove { RemoveHandler(CloseEvent, value); }
		}

		Action<object> ISelector.SelectorBeginMove => null;
		Action<object> ISelector.SelectorMove => null;
		Action<object> ISelector.SelectorEndMove => null;
		Action<object> ISelector.SelectorBeginResize => null;
		Action<object> ISelector.SelectorResize => null;
		Action<object> ISelector.SelectorEndResize { get; } = null;// @this => (@this as ImageEditor).SelectorCanResize = false;

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			if (GetTemplateChild(BackCanvasName) is Canvas backCanvas) {
				selector = backCanvas.FindName("SelectorBorder") as Canvas;

				SelectorEventRegister<ImageEditor>.EventHandler backClick = delegate {
					RaiseEvent(new RoutedEventArgs(ClickEvent, this));
				};

				SelectorEventRegister<ImageEditor>.EventHandler doubleClick = delegate {
					RaiseEvent(new AcceptSelectEventArgs(GetSelectBitmap(), AcceptSelectEvent, this));
				};

				SelectorEventRegister<ImageEditor>.EventHandler rightClick = delegate {
					if (SelectState != SelectState.Fixed) {
						RaiseEvent(new RoutedEventArgs(CancelSelectEvent, this));
					}
				};

				SelectorEventRegister<ImageEditor>.EventHandler closeClick = delegate {
					if (SelectState == SelectState.Fixed) {
						RaiseEvent(new RoutedEventArgs(CloseEvent, this));
					}
				};

				var initResize = SelectorEventRegister<ImageEditor>.GetFirstResize(this).AddClick(backClick).AddRightClick(rightClick);
				var selectorMove = SelectorEventRegister<ImageEditor>.GetSelectorMove(this).AddDoubleClick(doubleClick).AddRightClick(closeClick);
				var resizeT = SelectorEventRegister<ImageEditor>.GetResizeT(this).AddDoubleClick(doubleClick);
				var resizeB = SelectorEventRegister<ImageEditor>.GetResizeB(this).AddDoubleClick(doubleClick);
				var resizeL = SelectorEventRegister<ImageEditor>.GetResizeL(this).AddDoubleClick(doubleClick);
				var resizeR = SelectorEventRegister<ImageEditor>.GetResizeR(this).AddDoubleClick(doubleClick);
				var resizeLT = SelectorEventRegister<ImageEditor>.GetResizeLT(this).AddDoubleClick(doubleClick);
				var resizeLB = SelectorEventRegister<ImageEditor>.GetResizeLB(this).AddDoubleClick(doubleClick);
				var resizeRT = SelectorEventRegister<ImageEditor>.GetResizeRT(this).AddDoubleClick(doubleClick);
				var resizeRB = SelectorEventRegister<ImageEditor>.GetResizeRB(this).AddDoubleClick(doubleClick);

				const double MoveThreshold = 5, ClickRange = 5;
				SelectorEventRegister<ImageEditor>.Register(this, this, MoveThreshold, ClickRange, initResize);
				SelectorEventRegister<ImageEditor>.Register(this, selector, 0, ClickRange, selectorMove);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("lineT") as Line, 0, ClickRange, resizeT);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("lineB") as Line, 0, ClickRange, resizeB);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("lineL") as Line, 0, ClickRange, resizeL);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("lineR") as Line, 0, ClickRange, resizeR);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectT") as Rectangle, 0, ClickRange, resizeT);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectB") as Rectangle, 0, ClickRange, resizeB);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectL") as Rectangle, 0, ClickRange, resizeL);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectR") as Rectangle, 0, ClickRange, resizeR);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectLT") as Rectangle, 0, ClickRange, resizeLT);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectLB") as Rectangle, 0, ClickRange, resizeLB);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectRT") as Rectangle, 0, ClickRange, resizeRT);
				SelectorEventRegister<ImageEditor>.Register(this, selector.FindName("rectRB") as Rectangle, 0, ClickRange, resizeRB);
			}
		}

		public void Select(Rect selectRect) {
			SelectX = selectRect.X;
			SelectY = selectRect.Y;
			SelectWidth = selectRect.Width;
			SelectHeight = selectRect.Height;
			Select();
		}

		public void Select() => SelectState = SelectState.Selected;

		public void Reset() => SelectState = SelectState.Fixed;

		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);

			var backImage = BackgroundBitmap;
			var backRect = new Rect(0, 0, ActualWidth, ActualHeight);
			if (backImage != null) {
				var drawRect = new Rect(0, 0, backImage.Width, backImage.Height);
				dc.DrawImage(backImage, drawRect);
			}

			if (SelectorEnabled) {
				var backRectGeometry = new RectangleGeometry(backRect);
				var imageRect = new Rect(SelectX, SelectY, SelectWidth + 1, SelectHeight + 1);
				var imageRectGeometry = new RectangleGeometry(imageRect);

				dc.PushClip(Geometry.Combine(backRectGeometry, imageRectGeometry, GeometryCombineMode.Exclude, null));
				dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), null, backRect);
				dc.Pop();
			}
		}

		public BitmapSource GetSelectBitmap() {
			var x = (int) SelectX;
			var y = (int) SelectY;
			var width = (int) SelectWidth + 1;
			var height = (int) SelectHeight + 1;
			var bitmap = BackgroundBitmap;
			var result = new CroppedBitmap(bitmap, new Int32Rect(x, y, width, height));
			if (result.CanFreeze) result.Freeze();
			return result;
		}
	}
}
