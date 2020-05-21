﻿using System.Windows.Controls;

namespace ExampleDockManagerViews.View
{
    /// <summary>
    /// Interaction logic for DemoFiveView.xaml
    /// </summary>
    public partial class ToolFiveView : UserControl, WpfOpenControls.DockManager.IView
    {
        public ToolFiveView()
        {
            InitializeComponent();
        }

        public WpfOpenControls.DockManager.IViewModel IViewModel { get; set; }
    }
}