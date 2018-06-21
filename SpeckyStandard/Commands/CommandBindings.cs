using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpeckyStandard.Commands
{
    public sealed class CommandBindings : IDisposable
    {
        public Action<object> ExecuteAction { get; set; }
        public List<Predicate<object>> CanExecutePredicates { get; set; }
        public INotifyPropertyChanged BindingModel { get; set; }
        public List<string> BindingPropertNames { get; set; }

        public void Dispose()
        {
            ExecuteAction = null;
            CanExecutePredicates = null;
            BindingModel = null;
            BindingPropertNames = null;
            GC.SuppressFinalize(this);
        }

        ~CommandBindings() => Dispose();
    }
}
