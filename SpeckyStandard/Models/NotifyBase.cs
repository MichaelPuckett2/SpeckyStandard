using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeckyStandard.Models
{
    public class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify([CallerMemberName] string callerName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
    }
}
