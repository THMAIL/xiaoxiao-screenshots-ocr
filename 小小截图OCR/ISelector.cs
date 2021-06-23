using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace 小小截图OCR {
	public interface ISelector {
		Action<object> SelectorBeginMove { get; }
		Action<object> SelectorMove { get; }
		Action<object> SelectorEndMove { get; }
		Action<object> SelectorBeginResize { get; }
		Action<object> SelectorResize { get; }
		Action<object> SelectorEndResize { get; }

		double SelectX { get; set; }
		double SelectY { get; set; }
		double SelectWidth { get; set; }
		double SelectHeight { get; set; }
		FrameworkElement Selector { get; }
		SelectState SelectState { get; }
		bool SelectorCanMove { get; set; }
		bool SelectorCanResize { get; set; }
		bool SelectorEnabled { get; set; }

		void SetSelectState(SelectState state);
	}
}
