﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Xml;

/*
 * Note: I have placed most of the intelligence in this class rather than spreading 
 * it around multiple classes. 
 * 
 * Units: 
 * 
 * The actual width/height and top/left values for a window etc are defined in DIUs: 
 * 
 *      Device independent unit: 1/96"
 *      
 *  Logical unit = Device independent unit
 */

namespace WpfDockManagerDemo.DockManager
{
    public class LayoutManager : System.Windows.Controls.Grid
    {
        public LayoutManager()
        {
            FloatingPanes = new List<FloatingPane>();
        }

        ~LayoutManager()
        {
            Shutdown();
        }

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

                _edgeLocationPane?.Close();
                _edgeLocationPane = null;
            }
            catch
            {
                // Ignore
            }
        }

        // Call this from the mainwindow when loaded
        public void Initialise()
        {
            //IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            //if (windowHandle == null)
            //{
            //    throw new Exception("LayoutManager.Initialise(): windowHandle is null");
            //}

            //System.Windows.Interop.HwndSource src = System.Windows.Interop.HwndSource.FromHwnd(windowHandle);
            //if (src == null)
            //{
            //    throw new Exception("LayoutManager.Initialise(): src is null");
            //}

            //src.AddHook(WndProc);
        }

        /*
         * Note: 
         * 
         *  The application is responsible for instantiating the view models
         * 
         */

        // The Resources property is in a base class

        /*
         * These can override default appearance
         * They bind to a Title property
         */

        public DataTemplate ToolPaneTitleTemplate { get; set; }
        public DataTemplate ToolPaneHeaderTemplate { get; set; }
        public DataTemplate DocumentPaneTitleTemplate { get; set; }
        public DataTemplate DocumentPaneHeaderTemplate { get; set; }

        public List<FloatingPane> FloatingPanes;

        private ToolPane SelectedPane;

        #region dependency properties 

        #region DocumentsSource dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty DocumentsSourceProperty = DependencyProperty.Register("DocumentsSource", typeof(System.Collections.Generic.IEnumerable<IDocument>), typeof(LayoutManager), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDocumentsSourceChanged)));

        public System.Collections.Generic.IEnumerable<IDocument> DocumentsSource
        {
            get
            {
                return (System.Collections.Generic.IEnumerable<IDocument>)GetValue(DocumentsSourceProperty);
            }
            set
            {
                SetValue(DocumentsSourceProperty, value);
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
                DocumentsSource = (System.Collections.Generic.IEnumerable<IDocument>)e.NewValue;
                Create();
            }
        }

        #endregion

        #region ToolsSource dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty ToolsSourceProperty = DependencyProperty.Register("ToolsSource", typeof(System.Collections.IEnumerable), typeof(LayoutManager), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnToolsSourceChanged)));

        public System.Collections.IEnumerable ToolsSource
        {
            get
            {
                return (System.Collections.IEnumerable)GetValue(ToolsSourceProperty);
            }
            set
            {
                SetValue(ToolsSourceProperty, value);
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
                ToolsSource = (System.Collections.IEnumerable)e.NewValue;
                Create();
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
                Create();
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
                Create();
            }
        }

        #endregion

        #region CloseControlTemplate dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty CloseControlTemplateProperty = DependencyProperty.Register("CloseControlTemplate", typeof(System.Windows.Controls.ControlTemplate), typeof(LayoutManager), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCloseControlTemplateChanged)));

        public System.Windows.Controls.ControlTemplate CloseControlTemplate
        {
            get
            {
                return (System.Windows.Controls.ControlTemplate)GetValue(ToolsSourceProperty);
            }
            set
            {
                SetValue(CloseControlTemplateProperty, value);
            }
        }

        private static void OnCloseControlTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutManager)d).OnCloseControlTemplateChanged(e);
        }

        protected virtual void OnCloseControlTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                CloseControlTemplate = (System.Windows.Controls.ControlTemplate)e.NewValue;
                // Warning warning
                //CreateControl();
            }
        }

        #endregion

        #endregion

        public void Clear()
        {
            Children.Clear();
            while (FloatingPanes.Count > 0)
            {
                FloatingPanes[0].Close();
            }
            FloatingPanes.Clear();
        }

        public List<UserControl> LoadViewsFromTemplates(List<DataTemplate> dataTemplates, System.Collections.IEnumerable viewModels)
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
                                IView iToolView = (view as IView);
                                if (iToolView == null)
                                {
                                    throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": the UserControl must implement interface IView");
                                }
                                iToolView.IDocument = viewModel as IDocument;
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

        private delegate DockPane DelegateCreateDockPane();

        private void AddViews(List<UserControl> views, List<FrameworkElement> list_N, DelegateCreateDockPane createDockPane)
        {
            List<FrameworkElement> list_N_plus_1 = new List<FrameworkElement>();
            bool isHorizontal = false;
            int viewIndex = 1;

            while (viewIndex < views.Count)
            {
                for (int i = 0; (i < list_N.Count) && (viewIndex < views.Count); ++i)
                {
                    SplitterPane splitterPane = new SplitterPane(isHorizontal);

                    var node = list_N[i];
                    System.Windows.Markup.IAddChild parentElement = (System.Windows.Markup.IAddChild)node.Parent;
                    (node.Parent as Grid).Children.Remove(node);

                    parentElement.AddChild(splitterPane);
                    Grid.SetRow(splitterPane, Grid.GetRow(node));
                    Grid.SetColumn(splitterPane, Grid.GetColumn(node));

                    splitterPane.AddChild(node, true);

                    list_N_plus_1.Add(node);

                    node = views[viewIndex];
                    DockManager.DockPane dockPane = createDockPane();
                    dockPane.IDocumentContainer.AddUserControl(node as UserControl);

                    list_N_plus_1.Add(dockPane);

                    splitterPane.AddChild(dockPane, false);

                    ++viewIndex;
                }

                isHorizontal = !isHorizontal;
                list_N = list_N_plus_1;
                list_N_plus_1 = new List<FrameworkElement>();
            }
        }

        private void Create()
        {
            Clear();
            
            /*
             * We descend the tree level by level, adding a new level when the current one is full. 
             * We continue adding nodes until we have exhausted the items in views (created above). 
             * 
                    Parent              Level Index
                       G                    1   
                 /           \              
                 G           G              2 
              /     \     /     \           
              D     D     D     D           3
              
                Assume we are building level N where there are potentially 2 ^ (N-1) nodes (denoted by a star). 
                Maintain two node lists. One for level N and one for level N + 1. 
                List for level N is denoted list(N). 
                Assume level N nodes are complete. 
                Then for each item in list(N) we add two child nodes, and then add these child nodes to list (N+1). 

                First level: 

                       D1

                Add a node -> replace D1 with a grid containing two documents and a splitter: 

                       G
                 /           \              
                 D1          D2              

                Add a node -> replace D1 with a grid containing two documents and a splitter: 

                       G
                 /           \              
                 G           D2
              /     \     
              D1    D3     

                Add a node -> replace D2 with a grid containing two documents and a splitter: 

                       G
                 /           \              
                 G           G
              /     \     /    \
              D1    D3    D2    D4

                and so on ... 

                Document panes are children of a dock panel. At first this is a child of the top level 
                splitter pane, or the layout manager if there are no tool panes

             */

            SplitterPane rootSplitterPane = new SplitterPane(true);
            Children.Add(rootSplitterPane);

            DocumentPanel documentPanel = new DocumentPanel();
            rootSplitterPane.AddChild(documentPanel, true);

            List<UserControl> documentViews = LoadViewsFromTemplates(DocumentTemplates, DocumentsSource);

            if ((documentViews != null) && (documentViews.Count > 0))
            {
                List<FrameworkElement> list_N = new List<FrameworkElement>();

                DockManager.DockPane documentPane = CreateDocumentPane();
                documentPane.IDocumentContainer.AddUserControl(documentViews[0]);

                documentPanel.Children.Add(documentPane);
                list_N.Add(documentPane);
                AddViews(documentViews, list_N, delegate { return CreateDocumentPane(); });
            }

            List<UserControl> toolViews = LoadViewsFromTemplates(ToolTemplates, ToolsSource);
            if ((toolViews != null) && (toolViews.Count > 0))
            {
                List<FrameworkElement> list_N = new List<FrameworkElement>();

                DockManager.DockPane toolPane = CreateToolPane();
                toolPane.IDocumentContainer.AddUserControl(toolViews[0]);

                rootSplitterPane.AddChild(toolPane, false);

                list_N.Add(toolPane);
                AddViews(toolViews, list_N, delegate { return CreateToolPane(); });
            }

            UpdateLayout();
        }

        private XmlElement SaveToolGroupNode(XmlDocument xmlDocument, XmlNode xmlNode, ToolPane toolPane)
        {
            if ((xmlDocument == null) || (xmlNode == null))
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": invalid arguments");
            }

            XmlElement xmlToolGroup = xmlDocument.CreateElement("ToolGroup");

            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("row");
            xmlAttribute.Value = Grid.GetRow(toolPane).ToString();
            xmlToolGroup.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("column");
            xmlAttribute.Value = Grid.GetColumn(toolPane).ToString();
            xmlToolGroup.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("width");
            xmlAttribute.Value = toolPane.ActualWidth.ToString();
            xmlToolGroup.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("height");
            xmlAttribute.Value = toolPane.ActualHeight.ToString();
            xmlToolGroup.Attributes.Append(xmlAttribute);

            xmlNode.AppendChild(xmlToolGroup);
            return xmlToolGroup;
        }

        private XmlElement SaveToolNode(XmlDocument xmlDocument, XmlNode xmlNode, IDocument iDocument, string contentId)
        {
            if ((xmlDocument == null) || (xmlNode == null) || (iDocument == null))
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": invalid arguments");
            }

            XmlElement xmlElement = xmlDocument.CreateElement("Tool");
            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("Title");
            xmlAttribute.Value = iDocument.Title;
            xmlElement.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("ID");
            xmlAttribute.Value = iDocument.ID.ToString();
            xmlElement.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("ContentId");
            xmlAttribute.Value = contentId;
            xmlElement.Attributes.Append(xmlAttribute);

            xmlNode.AppendChild(xmlElement);
            return xmlElement;
        }

        private void SaveSplitterPaneNode(XmlDocument xmlDocument, Grid splitterPane, XmlNode xmlParentNode)
        {
            System.Diagnostics.Trace.Assert(xmlDocument != null);
            System.Diagnostics.Trace.Assert(splitterPane != null);
            System.Diagnostics.Trace.Assert(xmlParentNode != null);

            XmlElement xmlSplitterPane = xmlDocument.CreateElement("SplitterPane");
            xmlParentNode.AppendChild(xmlSplitterPane);

            GridSplitter gridSplitter = splitterPane.Children.OfType<GridSplitter>().Single();
            System.Diagnostics.Trace.Assert(gridSplitter != null);

            bool isHorizontal = Grid.GetRow(gridSplitter) == 1;
            var xmlAttribute = xmlDocument.CreateAttribute("Orientation");
            xmlAttribute.Value = isHorizontal ? "Horizontal" : "Vertical";
            xmlSplitterPane.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("row");
            xmlAttribute.Value = Grid.GetRow(splitterPane).ToString();
            xmlSplitterPane.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("column");
            xmlAttribute.Value = Grid.GetColumn(splitterPane).ToString();
            xmlSplitterPane.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("width");
            xmlAttribute.Value = splitterPane.ActualWidth.ToString();
            xmlSplitterPane.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("height");
            xmlAttribute.Value = splitterPane.ActualHeight.ToString();
            xmlSplitterPane.Attributes.Append(xmlAttribute);

            List<FrameworkElement> children = splitterPane.Children.OfType<FrameworkElement>().Where(n => !(n is GridSplitter)).OrderBy(n => Grid.GetRow(n) + Grid.GetColumn(n)).ToList();
            foreach (var childNode in children)
            {
                SaveNode(xmlDocument, childNode, xmlSplitterPane);
            }
        }

        private XmlElement SaveDocumentGroupNode(XmlDocument xmlDocument, XmlNode xmlNode, DocumentPane documentPane)
        {
            if ((xmlDocument == null) || (xmlNode == null))
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": invalid arguments");
            }

            XmlElement xmlDocumentGroup = xmlDocument.CreateElement("DocumentGroup");

            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("row");
            xmlAttribute.Value = Grid.GetRow(documentPane).ToString();
            xmlDocumentGroup.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("column");
            xmlAttribute.Value = Grid.GetColumn(documentPane).ToString();
            xmlDocumentGroup.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("width");
            xmlAttribute.Value = documentPane.ActualWidth.ToString();
            xmlDocumentGroup.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("height");
            xmlAttribute.Value = documentPane.ActualHeight.ToString();
            xmlDocumentGroup.Attributes.Append(xmlAttribute);

            xmlNode.AppendChild(xmlDocumentGroup);
            return xmlDocumentGroup;
        }

        private XmlElement SaveDocumentNode(XmlDocument xmlDocument, XmlNode xmlNode, IDocument iDocument, string contentId)
        {
            if ((xmlDocument == null) || (xmlNode == null) || (iDocument == null))
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": invalid arguments");
            }

            XmlElement xmlElement = xmlDocument.CreateElement("Document");
            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("Title");
            xmlAttribute.Value = iDocument.Title;
            xmlElement.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("ID");
            xmlAttribute.Value = iDocument.ID.ToString();
            xmlElement.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("ContentId");
            xmlAttribute.Value = contentId;
            xmlElement.Attributes.Append(xmlAttribute);

            xmlNode.AppendChild(xmlElement);
            return xmlElement;
        }

        private void SaveDocumentPanelNode(XmlDocument xmlDocument, DocumentPanel documentPanel, XmlNode xmlParentNode)
        {
            System.Diagnostics.Trace.Assert(xmlDocument != null);
            System.Diagnostics.Trace.Assert(documentPanel != null);
            System.Diagnostics.Trace.Assert(xmlParentNode != null);

            XmlElement xmlDocumentPanel = xmlDocument.CreateElement("DocumentPanel");
            xmlParentNode.AppendChild(xmlDocumentPanel);

            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("row");
            xmlAttribute.Value = Grid.GetRow(documentPanel).ToString();
            xmlDocumentPanel.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("column");
            xmlAttribute.Value = Grid.GetColumn(documentPanel).ToString();
            xmlDocumentPanel.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("width");
            xmlAttribute.Value = documentPanel.ActualWidth.ToString();
            xmlDocumentPanel.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("height");
            xmlAttribute.Value = documentPanel.ActualHeight.ToString();
            xmlDocumentPanel.Attributes.Append(xmlAttribute);

            foreach (var childNode in documentPanel.Children)
            {
                SaveNode(xmlDocument, childNode, xmlDocumentPanel);
            }
        }

        private XmlElement CreateFloatingPaneNode(XmlDocument xmlDocument, XmlNode xmlParentNode, FloatingPane floatingPane)
        {
            if ((xmlDocument == null) || (xmlParentNode == null))
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": invalid arguments");
            }

            XmlElement xmlFloatingPane = xmlDocument.CreateElement("FloatingPane");

            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("width");
            xmlAttribute.Value = floatingPane.ActualWidth.ToString();
            xmlFloatingPane.Attributes.Append(xmlAttribute);

            xmlAttribute = xmlDocument.CreateAttribute("height");
            xmlAttribute.Value = floatingPane.ActualHeight.ToString();
            xmlFloatingPane.Attributes.Append(xmlAttribute);

            xmlParentNode.AppendChild(xmlFloatingPane);
            return xmlFloatingPane;
        }

        private void SaveNode(XmlDocument xmlDocument, Object node, XmlNode xmlParentPane)
        {
            System.Diagnostics.Trace.Assert(xmlDocument != null);
            System.Diagnostics.Trace.Assert(node != null);
            System.Diagnostics.Trace.Assert(xmlParentPane != null);

            if (node is DocumentPanel)
            {
                SaveDocumentPanelNode(xmlDocument, node as DocumentPanel, xmlParentPane);
            }
            else if (node is DocumentPane)
            {
                DocumentPane documentPane = node as DocumentPane;
                int count = documentPane.IDocumentContainer.GetUserControlCount();
                if (count < 1)
                {
                    throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": no documents");
                }

                XmlNode xmlNodeParent = SaveDocumentGroupNode(xmlDocument, xmlParentPane, documentPane);

                for (int index = 0; index < count; ++index)
                {
                    UserControl userControl = documentPane.IDocumentContainer.GetUserControl(index);
                    if (userControl == null)
                    {
                        break;
                    }
                    SaveDocumentNode(xmlDocument, xmlNodeParent, userControl.DataContext as IDocument, userControl.Name);
                }
            }
            else if (node is ToolPane)
            {
                ToolPane toolPane = node as ToolPane;
                int count = toolPane.IDocumentContainer.GetUserControlCount();
                if (count < 1)
                {
                    throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": no tools");
                }

                XmlNode xmlNodeParent = SaveToolGroupNode(xmlDocument, xmlParentPane, toolPane);

                for (int index = 0; index < count; ++index)
                {
                    UserControl userControl = toolPane.IDocumentContainer.GetUserControl(index);
                    if (userControl == null)
                    {
                        break;
                    }
                    SaveToolNode(xmlDocument, xmlNodeParent, userControl.DataContext as IDocument, userControl.Name);
                }
            }
            else if (node is Grid)
            {
                SaveSplitterPaneNode(xmlDocument, node as Grid, xmlParentPane);
            }
        }

        public bool SaveLayout(out XmlDocument xmlDocument, string fileNameAndPath)
        {
            xmlDocument = new XmlDocument();

            if (Children.Count == 0)
            {
                return false;
            }

            XmlElement xmlLayoutManager = xmlDocument.CreateElement("LayoutManager");
            xmlDocument.AppendChild(xmlLayoutManager);

            SaveNode(xmlDocument, Children[0], xmlLayoutManager);
            foreach (FloatingPane floatingPane in FloatingPanes)
            {
                int count = floatingPane.IDocumentContainer.GetUserControlCount();
                if (count < 1)
                {
                    throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": no documents");
                }

                XmlElement xmlNodeParent = CreateFloatingPaneNode(xmlDocument, xmlLayoutManager, floatingPane);

                for (int index = 0; index < count; ++index)
                {
                    UserControl userControl = floatingPane.IDocumentContainer.GetUserControl(index);
                    if (userControl == null)
                    {
                        break;
                    }
                    SaveToolNode(xmlDocument, xmlNodeParent, userControl.DataContext as IDocument, userControl.Name);
                }
            }
            xmlDocument.Save(fileNameAndPath);

            return true;
        }

        private void SetWidthOrHeight(XmlElement xmlElement, FrameworkElement parentFrameworkElement, bool isParentHorizontal, int row, int column)
        {
            XmlAttribute xmlAttribute = xmlElement.Attributes.GetNamedItem("width") as XmlAttribute;
            if (xmlAttribute == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": unable to load layout: a " + xmlElement.Name + " element must have a width attribute");
            }
            double width = System.Convert.ToDouble(xmlAttribute.Value);

            xmlAttribute = xmlElement.Attributes.GetNamedItem("height") as XmlAttribute;
            if (xmlAttribute == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": unable to load layout: a " + xmlElement.Name + " element must have a height attribute");
            }
            double height = System.Convert.ToDouble(xmlAttribute.Value);

            Grid grid = parentFrameworkElement as Grid;
            if (grid != null)
            {
                if (isParentHorizontal)
                {
                    if (row < grid.RowDefinitions.Count)
                    {
                        grid.RowDefinitions[row].Height = new GridLength(height, GridUnitType.Star);
                    }
                }
                else
                {
                    if (column < grid.ColumnDefinitions.Count)
                    {
                        grid.ColumnDefinitions[column].Width = new GridLength(width, GridUnitType.Star);
                    }
                }
            }
        }

        private void LoadToolGroup(Dictionary<string, UserControl> viewsMap, XmlElement xmlToolGroup, IDocumentContainer iDocumentContainer)
        {
            foreach (var xmlToolGroupNode in xmlToolGroup.ChildNodes)
            {
                if (xmlToolGroupNode is XmlElement)
                {
                    if ((xmlToolGroupNode as XmlElement).Name == "Tool")
                    {
                        XmlElement xmlToolElement = xmlToolGroupNode as XmlElement;

                        XmlAttribute xmlAttribute = xmlToolElement.Attributes.GetNamedItem("ContentId") as XmlAttribute;
                        if (xmlAttribute == null)
                        {
                            throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": a Tool element must have an ID attribute");
                        }
                        
                        if (viewsMap.ContainsKey(xmlAttribute.Value))
                        {
                            iDocumentContainer.AddUserControl(viewsMap[xmlAttribute.Value]);
                            viewsMap.Remove(xmlAttribute.Value);
                        }
                    }
                }
            }
        }

        private void LoadDocumentGroup(Dictionary<string, UserControl> viewsMap, XmlElement xmlDocumentGroup, IDocumentContainer iDocumentContainer)
        {
            foreach (var xmlDocumentGroupNode in xmlDocumentGroup.ChildNodes)
            {
                if (xmlDocumentGroupNode is XmlElement)
                {
                    if ((xmlDocumentGroupNode as XmlElement).Name == "Document")
                    {
                        XmlElement xmlToolElement = xmlDocumentGroupNode as XmlElement;

                        XmlAttribute xmlAttribute = xmlToolElement.Attributes.GetNamedItem("ContentId") as XmlAttribute;
                        if (xmlAttribute == null)
                        {
                            throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": a Document element must have an ID attribute");
                        }

                        if (viewsMap.ContainsKey(xmlAttribute.Value))
                        {
                            iDocumentContainer.AddUserControl(viewsMap[xmlAttribute.Value]);
                            viewsMap.Remove(xmlAttribute.Value);
                        }
                    }
                }
            }
        }

        private void LoadNode(Dictionary<string, UserControl> viewsMap, FrameworkElement parentFrameworkElement, XmlNode parentXmlNode, bool isParentHorizontal)
        {
            int row = 0;
            int rowIncrement = isParentHorizontal ? 2 : 0;
            int column = 0;
            int columnIncrement = isParentHorizontal ? 0 : 2;

            foreach (var xmlChildNode in parentXmlNode.ChildNodes)
            {
                if (xmlChildNode is XmlElement)
                {
                    if ((xmlChildNode as XmlElement).Name == "SplitterPane")
                    {
                        XmlElement xmlSplitterPane = xmlChildNode as XmlElement;

                        XmlAttribute xmlAttribute = xmlSplitterPane.Attributes.GetNamedItem("Orientation") as XmlAttribute;
                        if (xmlAttribute == null)
                        {
                            throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": a SplitterPane element must have an orientation attribute");
                        }

                        SetWidthOrHeight(xmlSplitterPane, parentFrameworkElement, isParentHorizontal, row, column);

                        bool isChildHorizontal = xmlAttribute.Value == "Horizontal";

                        Grid newGrid = new SplitterPane(isChildHorizontal);
                        Grid.SetRow(newGrid, row);
                        Grid.SetColumn(newGrid, column);
                        row += rowIncrement;
                        column += columnIncrement;

                        System.Windows.Markup.IAddChild parentElement = (System.Windows.Markup.IAddChild)parentFrameworkElement;
                        parentElement.AddChild(newGrid);

                        LoadNode(viewsMap, newGrid, xmlSplitterPane, isChildHorizontal);
                    }
                    else if ((xmlChildNode as XmlElement).Name == "DocumentPanel")
                    {
                        DocumentPanel documentPanel = new DocumentPanel();

                        System.Windows.Markup.IAddChild parentElement = (System.Windows.Markup.IAddChild)parentFrameworkElement;
                        parentElement.AddChild(documentPanel);

                        XmlElement xmlDocumentPanel = xmlChildNode as XmlElement;

                        SetWidthOrHeight(xmlDocumentPanel, parentFrameworkElement, isParentHorizontal, row, column);

                        LoadNode(viewsMap, documentPanel, xmlDocumentPanel, true);

                        Grid.SetRow(documentPanel, row);
                        Grid.SetColumn(documentPanel, column);
                        row += rowIncrement;
                        column += columnIncrement;
                    }
                    else if ((xmlChildNode as XmlElement).Name == "DocumentGroup")
                    {
                        DocumentPane documentPane = CreateDocumentPane();

                        System.Windows.Markup.IAddChild parentElement = (System.Windows.Markup.IAddChild)parentFrameworkElement;
                        parentElement.AddChild(documentPane);

                        XmlElement xmlDocumentGroup = xmlChildNode as XmlElement;

                        SetWidthOrHeight(xmlDocumentGroup, parentFrameworkElement, isParentHorizontal, row, column);

                        LoadDocumentGroup(viewsMap, xmlDocumentGroup, documentPane.IDocumentContainer);
                        Grid.SetRow(documentPane, row);
                        Grid.SetColumn(documentPane, column);
                        row += rowIncrement;
                        column += columnIncrement;
                    }
                    else if ((xmlChildNode as XmlElement).Name == "ToolGroup")
                    {
                        ToolPane toolPane = CreateToolPane();

                        System.Windows.Markup.IAddChild parentElement = (System.Windows.Markup.IAddChild)parentFrameworkElement;
                        parentElement.AddChild(toolPane);

                        XmlElement xmlToolGroup = xmlChildNode as XmlElement;

                        SetWidthOrHeight(xmlToolGroup, parentFrameworkElement, isParentHorizontal, row, column);

                        LoadToolGroup(viewsMap, xmlToolGroup, toolPane.IDocumentContainer);
                        Grid.SetRow(toolPane, row);
                        Grid.SetColumn(toolPane, column);
                        row += rowIncrement;
                        column += columnIncrement;
                    }
                    else if ((xmlChildNode as XmlElement).Name == "FloatingPane")
                    {
                        FloatingPane floatingPane = CreateFloatingPane();

                        XmlElement xmlfloatingPane = xmlChildNode as XmlElement;
                        LoadToolGroup(viewsMap, xmlfloatingPane, floatingPane.IDocumentContainer);
                    }
                }
                
                if ((row > 2) || (column > 2))
                {
                    // we can only have two child elements (plus a splitter) in each grid
                    break;
                }
            }
        }

        public bool LoadLayout(out XmlDocument xmlDocument, string fileNameAndPath)
        {
            Clear();

            xmlDocument = new XmlDocument();
            xmlDocument.Load(fileNameAndPath);

            if (xmlDocument.ChildNodes.Count == 0)
            {
                return false;
            }

            List<UserControl> views = new List<UserControl>();
            Dictionary<string, UserControl> viewsMap = new Dictionary<string, UserControl>();

            List<UserControl> documentViews = LoadViewsFromTemplates(DocumentTemplates, DocumentsSource);
            foreach (var item in documentViews)
            {
                viewsMap.Add(item.Name, item);
            }

            List<UserControl> toolViews = LoadViewsFromTemplates(ToolTemplates, ToolsSource);
            foreach (var item in toolViews)
            {
                viewsMap.Add(item.Name, item);
            }

            // Now load the views into the dock manager => one or more views might not be visible!

            LoadNode(viewsMap, this, xmlDocument.DocumentElement, true);

            return true;
        }

        /*
         * Remove a document pane from the document tree
         */
        private ToolPane ExtractToolPane(ToolPane toolPane)
        {
            if (toolPane == null)
            {
                return null;
            }

            Grid parentGrid = toolPane.Parent as Grid;
            if (parentGrid == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": toolPane parent must be a Grid");
            }

            if (parentGrid == this)
            {
                this.Children.Remove(toolPane);
            }
            else
            {
                if (parentGrid.Parent is FloatingPane)
                {
                    return null;
                }

                Grid grandparentGrid = parentGrid.Parent as Grid;
                if (grandparentGrid == null)
                {
                    throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": Grid parent not a Grid");
                }
                parentGrid.Children.Remove(toolPane);

                FrameworkElement frameworkElement = null;
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
                grandparentGrid.Children.Remove(parentGrid);
                Grid.SetRow(frameworkElement, Grid.GetRow(parentGrid));
                Grid.SetColumn(frameworkElement, Grid.GetColumn(parentGrid));
                grandparentGrid.Children.Add(frameworkElement);
            }

            return toolPane;
        }

        private ToolPane CreateToolPane()
        {
            ToolPane toolPane = new ToolPane();
            toolPane.Close += ToolPane_Close;
            toolPane.Float += ToolPane_Float;
            toolPane.UngroupCurrent += ToolPane_UngroupCurrent;
            toolPane.Ungroup += ToolPane_Ungroup;
            return toolPane;
        }

        private DocumentPane CreateDocumentPane()
        {
            DocumentPane documentPane = new DocumentPane();
            documentPane.Close += ToolPane_Close;
            documentPane.Float += ToolPane_Float;
            documentPane.UngroupCurrent += ToolPane_UngroupCurrent;
            documentPane.Ungroup += ToolPane_Ungroup;
            return documentPane;
        }

        private bool UngroupToolPane(ToolPane toolPane, int index)
        {
            if (toolPane == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": toolPane is null");
            }

            int viewCount = toolPane.IDocumentContainer.GetUserControlCount();
            if (viewCount < 2)
            {
                // Cannot ungroup one item!
                return false;
            }

            // The parent must be a SplitterPane or the LayoutManager
            Grid parentSplitterPane = toolPane.Parent as Grid;
            if (parentSplitterPane == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": toolPane.Parent not a Grid");
            }

            UserControl userControl = toolPane.IDocumentContainer.ExtractUserControl(index);
            if (userControl == null)
            {
                return false;
            }

            ToolPane newDocumentPane = CreateToolPane();
            newDocumentPane.IDocumentContainer.AddUserControl(userControl);

            parentSplitterPane.Children.Remove(toolPane);

            SplitterPane newGrid = new SplitterPane(false);
            parentSplitterPane.Children.Add(newGrid);
            Grid.SetRow(newGrid, Grid.GetRow(toolPane));
            Grid.SetColumn(newGrid, Grid.GetColumn(toolPane));

            newGrid.AddChild(toolPane, true);
            newGrid.AddChild(newDocumentPane, false);

            return true;
        }

        private void ToolPane_Ungroup(object sender, EventArgs e)
        {
            ToolPane toolPane = sender as ToolPane;

            while (UngroupToolPane(toolPane, 1))
            {
                // Nothing here
            }
        }

        private void ToolPane_UngroupCurrent(object sender, EventArgs e)
        {
            ToolPane toolPane = sender as ToolPane;
            if (toolPane == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": sender not a ToolPane");
            }

            int index = toolPane.IDocumentContainer.GetCurrentTabIndex();
            if (index > -1)
            {
                UngroupToolPane(toolPane, index);
            }
        }

        private void FloatingPane_Closed(object sender, EventArgs e)
        {
            if (FloatingPanes.Contains(sender as FloatingPane))
            {
                FloatingPanes.Remove(sender as FloatingPane);
            }
        }

        private void FloatingPane_Ungroup(object sender, EventArgs e)
        {
            FloatingPane floatingPane = sender as FloatingPane;
            if (floatingPane == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": sender not a FloatingPane");
            }

            int viewCount = floatingPane.IDocumentContainer.GetUserControlCount();

            double left = floatingPane.Left;
            double top = floatingPane.Top;

            for (int index = 1; index < viewCount; ++index)
            {
                UserControl userControl = floatingPane.IDocumentContainer.ExtractUserControl(1);
                if (userControl == null)
                {
                    return;
                }

                FloatingPane newfloatingPane = CreateFloatingPane();
                // Warning warning -> check window is visible
                left += 10;
                top += 10;
                newfloatingPane.Left = left;
                newfloatingPane.Top = top;
                newfloatingPane.IDocumentContainer.AddUserControl(userControl);
            }
        }

        private void FloatingPane_UngroupCurrent(object sender, EventArgs e)
        {
            FloatingPane floatingPane = sender as FloatingPane;
            if (floatingPane == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": sender not a FloatingPane");
            }

            int index = floatingPane.IDocumentContainer.GetCurrentTabIndex();
            if (index > -1)
            {
                UserControl userControl = floatingPane.IDocumentContainer.ExtractUserControl(1);
                if (userControl == null)
                {
                    return;
                }

                FloatingPane newfloatingPane = CreateFloatingPane();
                newfloatingPane.Left = floatingPane.Left + 10;
                newfloatingPane.Top = floatingPane.Top + 10;
                newfloatingPane.IDocumentContainer.AddUserControl(userControl);
            }
        }

        private FloatingPane CreateFloatingPane()
        {
            FloatingPane floatingPane = new FloatingPane();
            floatingPane.LocationChanged += FloatingWindow_LocationChanged;
            floatingPane.Closed += FloatingPane_Closed;
            floatingPane.Ungroup += FloatingPane_Ungroup;
            floatingPane.UngroupCurrent += FloatingPane_UngroupCurrent;
            floatingPane.DataContext = new FloatingViewModel();
            (floatingPane.DataContext as FloatingViewModel).Title = floatingPane.Title;
            floatingPane.EndDrag += FloatingView_EndDrag;
            // Ensure the window remains on top of the main window
            floatingPane.Owner = App.Current.MainWindow;
            floatingPane.Show();
            FloatingPanes.Add(floatingPane);
            return floatingPane;
        }

        private void ToolPane_Float(object sender, EventArgs e)
        {
            System.Windows.Input.MouseEventArgs mouseEventArgs = e as System.Windows.Input.MouseEventArgs;

            ToolPane toolPane = sender as ToolPane;

            if (toolPane == null)
            {
                return;
            }

            Point mousePosition;
            if (mouseEventArgs != null)
            {
                mousePosition = mouseEventArgs.GetPosition(App.Current.MainWindow);
            }
            else
            {
                mousePosition = new Point(App.Current.MainWindow.Left + App.Current.MainWindow.Width/2, App.Current.MainWindow.Top + App.Current.MainWindow.Height / 2);
            }

            Point mainWindowLocation = App.Current.MainWindow.PointToScreen(new Point(0, 0));

            ExtractToolPane(toolPane);

            FloatingPane floatingPane = CreateFloatingPane();

            while (true)
            {
                UserControl userControl = toolPane.IDocumentContainer.ExtractUserControl(0);
                if (userControl == null)
                {
                    break;
                }

                floatingPane.IDocumentContainer.AddUserControl(userControl);
            }

            floatingPane.Left = mainWindowLocation.X + mousePosition.X;
            floatingPane.Top = mainWindowLocation.Y + mousePosition.Y;
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
            _edgeLocationPane?.Close();
            _edgeLocationPane = null;
        }

        private void ExtractDocuments(FloatingPane floatingPane, ToolPane toolPane)
        {
            while (true)
            {
                UserControl userControl = floatingPane.IDocumentContainer.ExtractUserControl(0);
                if (userControl == null)
                {
                    break;
                }

                toolPane.IDocumentContainer.AddUserControl(userControl);
            }
            floatingPane.Close();
        }

        private void FloatingView_EndDrag(object sender, EventArgs e)
        {
            FloatingPane floatingPane = sender as FloatingPane;

            App.Current.Dispatcher.Invoke(delegate
            {
                if (SelectedPane == null)
                {
                    return;
                }

                if (
                        (floatingPane == null) ||
                        (SelectedPane == null) ||
                        (!(SelectedPane.Parent is SplitterPane) && (SelectedPane.Parent != this)) ||
                        (_windowLocationPane == null) ||
                        (_insertionIndicatorManager == null) ||
                        (_insertionIndicatorManager.WindowLocation == WindowLocation.None)
                   )
                {
                    return;
                }

                SplitterPane parentSplitterPane = (SelectedPane.Parent as SplitterPane);
                ToolPane toolPane = null;
                ToolPane selectedPane = SelectedPane;
                WindowLocation windowLocation = _insertionIndicatorManager.WindowLocation;
                CancelSelection();

                switch (windowLocation)
                {
                    case WindowLocation.BottomEdge:
                    case WindowLocation.TopEdge:
                    case WindowLocation.LeftEdge:
                    case WindowLocation.RightEdge:

                        toolPane = CreateToolPane();
                        ExtractDocuments(floatingPane, toolPane);

                        parentSplitterPane = new SplitterPane((windowLocation == WindowLocation.TopEdge) || (windowLocation == WindowLocation.BottomEdge));
                        bool isFirst = (windowLocation == WindowLocation.TopEdge) || (windowLocation == WindowLocation.LeftEdge);
                        parentSplitterPane.AddChild(toolPane, isFirst);

                        if (Children.Count == 0)
                        {
                            Children.Add(parentSplitterPane);
                        }
                        else
                        {
                            SplitterPane rootSplitterPane = Children[0] as SplitterPane;
                            Children.Remove(rootSplitterPane);
                            Children.Add(parentSplitterPane);
                            parentSplitterPane.AddChild(rootSplitterPane, !isFirst);
                        }
                        break;

                    case WindowLocation.Right:
                    case WindowLocation.Left:
                    case WindowLocation.Top:
                    case WindowLocation.Bottom:

                        toolPane = CreateToolPane();
                        ExtractDocuments(floatingPane, toolPane);

                        parentSplitterPane.Children.Remove(selectedPane);

                        SplitterPane newGrid = new SplitterPane((windowLocation == WindowLocation.Top) || (windowLocation == WindowLocation.Bottom));
                        parentSplitterPane.Children.Add(newGrid);
                        Grid.SetRow(newGrid, Grid.GetRow(selectedPane));
                        Grid.SetColumn(newGrid, Grid.GetColumn(selectedPane));

                        bool isTargetFirst = (windowLocation == WindowLocation.Right) || (windowLocation == WindowLocation.Bottom);
                        newGrid.AddChild(selectedPane, isTargetFirst);
                        newGrid.AddChild(toolPane, !isTargetFirst);
                        break;

                    case WindowLocation.Middle:

                        ExtractDocuments(floatingPane, selectedPane as ToolPane);
                        break;
                }

                App.Current.MainWindow.Activate();
            });
        }

        /*
         * Locates the dock pane at the specified on screen point
         */
        private ToolPane FindView(Grid grid, Point pointOnScreen)
        {
            if (grid == null)
            {
                return null;
            }

            foreach (var child in grid.Children)
            {
                if ((child is ToolPane) || (child is SplitterPane))
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
                        if (child is ToolPane)
                        {
                            return child as ToolPane;
                        }

                        return FindView(childGrid, pointOnScreen);
                    }
                }
            }

            return null;
        }

        // The WPF method does not work properly -> call into User32.dll
        Point GetCursorPosition()
        {
            if (User32.GetCursorPos(out User32.POINT point) == false)
            {
                return new Point(0, 0);
            }
            return new Point(point.X, point.Y);
        }

        private WindowLocationPane _windowLocationPane;
        private EdgeLocationPane _edgeLocationPane;
        private InsertionIndicatorManager _insertionIndicatorManager;

        private void FloatingWindow_LocationChanged(object sender, EventArgs e)
        {
            Window floatingWindow = sender as Window;
            if (floatingWindow == null)
            {
                throw new Exception(System.Reflection.MethodBase.GetCurrentMethod().Name + ": null floating window");
            }

            Point cursorPositionOnScreen = GetCursorPosition();

            bool found = false;
            Point cursorPositionInMainWindow = this.PointFromScreen(cursorPositionOnScreen);
            if (
                    (cursorPositionInMainWindow.X >= 0) &&
                    (cursorPositionInMainWindow.X <= this.ActualWidth) && 
                    (cursorPositionInMainWindow.Y >= 0) &&
                    (cursorPositionInMainWindow.Y <= this.ActualHeight) 
                )
            { 
                var pane = FindView(this, cursorPositionOnScreen);
                found = pane != null;
                if ((pane != null) && (SelectedPane != pane))
                {
                    Point pointInGrid = this.PointFromScreen(pane.PointToScreen(new Point(0, 0)));

                    pane.IsHighlighted = true;
                    SelectedPane = pane;
                    if (_windowLocationPane != null)
                    {
                        _windowLocationPane.Close();
                    }
                    
                    _windowLocationPane = new WindowLocationPane();
                    _windowLocationPane.AllowsTransparency = true;
                    _windowLocationPane.Show();
                    // The constants are frigs .. 
                    _windowLocationPane.Left = App.Current.MainWindow.Left + pointInGrid.X + SelectedPane.ActualWidth / 2 - _windowLocationPane.ActualWidth / 2 + 7;
                    _windowLocationPane.Top = App.Current.MainWindow.Top + pointInGrid.Y + SelectedPane.ActualHeight / 2 - _windowLocationPane.ActualHeight / 2 + 2;
                    _windowLocationPane.Owner = floatingWindow.Owner;

                    if (_edgeLocationPane != null)
                    {
                        _edgeLocationPane.Close();
                    }
                    _edgeLocationPane = new EdgeLocationPane();
                    _edgeLocationPane.AllowsTransparency = true;
                    _edgeLocationPane.Show();
                    Point topLeftPoint = this.PointToScreen(new Point(0, 0));
                    _edgeLocationPane.Left = topLeftPoint.X;
                    _edgeLocationPane.Top = topLeftPoint.Y;
                    _edgeLocationPane.Width = this.ActualWidth;
                    _edgeLocationPane.Height = this.ActualHeight;
                }
            }

            ToolPane previousPane = SelectedPane;

            if (!found)
            {
                SelectedPane = null;
            }

            if (!found)
            {
                if (_windowLocationPane != null)
                {
                    _windowLocationPane.Close();
                    _windowLocationPane = null;
                }
                if (_edgeLocationPane != null)
                {
                    _edgeLocationPane.Close();
                    _edgeLocationPane = null;
                }
            }

            if ((previousPane != null) && (SelectedPane != previousPane))
            {
                previousPane.IsHighlighted = false;
            }

            bool insertionFound = false;
            if (_edgeLocationPane != null)
            {
                WindowLocation windowLocation = _edgeLocationPane.TrySelectIndicator(cursorPositionOnScreen);
                switch (windowLocation)
                {
                    case WindowLocation.LeftEdge:
                    case WindowLocation.RightEdge:
                    case WindowLocation.TopEdge:
                    case WindowLocation.BottomEdge:
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
                        insertionFound = true;
                        break;
                }
            }

            if ((_windowLocationPane != null) && !insertionFound)
            {
                WindowLocation windowLocation = _windowLocationPane.TrySelectIndicator(cursorPositionOnScreen);
                if (SelectedPane != null)
                {
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
        }

        private void ToolPane_Close(object sender, EventArgs e)
        {
            ToolPane toolPane = sender as ToolPane;

            if (toolPane == null)
            {
                return;
            }

            ExtractToolPane(toolPane);
         }
    }
}
