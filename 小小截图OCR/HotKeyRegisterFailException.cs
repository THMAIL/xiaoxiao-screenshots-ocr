using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 小小截图OCR {
	public class HotKeyRegisterFailException : Exception {
		public HotKeyRegisterFailException(string message, Exception e) : base(message, e) {}

		public HotKeyRegisterFailException(Exception e) : base("注册热键失败。", e) { }
	}
}
