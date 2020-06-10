﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Xml;
using System.Windows.Input;
using System.Collections.ObjectModel;
using OpenControls.Wpf.DockManager.Controls;

namespace OpenControls.Wpf.DockManager
{
    public class LayoutManager : System.Windows.Controls.Grid, ILayoutFactory, IDockPaneTree, IUnpinnedToolParent
    {
        public LayoutManager()
        {
            Tag = new Guid("3c81a424-ef66-4de7-a361-9968cd88071c");

            IDockPaneTreeManager = new DockPaneTreeManager(this, this);
            IUnpinnedToolManager = new UnpinnedToolManager(IDockPaneTreeManager, this, this);
        }

        public void Initialise()
        {
            System.Diagnostics.Trace.Assert(Theme != null);

            ResourceDictionary dictionary = new ResourceDictionary() { Source = Theme.Uri };
            Application.Current.Resources.MergedDictionaries.Add(dictionary);

            FloatingToolPaneGroups = new List<IFloatingPane>();
            FloatingDocumentPaneGroups = new List<IFloatingPane>();

            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });

            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto) });
            CreateToolListBoxes();

            Application.Current.MainWindow.LocationChanged += MainWindow_LocationChanged;
            PreviewMouseDown += LayoutManager_PreviewMouseDown;
            SizeChanged += LayoutManager_SizeChanged;
        }

        private void LayoutManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_root == null)
            {
                return;
            }

            Point cursorPoint = Utilities.GetCursorPosition();
            Point topLeftPoint = _root.PointToScreen(new Point(0, 0));
            if (
                    (cursorPoint.X > topLeftPoint.X) &&
                    (cursorPoint.Y > topLeftPoint.Y) &&
                    (cursorPoint.X < (topLeftPoint.X + _root.ActualWidth)) &&
                    (cursorPoint.Y < (topLeftPoint.Y + _root.ActualHeight))
                )
            {
                IUnpinnedToolManager.CloseUnpinnedToolPane();
            }
        }

        ~LayoutManager()
        {
            Shutdown();
        }

        private ILayoutFactory ILayoutFactory
        {
            get
            {
                return this;
            }
        }
        private IDockPaneTree IDockPaneTree
        {
            get
            {
                return this;
            }
        }
        private readonly IDockPaneTreeManager IDockPaneTreeManager;
        private readonly IUnpinnedToolManager IUnpinnedToolManager;

        public void Shutdown()
        {
            try
            {
                _windowLocationPane?.Close();
                _windowLocationPane = null;
            }
            catch
            {
                // Ignore
            }
            try
            {

                _sideLocationPane?.Close();
                _sideLocationPane = null;
            }
            catch
            {
                // Ignore
            }
        }

        public DataTemplate ToolPaneTitleTemplate { get; set; }
        public DataTemplate ToolPaneHeaderTemplate { get; set; }
        public DataTemplate DocumentPaneTitleTemplate { get; set; }
        public DataTemplate DocumentPaneHeaderTemplate { get; set; }

        public event EventHandler DocumentClosed;
        public event EventHandler ToolClosed;

        private List<IFloatingPane> FloatingToolPaneGroups;
        private List<IFloatingPane> FloatingDocumentPaneGroups;

        private Dictionary<WindowLocation, Controls.ToolListBox> _dictToolListBoxes;

        private Grid _root;

        private SelectablePane SelectedPane;

        private void ValidateFloatingPanes(Dictionary<IViewModel, List<string>> viewModels, List<IFloatingPane> floatingPanes)
        {
            int count = floatingPanes.Count;
            for (int paneIndex = count - 1; paneIndex > -1; --paneIndex)
            {
                IViewContainer iViewContainer = floatingPanes[paneIndex].IViewContainer;
                int tabCount = iViewContainer.GetUserControlCount();

                for (int index = tabCount - 1; index > -1; --index)
                {
                    IViewModel iViewModel = iViewContainer.GetIViewModel(index);
                    if (viewModels.ContainsKey(iViewModel) && (viewModels[iViewModel].Contains(iViewModel.URL)))
                    {
                        viewModels[iViewModel].Remove(iViewModel.URL);
                        if (viewModels[iViewModel].Count == 0)
                        {
                            viewModels.Remove(iViewModel);
                        }
                    }
                    else
                    {
                        iViewContainer.ExtractUserControl(index);
                    }
                }

                if (iViewContainer.GetUserControlCount() == 0)
                {
                    floatingPanes[paneIndex].Close();
                }
            }
        }

        private Dictionary<IViewModel, List<string>> CreateViewModelUrlDictionary(ObservableCollection<IViewModel> viewModels)
        {
            // The string list contains each URL for the given view model
            Dictionary<IViewModel, List<string>> mapViewModels = new Dictionary<IViewModel, List<string>>();

            foreach (var iViewModel in viewModels)
            {
                if (!mapViewModels.ContainsKey(iViewModel))
                {
                    mapViewModels.Add(iViewModel, new List<string>());
                }
                mapViewModels[iViewModel].Add(iViewModel.URL);
            }

            return mapViewModels;
        }

        private void ValidateToolPanes()
        {
            Dictionary<IViewModel, List<string>> viewModelUrlDictionary = CreateViewModelUrlDictionary(ToolsSource);

            IUnpinnedToolManager.Validate(viewModelUrlDictionary);

            ValidateFloatingPanes(viewModelUrlDictionary, FloatingToolPaneGroups);
            IDockPaneTreeManager.ValidateDockPanes(_root, viewModelUrlDictionary, typeof(ToolPaneGroup));
        }

        private void ValidateDocumentPanes()
        {
            Dictionary<IViewModel, List<string>> viewModelUrlDictionary = CreateViewModelUrlDictionary(DocumentsSource);

            ValidateFloatingPanes(viewModelUrlDictionary, FloatingDocumentPaneGroups);
            IDockPaneTreeManager.ValidateDockPanes(_root, viewModelUrlDictionary, typeof(DocumentPaneGroup));
        }

        #region dependency properties 

        #region Theme

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme", typeof(Themes.Theme), typeof(LayoutManager), new FrameworkPropertyMetadata(null, null));

        public Themes.Theme Theme
        {
            get
            {
                return (Themes.Theme)GetValue(ThemeProperty);
            }
            set
            {
                if (value != Theme)
                {
                    SetValue(ThemeProperty, value);
                }
            }
        }

        #endregion

        #region DocumentsSource dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty DocumentsSourceProperty = DependencyProperty.Register("DocumentsSource", typeof(ObservableCollection<IViewModel>), typeof(LayoutManager), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDocumentsSourceChanged)));

        private void LayoutManager_DocumentsSourceCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ValidateDocumentPanes();
        }

        public ObservableCollection<IViewModel> DocumentsSource
        {
            get
            {
                return (ObservableCollection<IViewModel>)GetValue(DocumentsSourceProperty);
            }
            set
            {
                SetValue(DocumentsSourceProperty, value);
                (value as System.Collections.Specialized.INotifyCollectionChanged).CollectionChanged += LayoutManager_DocumentsSourceCollectionChanged;
            }
        }

        private static void OnDocumentsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutManager)d).OnDocumentsSourceChanged(e);
        }

        protected virtual void OnDocumentsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DocumentsSource = (ObservableCollection<IViewModel>)e.NewValue;
            }
        }

        #endregion

        #region ToolsSource dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty ToolsSourceProperty = DependencyProperty.Register("ToolsSource", typeof(ObservableCollection<IViewModel>), typeof(LayoutManager), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnToolsSourceChanged)));

        private void LayoutManager_ToolsSourceCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ValidateToolPanes();
        }

        public ObservableCollection<IViewModel> ToolsSource
        {
            get
            {
                return (ObservableCollection<IViewModel>)GetValue(ToolsSourceProperty);
            }
            set
            {
                SetValue(ToolsSourceProperty, value);
                (value as System.Collections.Specialized.INotifyCollectionChanged).CollectionChanged += LayoutManager_ToolsSourceCollectionChanged;
            }
        }

        private static void OnToolsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutManager)d).OnToolsSourceChanged(e);
        }

        protected virtual void OnToolsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ToolsSource = e.NewValue as ObservableCollection<IViewModel>;
            }
        }

        #endregion

        #region DocumentTemplates dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty DocumentTemplatesProperty = DependencyProperty.Register("DocumentTemplates", typeof(List<DataTemplate>), typeof(LayoutManager), new FrameworkPropertyMetadata(new List<DataTemplate>(), new PropertyChangedCallback(OnDocumentTemplatesChanged)));

        public List<DataTemplate> DocumentTemplates
        {
            get
            {
                return (List<DataTemplate>)GetValue(DocumentTemplatesProperty);
            }
            set
            {
                SetValue(DocumentTemplatesProperty, value);
            }
        }

        private static void OnDocumentTemplatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutManager)d).OnDocumentTemplatesChanged(e);
        }

        protected virtual void OnDocumentTemplatesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DocumentTemplates = (List<DataTemplate>)e.NewValue;
            }
        }

        #endregion

        #region ToolTemplates dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty ToolTemplatesProperty = DependencyProperty.Register("ToolTemplates", typeof(System.Collections.Generic.List<DataTemplate>), typeof(LayoutManager), new FrameworkPropertyMetadata(new List<DataTemplate>(), new PropertyChangedCallback(OnToolTemplatesChanged)));

        public System.Collections.Generic.List<DataTemplate> ToolTemplates
        {
            get
            {
                return (System.Collections.Generic.List<DataTemplate>)GetValue(ToolTemplatesProperty);
            }
            set
            {
                SetValue(ToolTemplatesProperty, value);
            }
        }

        private static void OnToolTemplatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutManager)d).OnToolTemplatesChanged(e);
        }

        protected virtual void OnToolTemplatesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ToolTemplates = (System.Collections.Generic.List<DataTemplate>)e.NewValue;
            }
        }

        #endregion

        #endregion

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            IUnpinnedToolManager.ProcessMoveResize();
        }

        private void LayoutManager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IUnpinnedToolManager.ProcessMoveResize();
        }

        private void ToolListBox_ItemClick(object sender, Events.ItemClickEventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is ToolListBoxItem);
            System.Diagnostics.Trace.Assert((e != null) && (e.ToolListBox != null));

            IUnpinnedToolManager.ShowUnpinnedToolPane(sender as ToolListBoxItem, e.ToolListBox);
        }

        private void ToolPane_UnPinClick(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is ToolPaneGroup);

            IUnpinnedToolManager.Unpin(sender as ToolPaneGroup);
        }

        private void CreateToolListBox(int row, int column, bool isHorizontal, WindowLocation windowLocation)
        {
            ObservableCollection<Controls.IToolListBoxItem> items = new ObservableCollection<Controls.IToolListBoxItem>();
            Controls.ToolListBox toolListBox = new Controls.ToolListBox();
            toolListBox.TextAngle = ((windowLocation == WindowLocation.TopSide) || (windowLocation == WindowLocation.RightSide)) ? 180.0 : 0.0;
            toolListBox.WindowLocation = windowLocation;
            toolListBox.ItemsSource = items;
            toolListBox.IsHorizontal = isHorizontal;
            toolListBox.DisplayMemberPath = "Title";
            toolListBox.ItemContainerStyle = FindResource("SideToolItemStyle") as Style;
            toolListBox.ItemClick += ToolListBox_ItemClick;
            Children.Add(toolListBox);
            Grid.SetRow(toolListBox, row);
            Grid.SetColumn(toolListBox, column);

            _dictToolListBoxes.Add(windowLocation, toolListBox);
        }

        private void CreateToolListBoxes()
        {
            _dictToolListBoxes = new Dictionary<WindowLocation, Controls.ToolListBox>();
            CreateToolListBox(1, 0, false, WindowLocation.LeftSide);
            CreateToolListBox(1, 2, false, WindowLocation.RightSide);
            CreateToolListBox(0, 1, true, WindowLocation.TopSide);
            CreateToolListBox(2, 1, true, WindowLocation.BottomSide);
        }

        public void Clear()
        {
            Children.Clear();
            IUnpinnedToolManager.Clear();
            CreateToolListBoxes();
            while (FloatingToolPaneGroups.Count > 0)
            {
                FloatingToolPaneGroups[0].Close();
            }
            FloatingToolPaneGroups.Clear();
            while (FloatingDocumentPaneGroups.Count > 0)
            {
                FloatingDocumentPaneGroups[0].Close();
            }
            FloatingDocumentPaneGroups.Clear();
        }

        public List<UserControl> LoadViewsFromTemplates(List<DataTemplate> dataTemplates, ObservableCollection<IViewModel> viewModels)
        {
            List<UserControl> views = new List<UserControl>();

            if ((dataTemplates == null) || (dataTemplates.Count == 0) || (viewModels == null))
            {
                return views;
            }

            // First load the views and view models

            foreach (var viewModel in viewModels)
            {
                foreach (var item in dataTemplates)
                {
                    if (item is DataTemplate)
                    {
                        DataTemplate dataTemplate = item as DataTemplate;

                        if (viewModel.GetType() == (Type)dataTemplate.DataType)
                        {
                            UserControl view = (dataTemplate.LoadContent() as UserControl);
                            if (view != null)
                            {
                                view.DataContext = viewModel;
                                view.HorizontalAlignment = HorizontalAlignment.Stretch;
                                view.VerticalAlignment = VerticalAlignment.Stretch;

                                views.Add(view);
                            }
                        }
                    }
                }
            }

            return views;
        }

        #region ILayoutFactory

        DocumentPanel ILayoutFactory.MakeDocumentPanel()
        {
            return new DocumentPanel();
        }

        SplitterPane ILayoutFactory.MakeSplitterPane(bool isHorizontal)
        {
            return new SplitterPane(isHorizontal);
        }

        private void RegisterDockPane(DockPane dockPane)
        {
            System.Diagnostics.Trace.Assert(dockPane != null);

            dockPane.CloseRequest += DockPane_CloseRequest;
            dockPane.Float += DockPane_Float;
            dockPane.FloatTabRequest += DockPane_FloatTabRequest;
            dockPane.UngroupCurrent += DockPane_UngroupCurrent;
            dockPane.Ungroup += DockPane_Ungroup;
            dockPane.TabClosed += DockPane_TabClosed;
        }

        DocumentPaneGroup ILayoutFactory.MakeDocumentPaneGroup()
        {
            DocumentPaneGroup documentPaneGroup = new DocumentPaneGroup();
            RegisterDockPane(documentPaneGroup);
            return documentPaneGroup;
        }

        ToolPaneGroup ILayoutFactory.MakeToolPaneGroup()
        {
            ToolPaneGroup toolPaneGroup = new ToolPaneGroup();
            RegisterDockPane(toolPaneGroup);
            toolPaneGroup.UnPinClick += ToolPane_UnPinClick;
            return toolPaneGroup;
        }

        private void RegisterFloatingPane(FloatingPane floatingPane)
        {
            floatingPane.LocationChanged += FloatingWindow_LocationChanged;
            floatingPane.Closed += FloatingPane_Closed;
            floatingPane.Ungroup += FloatingPane_Ungroup;
            floatingPane.UngroupCurrent += FloatingPane_UngroupCurrent;
            floatingPane.DataContext = new FloatingViewModel();
            (floatingPane.DataContext as FloatingViewModel).Title = floatingPane.Title;
            floatingPane.EndDrag += FloatingPane_EndDrag;
            // Ensure the window remains on top of the main window
            floatingPane.Owner = Application.Current.MainWindow;
            floatingPane.Show();
        }

        FloatingDocumentPaneGroup ILayoutFactory.MakeFloatingDocumentPaneGroup()
        {
            FloatingDocumentPaneGroup floatingDocumentPaneGroup = new FloatingDocumentPaneGroup();
            RegisterFloatingPane(floatingDocumentPaneGroup);
            FloatingDocumentPaneGroups.Add(floatingDocumentPaneGroup);
            return floatingDocumentPaneGroup;
        }

        FloatingToolPaneGroup ILayoutFactory.MakeFloatingToolPaneGroup()
        {
            FloatingToolPaneGroup floatingToolPaneGroup = new FloatingToolPaneGroup();
            RegisterFloatingPane(floatingToolPaneGroup);
            FloatingToolPaneGroups.Add(floatingToolPaneGroup);
            return floatingToolPaneGroup;
        }

        void ILayoutFactory.MakeUnpinnedToolPaneGroup(WindowLocation windowLocation, ToolPaneGroup toolPaneGroup, string siblingGuid, bool isHorizontal, bool isFirst)
        {
            IUnpinnedToolManager.MakeUnpinnedToolPaneGroup(windowLocation, toolPaneGroup, siblingGuid, isHorizontal, isFirst);
        }

        string ILayoutFactory.MakeDocumentKey(string contentId, string Url)
        {
            return contentId + "," + Url;
        }

        void ILayoutFactory.SetRootPane(Grid grid, out int row, out int column)
        {
            IDockPaneTree.RootPane = grid;
            row = Grid.GetRow(grid);
            column = Grid.GetColumn(grid);
        }

        #endregion ILayoutFactory

        #region IDockPaneTree

        void IDockPaneTree.FrameworkElementRemoved(FrameworkElement frameworkElement)
        {
            IUnpinnedToolManager.FrameworkElementRemoved(frameworkElement);
        }

        Grid IDockPaneTree.RootPane
        {
            get
            {
                return _root;
            }
            set
            {

                if ((_root != null) && Children.Contains(_root))
                {
                    Children.Remove(_root);
                }
                _root = value;
                Children.Add(_root);
                Grid.SetRow(_root, 1);
                Grid.SetColumn(_root, 1);
            }
        }

        Grid IDockPaneTree.ParentGrid
        {
            get
            {
                return this;
            }
        }

        List<UserControl> IDockPaneTree.LoadToolViews(ObservableCollection<IViewModel> viewModels)
        {
            return LoadViewsFromTemplates(ToolTemplates, viewModels);
        }

        List<UserControl> IDockPaneTree.LoadDocumentViews(ObservableCollection<IViewModel> viewModels)
        {
            return LoadViewsFromTemplates(DocumentTemplates, viewModels);
        }

        #endregion IDockPaneTree

        public bool SaveLayoutToFile(string fileNameAndPath)
        {
            XmlDocument xmlDocument = new XmlDocument();

            if (Children.Count == 0)
            {
                return false;
            }

            Serialisation.LayoutWriter.SaveLayout(
                xmlDocument,
                _root,
                FloatingToolPaneGroups,
                FloatingDocumentPaneGroups,
                IUnpinnedToolManager.GetUnpinnedToolData());

            xmlDocument.Save(fileNameAndPath);

            return true;
        }

        private void LoadDefaultLayout()
        {
            List<UserControl> documentViews = LoadViewsFromTemplates(DocumentTemplates, DocumentsSource);
            List<UserControl> toolViews = LoadViewsFromTemplates(ToolTemplates, ToolsSource);

            IDockPaneTreeManager.CreateDefaultLayout(documentViews, toolViews);
            UpdateLayout();
            CancelSelection();
        }

        private bool Load(XmlDocument xmlDocument)
        {
            if (xmlDocument.ChildNodes.Count == 0)
            {
                return false;
            }

            List<UserControl> documentViews = LoadViewsFromTemplates(DocumentTemplates, DocumentsSource);
            List<UserControl> toolViews = LoadViewsFromTemplates(ToolTemplates, ToolsSource);

            List<UserControl> views = new List<UserControl>();
            Dictionary<string, UserControl> viewsMap = new Dictionary<string, UserControl>();

            foreach (var item in documentViews)
            {
                viewsMap.Add(ILayoutFactory.MakeDocumentKey(item.Name, ((item.DataContext) as IViewModel).URL), item);
            }

            foreach (var item in toolViews)
            {
                viewsMap.Add(item.Name, item);
            }

            // Now load the views into the dock manager => one or more views might not be visible!

            Serialisation.LayoutReader.LoadNode(this, viewsMap, this, this, xmlDocument.DocumentElement, true);

            // Remove any view without a view model 

            ValidateToolPanes();
            ValidateDocumentPanes();
            CancelSelection();

            return true;
        }

        public bool LoadLayout(string layout)
        {
            if (string.IsNullOrEmpty(layout))
            {
                LoadDefaultLayout();
                return true;
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(layout);
            return Load(xmlDocument);
        }

        public bool LoadLayoutFromFile(string fileNameAndPath)
        {
            if (string.IsNullOrEmpty(fileNameAndPath))
            {
                LoadDefaultLayout();
                return true;
            }

            Clear();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileNameAndPath);
            return Load(xmlDocument);
        }

        private void DockPane_Ungroup(object sender, EventArgs e)
        {
            DockPane dockPane = sender as DockPane;
            var parentGrid = dockPane.Parent as Grid;

            double paneWidth = dockPane.ActualWidth / dockPane.IViewContainer.GetUserControlCount();
            while (IDockPaneTreeManager.UngroupDockPane(dockPane, 1, paneWidth))
            {
                // Nothing here
            }
        }

        private void DockPane_UngroupCurrent(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is DockPane);

            DockPane dockPane = sender as DockPane;

            double paneWidth = dockPane.ActualWidth / 2;
            int index = dockPane.IViewContainer.GetCurrentTabIndex();
            if (index > -1)
            {
                IDockPaneTreeManager.UngroupDockPane(dockPane, index, paneWidth);
            }
        }

        private void DockPane_Float(object sender, Events.FloatEventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is DockPane);

            IDockPaneTreeManager.Float(sender as DockPane, e.Drag, false);
        }

        private void DockPane_FloatTabRequest(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is DockPane);

            IDockPaneTreeManager.Float(sender as DockPane, true, true);
        }

        private void DockPane_TabClosed(object sender, Events.TabClosedEventArgs e)
        {
            System.Diagnostics.Trace.Assert(e.UserControl.DataContext is IViewModel);
            if (sender is DocumentPaneGroup)
            {
                System.Diagnostics.Trace.Assert(DocumentsSource.Contains(e.UserControl.DataContext as IViewModel));
                DocumentsSource.Remove(e.UserControl.DataContext as IViewModel);
            }
            else if (sender is ToolPaneGroup)
            {
                System.Diagnostics.Trace.Assert(ToolsSource.Contains(e.UserControl.DataContext as IViewModel));
                ToolsSource.Remove(e.UserControl.DataContext as IViewModel);
            }
        }

        private void DockPane_CloseRequest(object sender, EventArgs e)
        {
            DockPane dockPane = sender as DockPane;

            if (dockPane == null)
            {
                return;
            }

            IDockPaneTreeManager.ExtractDockPane(dockPane, out FrameworkElement frameworkElement);

            IViewContainer iViewContainer = dockPane.IViewContainer;
            while (true)
            {
                UserControl userControl = iViewContainer.ExtractUserControl(0);
                if (userControl == null)
                {
                    break;
                }
                IViewModel iViewModel = (userControl.DataContext as IViewModel);
                if (dockPane is DocumentPaneGroup)
                {
                    System.Diagnostics.Trace.Assert(DocumentsSource.Contains(iViewModel));
                    DocumentsSource.Remove(iViewModel);
                    DocumentClosed?.Invoke(iViewModel, null);
                }
                else if (dockPane is ToolPaneGroup)
                {
                    System.Diagnostics.Trace.Assert(ToolsSource.Contains(iViewModel));
                    ToolsSource.Remove(iViewModel);
                    ToolClosed?.Invoke(iViewModel, null);
                }
            }
        }

        private void FloatingPane_Closed(object sender, EventArgs e)
        {
            FloatingPane floatingPane = sender as FloatingPane;

            int count = floatingPane.IViewContainer.GetUserControlCount();
            for (int index = count - 1; index > -1; --index)
            {
                UserControl userControl = floatingPane.IViewContainer.GetUserControl(index);
                if ((sender is FloatingToolPaneGroup))
                {
                    ToolsSource.Remove(userControl.DataContext as IViewModel);
                }
                else if ((sender is FloatingDocumentPaneGroup))
                {
                    DocumentsSource.Remove(userControl.DataContext as IViewModel);
                }
            }

            if ((sender is FloatingToolPaneGroup) && (FloatingToolPaneGroups.Contains(sender as FloatingToolPaneGroup)))
            {
                FloatingToolPaneGroups.Remove(sender as FloatingToolPaneGroup);
            }

            if ((sender is FloatingDocumentPaneGroup) && (FloatingDocumentPaneGroups.Contains(sender as FloatingDocumentPaneGroup)))
            {
                FloatingDocumentPaneGroups.Remove(sender as FloatingDocumentPaneGroup);
            }
        }

        private FloatingPane UnGroupFloatingPane(FloatingPane floatingPane, int index, double left, double top)
        {
            if (floatingPane == null)
            {
                return null;
            }

            UserControl userControl = floatingPane.IViewContainer.ExtractUserControl(index);
            if (userControl == null)
            {
                return null;
            }

            FloatingPane newFloatingPane = null;
            if (floatingPane is FloatingToolPaneGroup)
            {
                newFloatingPane = ILayoutFactory.MakeFloatingToolPaneGroup();
            }
            else
            {
                newFloatingPane = ILayoutFactory.MakeFloatingDocumentPaneGroup();
            }

            newFloatingPane.Left = left;
            newFloatingPane.Top = top;
            newFloatingPane.IViewContainer.AddUserControl(userControl);

            return floatingPane;
        }

        private void FloatingPane_Ungroup(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is FloatingPane);

            FloatingPane floatingPane = sender as FloatingPane;

            int viewCount = floatingPane.IViewContainer.GetUserControlCount();

            double left = floatingPane.Left;
            double top = floatingPane.Top;

            // Warning warning => off screen?
            for (int index = 1; index < viewCount; ++index)
            {
                left += 10;
                top += 10;
                if (UnGroupFloatingPane(floatingPane, 1, left, top) == null)
                {
                    return;
                }
            }
        }

        private void FloatingPane_UngroupCurrent(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is FloatingPane);

            FloatingPane floatingPane = sender as FloatingPane;

            int index = floatingPane.IViewContainer.GetCurrentTabIndex();
            if (index > -1)
            {
                UnGroupFloatingPane(floatingPane, index, floatingPane.Left + 10, floatingPane.Top + 10);
            }
        }

        private void CancelSelection()
        {
            if (SelectedPane != null)
            {
                SelectedPane.IsHighlighted = false;
                SelectedPane = null;
            }
            _insertionIndicatorManager?.HideInsertionIndicator();
            _windowLocationPane?.Close();
            _windowLocationPane = null;
            _sideLocationPane?.Close();
            _sideLocationPane = null;
        }

        private void FloatingPane_EndDrag(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is FloatingPane, System.Reflection.MethodBase.GetCurrentMethod().Name + ": sender not a FloatingPane");
            Application.Current.Dispatcher.Invoke(delegate
            {
                FloatingPane floatingPane = sender as FloatingPane;

                if (
                        (floatingPane == null) ||
                        ((SelectedPane != null) && !(SelectedPane.Parent is SplitterPane) && !(SelectedPane.Parent is DocumentPanel) && (SelectedPane.Parent != this)) ||
                        (_insertionIndicatorManager == null) ||
                        (_insertionIndicatorManager.WindowLocation == WindowLocation.None)
                   )
                {
                    return;
                }

                SelectablePane selectedPane = SelectedPane;
                WindowLocation windowLocation = _insertionIndicatorManager.WindowLocation;
                CancelSelection();

                IDockPaneTreeManager.Unfloat(floatingPane, selectedPane, windowLocation);

                Application.Current.MainWindow.Activate();
            });
        }

        private WindowLocationPane _windowLocationPane;
        private SideLocationPane _sideLocationPane;
        private InsertionIndicatorManager _insertionIndicatorManager;

        private void FloatingWindow_LocationChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(sender is Window);

            Window floatingWindow = sender as Window;
            Point cursorPositionOnScreen = OpenControls.Wpf.DockManager.Controls.Utilities.GetCursorPosition();

            bool foundSelectedPane = false;
            Point cursorPositionInMainWindow = PointFromScreen(cursorPositionOnScreen);
            if (
                    (cursorPositionInMainWindow.X >= 0) &&
                    (cursorPositionInMainWindow.X <= this.ActualWidth) && 
                    (cursorPositionInMainWindow.Y >= 0) &&
                    (cursorPositionInMainWindow.Y <= this.ActualHeight) 
                )
            {
                Type paneType = (sender is FloatingDocumentPaneGroup) ? typeof(DocumentPaneGroup) : typeof(ToolPaneGroup);
                var pane = IDockPaneTreeManager.FindSelectablePane(this, cursorPositionOnScreen);
                foundSelectedPane = pane != null;
                if ((pane != null) && (SelectedPane != pane))
                {
                    if (SelectedPane != null)
                    {
                        SelectedPane.IsHighlighted = false;
                    }

                    pane.IsHighlighted = true;
                    SelectedPane = pane;
                    if (_windowLocationPane != null)
                    {
                        _windowLocationPane.Close();
                        _windowLocationPane = null;
                    }

                    if ((paneType == pane.GetType()) || ((pane is DocumentPanel) && (sender is FloatingDocumentPaneGroup)))
                    {
                        _windowLocationPane = new WindowLocationPane();
                        _windowLocationPane.AllowsTransparency = true;
                        if (SelectedPane is DocumentPanel)
                        {
                            _windowLocationPane.ShowIcons(WindowLocation.Middle);
                        }
                        Point topLeftPanePoint = pane.PointToScreen(new Point(0, 0));
                        _windowLocationPane.Left = topLeftPanePoint.X;
                        _windowLocationPane.Top = topLeftPanePoint.Y;
                        _windowLocationPane.Width = SelectedPane.ActualWidth;
                        _windowLocationPane.Height = SelectedPane.ActualHeight;
                        _windowLocationPane.Show();
                    }
                }

                if (sender is FloatingToolPaneGroup)
                {
                    if (_sideLocationPane == null)
                    {
                        _sideLocationPane = new SideLocationPane();
                        _sideLocationPane.AllowsTransparency = true;
                    }

                    Point topLeftRootPoint = _root.PointToScreen(new Point(0, 0));
                    _sideLocationPane.Left = topLeftRootPoint.X;
                    _sideLocationPane.Top = topLeftRootPoint.Y;
                    _sideLocationPane.Width = _root.ActualWidth;
                    _sideLocationPane.Height = _root.ActualHeight;
                    _sideLocationPane.Show();
                }
            }
            else
            {
                if (_sideLocationPane != null)
                {
                    _sideLocationPane.Close();
                    _sideLocationPane = null;
                }
            }

            SelectablePane previousPane = SelectedPane;

            if (!foundSelectedPane)
            {
                SelectedPane = null;
            }

            if (!foundSelectedPane)
            {
                if (_windowLocationPane != null)
                {
                    _windowLocationPane.Close();
                    _windowLocationPane = null;
                }
           }

            if ((previousPane != null) && (SelectedPane != previousPane))
            {
                previousPane.IsHighlighted = false;
            }

            WindowLocation windowLocation = WindowLocation.None;

            if (_sideLocationPane != null)
            {
                windowLocation = _sideLocationPane.TrySelectIndicator(cursorPositionOnScreen);
                switch (windowLocation)
                {
                    case WindowLocation.LeftSide:
                    case WindowLocation.RightSide:
                    case WindowLocation.TopSide:
                    case WindowLocation.BottomSide:
                        if ((_insertionIndicatorManager != null) && (_insertionIndicatorManager.ParentGrid != this))
                        {
                            _insertionIndicatorManager.HideInsertionIndicator();
                            _insertionIndicatorManager = null;
                        }
                        if (_insertionIndicatorManager == null)
                        {
                            _insertionIndicatorManager = new InsertionIndicatorManager(this);
                        }
                        _insertionIndicatorManager.ShowInsertionIndicator(windowLocation);
                        return;
                }
            }

            if ((_windowLocationPane != null) && (SelectedPane != null))
            {
                windowLocation = _windowLocationPane.TrySelectIndicator(cursorPositionOnScreen);
                if ((_insertionIndicatorManager != null) && (_insertionIndicatorManager.ParentGrid != SelectedPane))
                {
                    _insertionIndicatorManager.HideInsertionIndicator();
                    _insertionIndicatorManager = null;
                }
                if (_insertionIndicatorManager == null)
                {
                    _insertionIndicatorManager = new InsertionIndicatorManager(SelectedPane);
                }

                _insertionIndicatorManager.ShowInsertionIndicator(windowLocation);
            }
        }

        #region IUnpinnedToolParent

        void IUnpinnedToolParent.ViewModelRemoved(IViewModel iViewModel)
        {
            System.Diagnostics.Trace.Assert(ToolsSource.Contains(iViewModel));
            
            ToolsSource.Remove(iViewModel);
        }

        IToolListBox IUnpinnedToolParent.GetToolListBox(WindowLocation windowLocation)
        {
            System.Diagnostics.Trace.Assert(_dictToolListBoxes.ContainsKey(windowLocation));

            return _dictToolListBoxes[windowLocation];
        }

        #endregion IUnpinnedToolPaneOwner
    }
}