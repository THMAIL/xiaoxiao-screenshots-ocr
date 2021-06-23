using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace 小小截图OCR {
	/// <summary>
	/// TestWindow.xaml 的交互逻辑
	/// </summary>
	public partial class TestWindow : Window {
		public TestWindow() {
			InitializeComponent();
		}

		private void Grid_PreviewMouseMove(object sender, MouseEventArgs e) {
			//var point = Tool.ClientToScreen(e);
			//point.Offset(16, 16);
			//tip.TipX = point.X;
			//tip.TipY = point.Y;
			//System.Diagnostics.Debug.Print($"{point}");

			//popup.ClearValue(Popup.IsOpenProperty);
			//popup.IsOpen = true;

			//var point = e.GetPosition(grid);
			//popup.HorizontalOffset = point.X + 16;
			//popup.VerticalOffset = point.Y + 16;
		}

		private void tip_PreviewMouseMove(object sender, MouseEventArgs e) {

		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {

		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			//System.Diagnostics.Debug.Print($"click");
		}

		private void Grid_MouseEnter(object sender, MouseEventArgs e) {
			//tip.IsOpen = true;
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e) {
			//tip.IsOpen = false;
		}
	}
}
