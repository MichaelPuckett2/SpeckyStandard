using SpeckyStandard.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SpeckyStandard.Commands
{
    public class AutoRelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Action<object> ExecuteAction { get; }
        public List<Predicate<object>> CanExecutePredicates { get; }
        public INotifyPropertyChanged BindingModel { get; }
        public List<string> BindingPropertNames { get; }

        public AutoRelayCommand(CommandBindings commandBindings)
        {
            ExecuteAction = commandBindings.ExecuteAction;
            CanExecutePredicates = commandBindings.CanExecutePredicates.ToList();
            BindingModel = commandBindings.BindingModel;
            BindingPropertNames = commandBindings.BindingPropertNames.ToList();
            BindingModel.PropertyChanged += (s, e) => BindingPropertNames.Contains(e.PropertyName).PulseOnTrue(Update);
        }

        public bool CanExecute(object parameter) => CanExecutePredicates.All(canExecute => canExecute(parameter));
        public void Execute(object parameter) => Execute(parameter);
        public void Update() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
