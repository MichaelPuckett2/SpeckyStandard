using System;
using System.Windows.Input;

namespace SpeckyStandard.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Predicate<object> CanExecutePredicate;
        private readonly Action<object> ExecuteAction;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            ExecuteAction = execute;
            CanExecutePredicate = canExecute ?? new Predicate<object>((obj) => true);
        }

        public bool CanExecute(object parameter) => CanExecutePredicate.Invoke(parameter);
        public void Execute(object parameter) => ExecuteAction.Invoke(parameter);
        public void Update() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
