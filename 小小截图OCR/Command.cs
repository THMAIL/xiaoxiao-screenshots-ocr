using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace 小小截图OCR {
	public class Command<T> : ICommand {
		public event EventHandler CanExecuteChanged;

		private Action<T> executeAction;
		private Func<T, bool> canExecuteAction;

		public Command(Action<T> executeAction, Func<T, bool> canExecuteAction = null) {
			this.executeAction = executeAction;
			this.canExecuteAction = canExecuteAction;
		}

		public bool CanExecute(T parameter) {
			return canExecuteAction?.Invoke(parameter) ?? true;
		}

		bool ICommand.CanExecute(object parameter) {
			if (parameter == null) return true;
			return parameter is T param && CanExecute(param);
		}

		public void Execute(T parameter) {
			executeAction?.Invoke(parameter);
		}

		void ICommand.Execute(object parameter) {
			Execute((T) parameter);
		}

		public void RaiseCanExecuteChanged() {
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
