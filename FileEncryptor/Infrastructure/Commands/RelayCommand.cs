using FileEncryptor.Infrastructure.Commands.Base;
using System;

namespace FileEncryptor.Infrastructure.Commands
{
    internal class RelayCommand : Command
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(
                execute: p => execute(),
                canExecute: canExecute is null ? (Func<object, bool>)null : p => canExecute())
        {

        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(Execute));
            _canExecute = canExecute;
        }

        protected override bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        protected override void Execute(object parameter) => _execute(parameter);
    }
}
