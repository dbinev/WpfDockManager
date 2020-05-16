﻿namespace WpfDockManagerDemo.DockManager
{
    public interface IViewModel
    {
        // A user friendly title
        string Title { get; set; }

        /*
         * Uniquely identifies a document instance.
         * For example a file path for a text document. 
         */
        string URL { get; set; }

        bool CanClose { get; }
        bool CanFloat { get; }

        /*
         * Return true if there are edits that need to be saved
         * Not used by Tool view model
         */
        bool HasChanged { get; }
        void Save();
        void Close();
    }
}
