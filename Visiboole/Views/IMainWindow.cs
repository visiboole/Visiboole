using VisiBoole.Controllers;

namespace VisiBoole.Views
{
	/// <summary>
	/// Exposes the methods for the MainWindow
	/// </summary>
	public interface IMainWindow
	{
        /// <summary>
        /// Saves the handle to the controller for this view
        /// </summary>
        /// <param name="controller">The handle to the controller for this view</param>
        void AttachController(IMainWindowController controller);

        /// <summary>
        /// Adds a new node in the TreeView
        /// </summary>
        /// <param name="path">The filepath string that will be parsed to obtain the name of this treenode</param>
        void AddNavTreeNode(string path);

        /// <summary>
		/// Removes a node in the TreeView
		/// </summary>
		/// <param name="name">The name of the node to be removed</param>
		void RemoveNavTreeNode(string name);

		/// <summary>
		/// Loads the given IDisplay
		/// </summary>
		/// <param name="previous">The display to replace</param>
		/// <param name="current">The display to be loaded</param>
		void LoadDisplay(IDisplay previous, IDisplay current);

		/// <summary>
		/// Displays file-save success message to the user
		/// </summary>
		/// <param name="fileSaved">True if the file was saved successfully</param>
		void SaveFileSuccess(bool fileSaved);

        /// <summary>
        /// Confrims whether the user wants to close the selected SubDesign
        /// </summary>
        /// <param name="isDirty">True if the SubDesign being closed has been modified since last save</param>
        /// <returns>Whether the selected SubDesign will be closed</returns>
		bool ConfirmClose(bool isDirty);

        /// <summary>
        /// Confirms exit with the user if the application is dirty
        /// </summary>
        /// <param name="isDirty">True if any open SubDesigns have been modified since last save</param>
        void ConfirmExit(bool isDirty);
    }
}