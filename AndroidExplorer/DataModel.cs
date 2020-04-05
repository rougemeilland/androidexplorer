using System.ComponentModel;

namespace AndroidExplorer
{
    internal class DataModel
        : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

