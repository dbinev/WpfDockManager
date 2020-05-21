﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfDockManagerDemo.DockManager
{
    internal class ToolPaneGroup : DockPane
    {
        public ToolPaneGroup() : base(new ToolContainer())
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinitions.Add(columnDefinition);

            columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinitions.Add(columnDefinition);

            columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinitions.Add(columnDefinition);

            columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinitions.Add(columnDefinition);

            columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinitions.Add(columnDefinition);

            RowDefinitions.Add(new RowDefinition());
            RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
            RowDefinitions.Add(new RowDefinition());
            RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

            HeaderBorder = new Border();
            HeaderBorder.VerticalAlignment = VerticalAlignment.Stretch;
            HeaderBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            HeaderBorder.BorderBrush = System.Windows.Media.Brushes.Black;
            HeaderBackground = System.Windows.Media.Brushes.MidnightBlue;
            HeaderBorder.BorderThickness = new Thickness(1);
            Grid.SetRow(HeaderBorder, 0);
            Grid.SetColumn(HeaderBorder, 0);
            Grid.SetColumnSpan(HeaderBorder, 5);
            Children.Add(HeaderBorder);

            _titleLabel = new Label();
            _titleLabel.FontSize = 12;
            _titleLabel.Padding = new Thickness(4, 0, 0, 0);
            _titleLabel.VerticalAlignment = VerticalAlignment.Center;
            _titleLabel.Background = System.Windows.Media.Brushes.Transparent;
            _titleLabel.Foreground = System.Windows.Media.Brushes.White;
            Grid.SetRow(_titleLabel, 0);
            Grid.SetColumn(_titleLabel, 0);
            Children.Add(_titleLabel);

            System.Windows.ResourceDictionary res = Utilities.GetResourceDictionary();

            Button menuButton = new Button();
            menuButton.VerticalAlignment = VerticalAlignment.Center;
            menuButton.Style = res["StyleViewListButton"] as Style;
            menuButton.Click += delegate { DisplayGeneralMenu(); };
            Grid.SetRow(menuButton, 0);
            Grid.SetColumn(menuButton, 2);
            Children.Add(menuButton);

            _pinButton = new Button();
            _pinButton.VerticalAlignment = VerticalAlignment.Center;
            _pinButton.LayoutTransform = new System.Windows.Media.RotateTransform();
            _pinButton.Style = res["StylePinButton"] as Style;
            _pinButton.Click += PinButton_Click;
            Grid.SetRow(_pinButton, 0);
            Grid.SetColumn(_pinButton, 3);
            Children.Add(_pinButton);

            Button closeButton = new Button();
            closeButton.VerticalAlignment = VerticalAlignment.Center;
            closeButton.Style = res["StyleCloseButton"] as Style;
            Grid.SetRow(closeButton, 0);
            Grid.SetColumn(closeButton, 4);
            Panel.SetZIndex(closeButton, 99);
            Children.Add(closeButton);
            closeButton.Click += delegate { FireClose(); };

            IViewContainer.SelectionChanged += DocumentContainer_SelectionChanged;
            Grid.SetRow(IViewContainer as System.Windows.UIElement, 1);
            Grid.SetColumn(IViewContainer as System.Windows.UIElement, 0);
            Grid.SetColumnSpan(IViewContainer as System.Windows.UIElement, ColumnDefinitions.Count);
        }

        public double FontSize
        {
            set
            {
                if (_titleLabel != null)
                {
                    _titleLabel.FontSize = value;
                }
            }
        }

        public void ShowAsUnPinned()
        {
            (_pinButton.LayoutTransform as System.Windows.Media.RotateTransform).Angle = 90.0;
            (_pinButton.LayoutTransform as System.Windows.Media.RotateTransform).CenterX = 0.5;
            (_pinButton.LayoutTransform as System.Windows.Media.RotateTransform).CenterY = 0.5;
        }

        public event EventHandler UnPinClick;

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            UnPinClick?.Invoke(this, null);
        }

        private void DocumentContainer_SelectionChanged(object sender, EventArgs e)
        {
            _titleLabel.Content = IViewContainer.Title;
        }

        protected Label _titleLabel;

        public string Title { get { return IViewContainer.Title; } }

        Button _pinButton;

        Point _mouseDownPosition;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _mouseDownPosition = e.GetPosition(this);
            base.OnMouseLeftButtonDown(e);
            System.Windows.Input.Mouse.Capture(this);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            System.Windows.Input.Mouse.Capture(this, CaptureMode.None);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (System.Windows.Input.Mouse.Captured == this)
            {
                Point mousePosition = e.GetPosition(this);
                double xdiff = mousePosition.X - _mouseDownPosition.X;
                double ydiff = mousePosition.Y - _mouseDownPosition.Y;
                if ((xdiff * xdiff + ydiff * ydiff) > 200)
                {

                    FireFloat(true);
                    System.Windows.Input.Mouse.Capture(this, CaptureMode.None);
                }
            }
        }
    }

}