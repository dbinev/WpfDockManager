﻿using System;
using System.Windows;

namespace WpfOpenControls.DockManager
{
    internal class FloatingDocumentPaneGroup : FloatingPane
    {
        internal FloatingDocumentPaneGroup() : base(new DocumentContainer())
        {
            IViewContainer.SelectionChanged += IViewContainer_SelectionChanged;
        }

        private void IViewContainer_SelectionChanged(object sender, EventArgs e)
        {
            FloatingViewModel floatingViewModel = DataContext as FloatingViewModel;
            System.Diagnostics.Trace.Assert(floatingViewModel != null);

            floatingViewModel.Title = Application.Current.MainWindow.Title + " - " + IViewContainer.URL;
        }
    }
}
