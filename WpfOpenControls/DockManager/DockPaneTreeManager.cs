﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfOpenControls.DockManager
{
    internal class DockPaneTreeManager : IDockPaneTreeManager
    {
        public DockPaneTreeManager(IDockPaneTree iDockPaneTree, ILayoutFactory iLayoutFactory)
        {
            ILayoutFactory = iLayoutFactory;
            IDockPaneTree = iDockPaneTree;
        }

        private readonly IDockPaneTree IDockPaneTree;
        private readonly ILayoutFactory ILayoutFactory;

        public DockPane ExtractDockPane(DockPane dockPane, out FrameworkElement frameworkElement)
        {
            frameworkElement = null;

            if (dockPane == null)
            {
                return null;
            }

            Grid parentGrid = dockPane.Parent as Grid;
            System.Diagnostics.Trace.Assert(parentGrid != null, System.Reflection.MethodBase.GetCurrentMethod().Name + ": DockPane parent must be a Grid");

            if (parentGrid == IDockPaneTree)
            {
                IDockPaneTree.Children.Remove(dockPane);
            }
            else
            {
                Grid grandparentGrid = parentGrid.Parent as Grid;
                System.Diagnostics.Trace.Assert(grandparentGrid != null, System.Reflection.MethodBase.GetCurrentMethod().Name + ": Grid parent not a Grid");

                IDockPaneTree.FrameworkElementRemoved(dockPane);
                parentGrid.Children.Remove(dockPane);

                if (!(parentGrid is DocumentPanel))
                {
                    foreach (var item in parentGrid.Children)
                    {
                        if (!(item is GridSplitter))
                        {
                            frameworkElement = item as FrameworkElement;
                            break;
                        }
                    }

                    System.Diagnostics.Trace.Assert(frameworkElement != null);

                    parentGrid.Children.Remove(frameworkElement);
                    int row = Grid.GetRow(parentGrid);
                    int column = Grid.GetColumn(parentGrid);
                    IDockPaneTree.FrameworkElementRemoved(parentGrid);
                    grandparentGrid.Children.Remove(parentGrid);
                    if (grandparentGrid == IDockPaneTree)
                    {
                        IDockPaneTree.RootPane = frameworkElement as Grid;
                    }
                    else
                    {
                        grandparentGrid.Children.Add(frameworkElement);
                        Grid.SetRow(frameworkElement, row);
                        Grid.SetColumn(frameworkElement, column);
                    }
                }
            }

            return dockPane;
        }

        public bool UngroupDockPane(DockPane dockPane, int index, double paneWidth)
        {
            System.Diagnostics.Trace.Assert(dockPane != null, System.Reflection.MethodBase.GetCurrentMethod().Name + ": dockPane is null");

            int viewCount = dockPane.IViewContainer.GetUserControlCount();
            if (viewCount < 2)
            {
                // Cannot ungroup one item!
                return false;
            }

            // The parent must be a SplitterPane or the LayoutManager
            Grid parentSplitterPane = dockPane.Parent as Grid;
            System.Diagnostics.Trace.Assert(parentSplitterPane != null, System.Reflection.MethodBase.GetCurrentMethod().Name + ": dockPane.Parent not a Grid");

            UserControl userControl = dockPane.IViewContainer.ExtractUserControl(index);
            if (userControl == null)
            {
                return false;
            }

            DockPane newDockPane = (dockPane is ToolPaneGroup) ? (DockPane)ILayoutFactory.MakeToolPaneGroup() : ILayoutFactory.MakeDocumentPaneGroup();
            newDockPane.IViewContainer.AddUserControl(userControl);

            parentSplitterPane.Children.Remove(dockPane);

            SplitterPane newGrid = ILayoutFactory.MakeSplitterPane(false);
            parentSplitterPane.Children.Add(newGrid);
            Grid.SetRow(newGrid, Grid.GetRow(dockPane));
            Grid.SetColumn(newGrid, Grid.GetColumn(dockPane));

            newGrid.AddChild(dockPane, true);
            newGrid.AddChild(newDockPane, false);

            return true;
        }

        public void Float(DockPane dockPane, FloatEventArgs e, bool selectedTabOnly)
        {
            if (!selectedTabOnly || (dockPane.IViewContainer.GetUserControlCount() == 1))
            {
                ExtractDockPane(dockPane, out FrameworkElement frameworkElement);
            }

            Point mainWindowLocation = Application.Current.MainWindow.PointToScreen(new Point(0, 0));

            FloatingPane floatingPane = null;
            if (dockPane is ToolPaneGroup)
            {
                floatingPane = ILayoutFactory.MakeFloatingToolPaneGroup();
            }
            else
            {
                floatingPane = ILayoutFactory.MakeFloatingDocumentPaneGroup();
            }

            int index = selectedTabOnly ? dockPane.IViewContainer.GetCurrentTabIndex() : 0;
            while (true)
            {
                UserControl userControl = dockPane.IViewContainer.ExtractUserControl(index);
                if (userControl == null)
                {
                    break;
                }

                floatingPane.IViewContainer.AddUserControl(userControl);

                if (selectedTabOnly)
                {
                    break;
                }
            }

            if (e.Drag)
            {
                IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
                WpfOpenControls.Controls.Utilities.SendLeftMouseButtonUp(hWnd);

                // Ensure the floated window can be dragged by the user
                hWnd = new System.Windows.Interop.WindowInteropHelper(floatingPane).EnsureHandle();
                WpfOpenControls.Controls.Utilities.SendLeftMouseButtonDown(hWnd);
            }

            Point cursorPositionOnScreen = WpfOpenControls.Controls.Utilities.GetCursorPosition();
            floatingPane.Left = cursorPositionOnScreen.X - 30;
            floatingPane.Top = cursorPositionOnScreen.Y - 30;
            floatingPane.Width = dockPane.ActualWidth;
            floatingPane.Height = dockPane.ActualHeight;
        }

        public SelectablePane FindSelectablePane(Grid grid, Point pointOnScreen)
        {
            if (grid == null)
            {
                return null;
            }

            foreach (var child in grid.Children)
            {
                if ((child is SelectablePane) || (child is SplitterPane))
                {
                    Grid childGrid = child as Grid;
                    Point pointInToolPane = childGrid.PointFromScreen(pointOnScreen);
                    if (
                            (pointInToolPane.X >= 0) &&
                            (pointInToolPane.X <= childGrid.ActualWidth) &&
                            (pointInToolPane.Y >= 0) &&
                            (pointInToolPane.Y <= childGrid.ActualHeight)
                        )
                    {
                        if (child is DocumentPanel)
                        {
                            if (!(child as DocumentPanel).ContainsDocuments())
                            {
                                return child as DocumentPanel;
                            }
                        }
                        else if (child is DockPane)
                        {
                            return child as DockPane;
                        }

                        return FindSelectablePane(childGrid, pointOnScreen);
                    }
                }
            }

            return null;
        }
        
        private static void ExtractDocuments(FloatingPane floatingPane, DockPane dockPane)
        {
            while (true)
            {
                UserControl userControl = floatingPane.IViewContainer.ExtractUserControl(0);
                if (userControl == null)
                {
                    break;
                }

                dockPane.IViewContainer.AddUserControl(userControl);
            }
            floatingPane.Close();
        }

        public void Unfloat(FloatingPane floatingPane, SelectablePane selectedPane, WindowLocation windowLocation)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (
                        (floatingPane == null) ||
                        ((selectedPane != null) && !(selectedPane.Parent is SplitterPane) && !(selectedPane.Parent is DocumentPanel) && (selectedPane.Parent != IDockPaneTree))
                   )
                {
                    return;
                }

                SplitterPane parentSplitterPane = null;
                DockPane dockPane = null;

                switch (windowLocation)
                {
                    case WindowLocation.BottomSide:
                    case WindowLocation.TopSide:
                    case WindowLocation.LeftSide:
                    case WindowLocation.RightSide:

                        if (floatingPane is FloatingToolPaneGroup)
                        {
                            dockPane = ILayoutFactory.MakeToolPaneGroup();
                        }
                        else
                        {
                            dockPane = ILayoutFactory.MakeDocumentPaneGroup();
                        }
                        ExtractDocuments(floatingPane, dockPane);

                        parentSplitterPane = ILayoutFactory.MakeSplitterPane((windowLocation == WindowLocation.TopSide) || (windowLocation == WindowLocation.BottomSide));
                        bool isFirst = (windowLocation == WindowLocation.TopSide) || (windowLocation == WindowLocation.LeftSide);
                        parentSplitterPane.AddChild(dockPane, isFirst);

                        if (IDockPaneTree.Children.Count == 0)
                        {
                            IDockPaneTree.Children.Add(parentSplitterPane);
                        }
                        else
                        {
                            Grid rootPane = IDockPaneTree.RootPane;
                            IDockPaneTree.RootPane = parentSplitterPane;
                            parentSplitterPane.AddChild(rootPane, !isFirst);
                        }
                        break;

                    case WindowLocation.Right:
                    case WindowLocation.Left:
                    case WindowLocation.Top:
                    case WindowLocation.Bottom:

                        if (floatingPane is FloatingToolPaneGroup)
                        {
                            dockPane = ILayoutFactory.MakeToolPaneGroup();
                        }
                        else
                        {
                            dockPane = ILayoutFactory.MakeDocumentPaneGroup();
                        }
                        ExtractDocuments(floatingPane, dockPane);

                        SplitterPane newGrid = ILayoutFactory.MakeSplitterPane((windowLocation == WindowLocation.Top) || (windowLocation == WindowLocation.Bottom));

                        if (selectedPane.Parent is DocumentPanel)
                        {
                            DocumentPanel documentPanel = selectedPane.Parent as DocumentPanel;
                            documentPanel.Children.Remove(selectedPane);
                            documentPanel.Children.Add(newGrid);
                        }
                        else
                        {
                            parentSplitterPane = (selectedPane.Parent as SplitterPane);
                            parentSplitterPane.Children.Remove(selectedPane);
                            parentSplitterPane.Children.Add(newGrid);
                            Grid.SetRow(newGrid, Grid.GetRow(selectedPane));
                            Grid.SetColumn(newGrid, Grid.GetColumn(selectedPane));
                        }

                        bool isTargetFirst = (windowLocation == WindowLocation.Right) || (windowLocation == WindowLocation.Bottom);
                        newGrid.AddChild(selectedPane, isTargetFirst);
                        newGrid.AddChild(dockPane, !isTargetFirst);
                        break;

                    case WindowLocation.Middle:

                        if (selectedPane is DockPane)
                        {
                            ExtractDocuments(floatingPane, selectedPane as DockPane);
                        }
                        else if (selectedPane is DocumentPanel)
                        {
                            DocumentPaneGroup documentPaneGroup = ILayoutFactory.MakeDocumentPaneGroup();
                            selectedPane.Children.Add(documentPaneGroup);
                            ExtractDocuments(floatingPane, documentPaneGroup);
                        }
                        break;
                }

                Application.Current.MainWindow.Activate();
            });
        }
    }
}
