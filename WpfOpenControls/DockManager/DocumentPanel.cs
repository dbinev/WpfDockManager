﻿using System.Windows.Controls;

namespace WpfOpenControls.DockManager
{
    internal class DocumentPanel : SelectablePane
    {
        public DocumentPanel()
        {
            IsHighlighted = false;
        }

        private bool _isHighlighted;
        public override bool IsHighlighted 
        { 
            get
            {
                return _isHighlighted;
            }
            set
            {
                _isHighlighted = value;
                if (value)
                {
                    Background = System.Windows.Media.Brushes.Firebrick;
                }
                else
                {
                    Background = System.Windows.Media.Brushes.White;
                }
            }
        }

        private bool ContainsDocuments(Grid grid)
        {
            if (grid == null)
            {
                return false;
            }

            foreach (var child in grid.Children)
            {
                if (child is DockPane)
                {
                    return true;
                }
                if (child is Grid)
                {
                    if (ContainsDocuments(child as Grid))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsDocuments()
        {
            return ContainsDocuments(this);
        }
    }
}
