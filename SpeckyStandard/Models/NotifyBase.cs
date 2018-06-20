using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeckyStandard.Models
{
    /// <summary>
    /// A base model that implements INotifyPropertyChanged and eases notifications.
    /// </summary>
    public abstract class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes PropertyChanged passing the supplied name.
        /// Uses the calling members name if no name is supplied.
        /// </summary>
        /// <param name="callerName">The calling member name.  In typical cases this is left blank or the property name is given.</param>
        public void Notify([CallerMemberName] string callerName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
    }
}
