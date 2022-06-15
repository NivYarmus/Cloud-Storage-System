using NivDrive.MVVM.ViewModels;
using NivDrive.Stores;
using System;

namespace NivDrive.Commands.NavigationCommands
{
    internal class NavigationCommand : CommandBase
    {
        protected readonly NavigationStore _navigationStore;
        private readonly Func<object, ViewModelBase> _createViewModel;

        public NavigationCommand(NavigationStore navigationStore, Func<object, ViewModelBase> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public override void Execute(object? parameter)
        {
            _navigationStore.ViewModel = _createViewModel(parameter);
        }
    }
}
