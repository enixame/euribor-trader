using System;
using System.Windows.Input;

namespace Trading.Client.Wpf.ViewModels
{
    /// <summary>
    /// Simple delegate-based implementation of ICommand.  When executed,
    /// invokes the provided <see cref="Action"/>.  CanExecute optionally
    /// evaluates a predicate.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}