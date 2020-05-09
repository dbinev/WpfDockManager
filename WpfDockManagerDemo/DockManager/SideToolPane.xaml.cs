﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDockManagerDemo.DockManager
{
    /// <summary>
    /// Interaction logic for SidePane.xaml
    /// </summary>
    public partial class SideToolPane : Window
    {
        public SideToolPane()
        {
            InitializeComponent();
            _toolPane.ShowAsUnPinned();
        }

        private void _buttonMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        internal ToolPane ToolPane { get { return _toolPane; } }

    }
}