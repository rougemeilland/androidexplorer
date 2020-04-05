using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

namespace AndroidExplorer
{
    internal class TreeNode
        : DataModel
    {
        protected class DelegateCommand
            : ICommand
        {
            private Action _execute;
            private Func<bool> _can_execute;

            public DelegateCommand(Action execute, Func<bool> can_execute)
            {
                _execute = execute;
                _can_execute = can_execute;
            }

            public bool CanExecute(object parameter)
            {
                return _can_execute();
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                _execute();
            }
        }


        private string _display_name;
        private bool _is_expanded;

        protected TreeNode(string display_name)
        {
            _display_name = display_name;
            _is_expanded = false;
            Children = new ObservableCollection<TreeNode>();
            DetailItems = new ObservableCollection<DetailItem>();
            IsDisconnected = false;
            RefreshNodeCommand = new DelegateCommand(
                async () =>
                {
                    await Update(true);
                },
                () => true);
            SetPropertiesCommand = new DelegateCommand(
                () => { },
                () => false);
        }

        public virtual bool IsActiveRefreshNode => true;

        public ICommand RefreshNodeCommand { get; protected set; }

        public virtual bool IsActiveSetProperties => false;

        public ICommand SetPropertiesCommand { get; protected set; }

        public string DisplayName
        {
            get => _display_name;

            protected set
            {
                if (value != _display_name)
                {
                    _display_name = value;
                    RaisePropertyChanged("DisplayName");
                }
            }
        }

        public bool IsExpanded
        {
            get => _is_expanded;

            set
            {
                if (value != _is_expanded)
                {
                    _is_expanded = value;
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }


        public IList<TreeNode> Children { get; private set; }

        public IList<DetailItem> DetailItems { get; private set; }

        async public virtual Task Update(bool forcely)
        {
            if (IsDisconnected)
                return;
            await Task.Run(() => { });
        }

        public virtual void Disconnect()
        {
            foreach (var child in Children)
                child.Disconnect();
            Children.Clear();
            IsDisconnected = true;
        }

        public bool IsDisconnected { get; private set; }
    }
}

