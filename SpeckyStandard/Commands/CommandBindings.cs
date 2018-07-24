using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpeckyStandard.Commands
{
    public sealed class CommandBindings
    {
        public Action<object> ExecuteAction { get; set; }
        public List<Predicate<object>> CanExecutePredicates { get; set; }
        public INotifyPropertyChanged BindingModel { get; set; }
        public List<string> BindingPropertNames { get; set; }
    }
}
