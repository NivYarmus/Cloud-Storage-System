using NivDrive.MVVM.ViewModels;
using System;

namespace NivDrive.Stores
{
    internal class NavigationStore
    {
        private ViewModelBase _ViewModel;
        public ViewModelBase ViewModel
        {
            get { return _ViewModel; }
            set
            {
                _ViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public event Action? CurrentViewModelChanged;

        public NavigationStore(ViewModelBase startViewModel)
        {
            ViewModel = startViewModel;
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
