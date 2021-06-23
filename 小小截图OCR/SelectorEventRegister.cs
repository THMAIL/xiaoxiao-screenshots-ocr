using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace 小小截图OCR {
	public static class SelectorEventRegister<T> where T : FrameworkElement, ISelector {
		public static Events GetSelectorMove(T parent) {
			var result = new Events();
			var oldSelectorPosition = new Point();

			result.Enabled += (ref bool enabled)
				=> enabled = enabled && (parent.SelectState == SelectState.Selected && parent.SelectorCanMove);

			result.BeginMove += (sender, e) => {
				if (parent.SelectState != SelectState.Selected) return;
				oldSelectorPosition = new Point(parent.SelectX, parent.SelectY);
				parent.SetSelectState(SelectState.Move);
				parent.SelectorBeginMove?.Invoke(parent);
			};

			result.Move += (sender, e, start, curr) => {
				var newX = oldSelectorPosition.X + curr.X - start.X;
				var newY = oldSelectorPosition.Y + curr.Y - start.Y;
				newX = Math.Min(parent.ActualWidth - parent.Selector.ActualWidth, newX);
				newY = Math.Min(parent.ActualHeight - parent.Selector.ActualHeight, newY);
				newX = Math.Max(0, newX);
				newY = Math.Max(0, newY);
				parent.SelectX = newX;
				parent.SelectY = newY;
				parent.SelectorMove?.Invoke(parent);
			};

			result.EndMove += delegate {
				if (parent.SelectState == SelectState.Move) parent.SetSelectState(SelectState.Selected);
				parent.SelectorEndMove?.Invoke(parent);
			};

			return result;
		}

		private static Events GetResize<TPrep>(T parent, Func<TPrep> prep, Action<TPrep, Vector> resize) {
			var result = new Events();
			var prepValue = default(TPrep);

			result.Enabled += (ref bool enabled)
				=> enabled = enabled && parent.SelectState == SelectState.Selected && parent.SelectorCanResize;

			result.BeginMove += delegate {
				prepValue = prep();
				if (parent.SelectState == SelectState.Selected) parent.SetSelectState(SelectState.Resize);
				parent.SelectorBeginResize?.Invoke(parent);
			};

			result.Move += (sender, e, start, curr) => {
				resize(prepValue, curr - start);
				parent.SelectorResize?.Invoke(parent);
			};

			result.EndMove += delegate {
				if (parent.SelectState == SelectState.Resize) parent.SetSelectState(SelectState.Selected);
				parent.SelectorEndResize?.Invoke(parent);
			};

			return result;
		}

		public static void ResizeX(T parent, double reference, double newWidth) {
			if (newWidth >= 0) {
				newWidth = Math.Min(newWidth, parent.ActualWidth - reference);
				newWidth = Math.Min(newWidth, parent.ActualWidth - 1);
			} else {
				newWidth = Math.Min(reference, -newWidth);
				newWidth = Math.Min(newWidth, parent.ActualWidth - 1);
				reference -= newWidth;
			}
			parent.SelectX = reference;
			parent.SelectWidth = newWidth;
		}

		public static void ResizeY(T parent, double reference, double newHeight) {
			if (newHeight >= 0) {
				newHeight = Math.Min(newHeight, parent.ActualHeight - reference);
				newHeight = Math.Min(newHeight, parent.ActualHeight - 1);
			} else {
				newHeight = Math.Min(reference, -newHeight);
				newHeight = Math.Min(newHeight, parent.ActualHeight - 1);
				reference -= newHeight;
			}
			parent.SelectY = reference;
			parent.SelectHeight = newHeight;
		}

		private static (double Reference, double OldValue) GetPrepValueT(T parent)
			=> (parent.SelectY + parent.Selector.Height, -parent.Selector.Height);

		private static (double Reference, double OldValue) GetPrepValueB(T parent)
			=> (parent.SelectY, parent.Selector.Height);

		private static (double Reference, double OldValue) GetPrepValueL(T parent)
			=> (parent.SelectX + parent.Selector.Width, -parent.Selector.Width);

		private static (double Reference, double OldValue) GetPrepValueR(T parent)
			=> (parent.SelectX, parent.Selector.Width);

		public static Events GetResizeT(T parent)
			=> GetResize(parent,
				() => GetPrepValueT(parent),
				(p, diff) => ResizeY(parent, p.Reference, p.OldValue + diff.Y)
			);

		public static Events GetResizeB(T parent)
			=> GetResize(parent,
				() => GetPrepValueB(parent),
				(p, diff) => ResizeY(parent, p.Reference, p.OldValue + diff.Y)
			);

		public static Events GetResizeL(T parent)
			=> GetResize(parent,
				() => GetPrepValueL(parent),
				(p, diff) => ResizeX(parent, p.Reference, p.OldValue + diff.X)
			);

		public static Events GetResizeR(T parent)
			=> GetResize(parent,
				() => GetPrepValueR(parent),
				(p, diff) => ResizeX(parent, p.Reference, p.OldValue + diff.X)
			);

		private delegate (double Reference, double OldValue) GetPrepHandler(T parent);

		private static Events GetResizeBevel(T parent, GetPrepHandler xPrep, GetPrepHandler yPrep)
			=> GetResize(parent,
				() => {
					var (xReference, xOldValue) = xPrep(parent);
					var (yReference, yOldValue) = yPrep(parent);
					return (
						ReferenceX: xReference,
						OldX: xOldValue,
						ReferenceY: yReference,
						OldY: yOldValue
					);
				},
				(p, diff) => {
					ResizeX(parent, p.ReferenceX, p.OldX + diff.X);
					ResizeY(parent, p.ReferenceY, p.OldY + diff.Y);
				}
			);

		public static Events GetResizeLT(T parent)
			=> GetResizeBevel(parent, GetPrepValueL, GetPrepValueT);

		public static Events GetResizeLB(T parent)
			=> GetResizeBevel(parent, GetPrepValueL, GetPrepValueB);

		public static Events GetResizeRT(T parent)
			=> GetResizeBevel(parent, GetPrepValueR, GetPrepValueT);

		public static Events GetResizeRB(T parent)
			=> GetResizeBevel(parent, GetPrepValueR, GetPrepValueB);

		public static Events GetFirstResize(T parent) {
			bool canResize = false;
			var result = new Events();

			result.Enabled += (ref bool enabled)
				=> enabled = enabled && parent.SelectorEnabled && parent.SelectState == SelectState.Fixed;

			result.BeginMove += delegate {
				if (parent.SelectState != SelectState.Fixed) return;
				canResize = true;
				parent.SetSelectState(SelectState.Resize);
				parent.SelectorBeginResize?.Invoke(parent);
			};

			result.Move += (sender, e, start, curr) => {
				if (!canResize) return;
				ResizeX(parent, start.X, curr.X - start.X);
				ResizeY(parent, start.Y, curr.Y - start.Y);
				parent.SelectorResize?.Invoke(parent);
			};

			result.EndMove += delegate {
				if (!canResize) return;
				if (parent.SelectState == SelectState.Resize) parent.SetSelectState(SelectState.Selected);
				parent.SelectorEndResize?.Invoke(parent);
				canResize = false;
			};

			return result;
		}

		public delegate void EnabledHandler(ref bool enabled);
		public delegate void EventHandler(object sender, RoutedEventArgs e);
		public delegate void MouseButtonHandler(object sender, MouseButtonEventArgs e);
		public delegate void MouseMoveHandler(object sender, MouseButtonEventArgs e, Point start, Point curr);

		public sealed class Events {
			public EnabledHandler Enabled;
			public MouseButtonHandler BeginMove;
			public MouseMoveHandler Move;
			public MouseButtonHandler EndMove;
			public EventHandler Click;
			public EventHandler DoubleClick;
			public EventHandler RightClick;

			public Events AddClick(EventHandler click) {
				Click += click;
				return this;
			}

			public Events AddDoubleClick(EventHandler doubleClick) {
				DoubleClick += doubleClick;
				return this;
			}

			public Events AddRightClick(EventHandler rightClick) {
				RightClick += rightClick;
				return this;
			}
		}

		private static bool firstClick = false;
		private static System.Diagnostics.Stopwatch clickCounter = new System.Diagnostics.Stopwatch();

		public static void Register(T parent, UIElement handler, double moveThreshold, double clickRangeThreshold, params Events[] events) {
			bool isDown = false, isMove = false, enabled = true;
			Point start = new Point();
			const long ClickTimeSpanMilliseconds = 500;

			EnabledHandler hookHandler = null;
			MouseButtonHandler beginMove = null;
			MouseMoveHandler move = null;
			MouseButtonHandler endMove = null;
			EventHandler click = null;
			EventHandler doubleClick = null;
			EventHandler rightClick = null;

			foreach (var e in events) {
				hookHandler += e.Enabled;
				beginMove += e.BeginMove;
				move += e.Move;
				endMove += e.EndMove;
				click += e.Click;
				doubleClick += e.DoubleClick;
				rightClick += e.RightClick;
			}

			handler.MouseDown += (sender, e) => {
				if (e.LeftButton != MouseButtonState.Pressed) return;
				start = e.GetPosition(parent);
				enabled = true;
				hookHandler?.Invoke(ref enabled);
				if (enabled) handler.CaptureMouse(); // 先捕获
				isMove = moveThreshold <= 0;
				isDown = true;

				if (isMove && enabled) beginMove?.Invoke(sender, e);
				e.Handled = enabled;
			};

			handler.MouseMove += (sender, e) => {
				if (!isDown || !enabled) return;
				var curr = e.GetPosition(parent);
				if (isMove || Math.Abs(curr.X - start.X) >= moveThreshold || Math.Abs(curr.Y - start.Y) >= moveThreshold) {
					var newArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
					if (!isMove) beginMove?.Invoke(sender, newArgs);
					isMove = true;
					move?.Invoke(sender, newArgs, start, curr);
					e.Handled = newArgs.Handled;
				}
			};

			handler.MouseUp += (sender, e) => {
				if (!isDown || !enabled) return;
				var curr = e.GetPosition(parent);
				var isClick = Math.Abs(curr.X - start.X) < clickRangeThreshold && Math.Abs(curr.Y - start.Y) < clickRangeThreshold;
				bool isDoubleClick = firstClick && clickCounter.ElapsedMilliseconds < ClickTimeSpanMilliseconds && isClick;
				if (isDoubleClick) {
					doubleClick?.Invoke(sender, new RoutedEventArgs());
					firstClick = false;
				} else if (isClick) {
					firstClick = true;
				}

				start = e.GetPosition(parent);
				if (isClick) {
					clickCounter.Restart();
					click?.Invoke(sender, new RoutedEventArgs());
				}

				if (isMove) {
					isMove = false;
					endMove?.Invoke(sender, e);
				}
				isDown = false;
				handler.ReleaseMouseCapture(); // 后释放
			};

			var rightStart = new Point();

			handler.MouseRightButtonDown += (sender, e) => {
				rightStart = e.GetPosition(parent);
				e.Handled = true;
			};

			handler.MouseRightButtonUp += (sender, e) => {
				var curr = e.GetPosition(parent);
				var isClick = Math.Abs(curr.X - rightStart.X) < clickRangeThreshold && Math.Abs(curr.Y - rightStart.Y) < clickRangeThreshold;
				if (isClick) {
					rightClick?.Invoke(sender, new RoutedEventArgs());
				}
				e.Handled = true;
			};
		}
	}
}
