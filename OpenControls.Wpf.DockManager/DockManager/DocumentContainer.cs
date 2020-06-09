﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenControls.Wpf.DockManager
{
    internal class DocumentContainer : ViewContainer
    {
        public DocumentContainer()
        {
            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(4, System.Windows.GridUnitType.Pixel) });
            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(4, System.Windows.GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Pixel) });

            CreateTabControl(1, 0);
            TabHeaderControl.ItemContainerStyle = FindResource("DocumentPaneTabItem") as Style;

            _gap = new Border();
            _gap.SetResourceReference(Border.HeightProperty, "DocumentPaneGapHeight");
            _gap.SetResourceReference(Border.BackgroundProperty, "DocumentPaneGapBrush");
            Children.Add(_gap);
            Grid.SetRow(_gap, 2);
            Grid.SetColumn(_gap, 0);
            Grid.SetColumnSpan(_gap, 6);

            _commandsButton = new Button();
            Children.Add(_commandsButton);
            Grid.SetRow(_commandsButton, 1);
            Grid.SetColumn(_commandsButton, 2);
            _commandsButton.Click += delegate { if (DisplayGeneralMenu != null) DisplayGeneralMenu(); };
            _commandsButton.SetResourceReference(StyleProperty, "DocumentPaneCommandsButtonStyle");

            _listButton = new Button();
            Children.Add(_listButton);
            Grid.SetRow(_listButton, 1);
            Grid.SetColumn(_listButton, 4);
            _listButton.Click += delegate { Helpers.DisplayItemsMenu(_items, TabHeaderControl, _selectedUserControl); };
            _listButton.SetResourceReference(StyleProperty, "DocumentPaneListButtonStyle");

            TabHeaderControl.ActiveArrowBrush = FindResource("DocumentPaneActiveScrollIndicatorBrush") as Brush;
            TabHeaderControl.InactiveArrowBrush = FindResource("DocumentPaneInactiveScrollIndicatorBrush") as Brush;
        }

        public void HideCommandsButton()
        {
            _commandsButton.Visibility = Visibility.Collapsed;
        }

        public Style CommandsButtonStyle
        {
            set
            {
                if (value != null)
                {
                    _commandsButton.Style = value;
                }
            }
        }

        private Button _commandsButton;

        public Action DisplayGeneralMenu;

        protected override void SetSelectedUserControlGridPosition()
        {
            Grid.SetRow(_selectedUserControl, 3);
            Grid.SetColumn(_selectedUserControl, 0);
            Grid.SetColumnSpan(_selectedUserControl, 99);
            Grid.SetZIndex(_selectedUserControl, 2);
        }

        protected override void CheckTabCount()
        {
            // No need to do anything ... 
        }
    }
}
