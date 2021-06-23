using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 小小截图OCR {
	[DefaultProperty(nameof(Child))]
	[ContentProperty(nameof(Child))]
	public class Tip : FrameworkElement, IAddChild {
		readonly static IntPtr HWND_TOPMOST = (IntPtr) (-1);

		const int WS_CLIPSIBLINGS = 0x04000000;
		const int WS_POPUP = unchecked((int) 0x80000000);

		const int WS_EX_TOOLWINDOW = 0x00000080;
		const int WS_EX_NOACTIVATE = 0x08000000;
		const int WS_EX_TOPMOST = 0x00000008;

		const int SWP_NOACTIVATE = 0x0010;
		const int SWP_NOMOVE = 0x0002;
		const int SWP_NOSIZE = 0x0001;
		const int SWP_NOOWNERZORDER = 0x0200;
		const int SWP_SHOWWINDOW = 0x0040;

		const int SW_HIDE = 0;

		static Tip() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Tip), new FrameworkPropertyMetadata(typeof(Tip)));
		}

		public Visual Child {
			get { return (Visual) GetValue(ChildProperty); }
			set { SetValue(ChildProperty, value); }
		}

		public static readonly DependencyProperty ChildProperty =
				DependencyProperty.Register("Child", typeof(FrameworkElement), typeof(Tip), new FrameworkPropertyMetadata(null));

		public UIElement PlacementTarget {
			get { return (UIElement) GetValue(PlacementTargetProperty); }
			set { SetValue(PlacementTargetProperty, value); }
		}

		public static readonly DependencyProperty PlacementTargetProperty =
			DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(Tip), new PropertyMetadata(null));

		public bool IsOpen {
			get { return (bool) GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}

		public static readonly DependencyProperty IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof(bool), typeof(Tip), new PropertyMetadata(false));

		public double TipX {
			get { return (double) GetValue(TipXProperty); }
			set { SetValue(TipXProperty, value); }
		}

		public static readonly DependencyProperty TipXProperty =
			DependencyProperty.Register("TipX", typeof(double), typeof(Tip), new PropertyMetadata(0d));


		public double TipY {
			get { return (double) GetValue(TipYProperty); }
			set { SetValue(TipYProperty, value); }
		}

		public static readonly DependencyProperty TipYProperty =
			DependencyProperty.Register("TipY", typeof(double), typeof(Tip), new PropertyMetadata(0d));

		public double TipWidth {
			get { return (double) GetValue(TipWidthProperty); }
			protected set { SetValue(TipWidthPropertKey, value); }
		}

		private static readonly DependencyPropertyKey TipWidthPropertKey =
			DependencyProperty.RegisterAttachedReadOnly("TipWidth", typeof(double), typeof(Tip), new PropertyMetadata(0d));

		public static readonly DependencyProperty TipWidthProperty = TipWidthPropertKey.DependencyProperty;

		public double TipHeight {
			get { return (double) GetValue(TipHeightProperty); }
			protected set { SetValue(TipHeightPropertKey, value); }
		}

		private static readonly DependencyPropertyKey TipHeightPropertKey =
			DependencyProperty.RegisterAttachedReadOnly("TipHeight", typeof(double), typeof(Tip), new PropertyMetadata(0d));

		public static readonly DependencyProperty TipHeightProperty = TipHeightPropertKey.DependencyProperty;


		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);

			if (e.Property == ChildProperty) {
				hwndSource.RootVisual = (Visual) e.NewValue;
				var rect = GetWindowRect();
				TipWidth = rect.Width;
				TipHeight = rect.Height;
			} else if (e.Property == IsOpenProperty) {
				if ((bool) e.NewValue)
					ShowWindow();
				else
					HideWindow();
			} else if (e.Property == TipXProperty || e.Property == TipYProperty) {
				updatePosition = IsOpen;
			} else if (e.Property == PlacementTargetProperty) {
				RemovePlacementTargetEvents(e.OldValue as UIElement);
				AddPlacementTargetEvents(e.NewValue as UIElement);
			} else if (e.Property == VisibilityProperty) {
				if ((Visibility) e.NewValue == Visibility.Visible && IsOpen) {
					ShowWindow();
				} else {
					HideWindow();
				}
			}

			if (updatePosition) SetWindowFollowMouse(TipX, TipY);
			updatePosition = false;
		}


		private HwndSource hwndSource;
		private bool updatePosition;

		public Tip() {
			Loaded += delegate {
				AddPlacementTargetEvents(PlacementTarget);
			};
			Unloaded += delegate {
				hwndSource?.Dispose();
				RemovePlacementTargetEvents(PlacementTarget);
			};
			BuildWindow();
		}

		private void AddPlacementTargetEvents(UIElement target) {
			if (target == null) return;
			target.PreviewMouseMove += TargetPreviewMouseMove;
			target.MouseEnter += TargetMouseEnter;
			target.MouseLeave += TargetMouseLeave;
		}

		private void RemovePlacementTargetEvents(UIElement target) {
			if (target == null) return;
			target.PreviewMouseMove -= TargetPreviewMouseMove;
			target.MouseEnter -= TargetMouseEnter;
			target.MouseLeave -= TargetMouseLeave;
		}

		private void TargetMouseLeave(object sender, MouseEventArgs e) {
			IsOpen = false;
		}

		private void TargetMouseEnter(object sender, MouseEventArgs e) {
			IsOpen = true;
		}

		private void TargetPreviewMouseMove(object sender, MouseEventArgs e) {
			if (!IsOpen) return;
			var point = Tool.ClientToScreen(e);
			TipX = point.X;
			TipY = point.Y;
		}

		void IAddChild.AddChild(object value) {
			Child = (Visual) value;
		}

		void IAddChild.AddText(string text) {
			Child = new TextBlock { Text = text };
		}

		protected override IEnumerator LogicalChildren {
			get {
				if (Child != null) yield return Child;
			}
		}

		protected override Size MeasureOverride(Size availableSize) {
			return new Size();
		}

		private void BuildWindow() {
			var param = new HwndSourceParameters(string.Empty) {
				WindowClassStyle = 0,
				WindowStyle = WS_CLIPSIBLINGS | WS_POPUP,
				ExtendedWindowStyle = WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST,
				UsesPerPixelTransparency = true
			};
			param.SetPosition(0, 0);
			hwndSource?.Dispose();
			hwndSource = new HwndSource(param);
		}

		private void ShowWindow() {
			//if (hwndSource == null || hwndSource.IsDisposed) return;
			//if (Visibility != Visibility.Visible) return;

			//Tool.SetWindowPos(hwndSource.Handle, HWND_TOPMOST, 0, 0, 0, 0,
			//	SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE | SWP_NOOWNERZORDER | SWP_SHOWWINDOW);
			SetWindowFollowMouse(TipX, TipY);
		}

		private void SetWindowFollowMouse(double x, double y) {
			if (hwndSource == null || hwndSource.IsDisposed) return;
			if (Visibility != Visibility.Visible) return;

			var cursorInfo = Tool.GetCursorInfo();
			var winRect = GetWindowRect();
			Thickness offset = new Thickness {
				Left = 5,
				Right = 5,
				Top = cursorInfo.HotY + 5,
				Bottom = cursorInfo.Height - cursorInfo.HotY + 5
			};
			var right = x + offset.Right + winRect.Width;
			var bottom = y + offset.Bottom + winRect.Height;
			if (right >= SystemParameters.PrimaryScreenWidth) {
				x -= winRect.Width + offset.Left;
			} else {
				x += offset.Right;
			}
			if (bottom >= SystemParameters.PrimaryScreenHeight) {
				y -= winRect.Height + offset.Top;
			} else {
				y += offset.Bottom;
			}

			Tool.SetWindowPos(hwndSource.Handle, HWND_TOPMOST, (int) x, (int) y, 0, 0,
				SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOOWNERZORDER | SWP_SHOWWINDOW);
		}

		private void HideWindow() {
			if (hwndSource == null || hwndSource.IsDisposed) return;

			Tool.ShowWindow(hwndSource.Handle, SW_HIDE);
		}

		private Rect GetWindowRect() {
			if (hwndSource == null || hwndSource.IsDisposed) return new Rect();
			return Tool.GetWindowRect(hwndSource.Handle);
		}
	}
}
