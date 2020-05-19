﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfOpenControls.DockManager
{
    public class Command : System.Windows.Input.ICommand
    {
        #region Properties

        private readonly Action<object> ExecuteAction;
        private readonly Predicate<object> CanExecuteAction;

        #endregion

        public Command(Action<object> execute) : this(execute, _ => true)
        {
        }

        public Command(Action<object> action, Predicate<object> canExecute)
        {
            ExecuteAction = action;
            CanExecuteAction = canExecute;
        }

        #region Methods

        public bool CanExecute(object parameter)
        {
            return CanExecuteAction(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            ExecuteAction(parameter);
        }

        #endregion
    }
}
