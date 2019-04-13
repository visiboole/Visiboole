using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisiBoole;

namespace CustomTabControl
{
    public class NewTabControl : TabControl
    {
        /// <summary>
        /// Background color of the component.
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.FromArgb(66, 66, 66);

        /// <summary>
        /// Color of the tab headers.
        /// </summary>
        public Color TabColor { get; set; } = Color.FromArgb(66, 66, 66);

        /// <summary>
        /// Color of the tab header that is currently selected.
        /// </summary>
        public Color SelectedTabColor { get; set; } = SystemColors.Highlight;

        /// <summary>
        /// Color of the outside boundary for the tab header.
        /// </summary>
        public Color TabBoundaryColor { get; set; } = Color.Black;

        /// <summary>
        /// Color of the tab header text.
        /// </summary>
        public Color TabTextColor { get; set; } = Color.White;

        /// <summary>
        /// Color of the selected tab header text.
        /// </summary>
        public Color SelectedTabTextColor { get; set; } = Color.White;

        public NewTabControl() : base()
        {
            AllowDrop = true;
            ShowToolTips = true;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(125, 25);
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);

            MouseDown += MouseDownEvent;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Paint background
            SolidBrush backgroundBrush = new SolidBrush(BackgroundColor);
            pevent.Graphics.FillRectangle(backgroundBrush, 0, 0, Size.Width, Size.Height);
            backgroundBrush.Dispose();

            // Paint each tab
            foreach (TabPage tab in TabPages)
            {
                // Get tab info
                int index = TabPages.IndexOf(tab);
                Rectangle TabBoundary = GetTabRect(index);
                tab.ToolTipText = $"{tab.Text.TrimStart('*')}.vbi";

                // Draw tab
                Color tabColor = index == SelectedIndex ? SelectedTabColor : TabColor;
                SolidBrush tabBrush = new SolidBrush(tabColor);
                pevent.Graphics.FillRectangle(tabBrush, TabBoundary);
                tabBrush.Dispose();

                // Draw tab boundary
                Pen boundaryPen = new Pen(TabBoundaryColor);
                pevent.Graphics.DrawRectangle(boundaryPen, TabBoundary);
                boundaryPen.Dispose();

                // Draw closing X
                pevent.Graphics.DrawImage(VisiBoole.Properties.Resources.Close, TabBoundary.Right - 19, TabBoundary.Height - 18);

                // Draw text
                Rectangle TabTextBoundary = new Rectangle(TabBoundary.X + 2, TabBoundary.Y + 2, TabBoundary.Width - 17, TabBoundary.Height - 2);
                Color fontColor = index == SelectedIndex ? SelectedTabTextColor : TabTextColor;
                TextRenderer.DrawText(pevent.Graphics, tab.Text, Font, TabTextBoundary, fontColor, TextFormatFlags.WordEllipsis);
            }

            pevent.Dispose();
        }

        /// <summary>
        /// Handles the event that occurs when the mouse pointer is over the control and a mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseDownEvent(object sender, MouseEventArgs e)
        {
            // Get clicked tab
            int clickedIndex = GetHoverTabIndex();
            if (clickedIndex >= 0)
            {
                Tag = TabPages[clickedIndex];

                /*
                Rectangle current = GetTabRect(clickedIndex);
                Rectangle close = new Rectangle(current.Right - 18, current.Height - 15, 16, 16);
                if (close.Contains(e.Location))
                {
                    SelectedIndex = SelectedIndex != 0 ? SelectedIndex - 1 : SelectedIndex + 1;
                    TabPages.RemoveAt(clickedIndex);
                }
                */
            }
        }

        /// <summary>
        /// Handles the event that occurs when the mouse pointer is over the control and a mouse button is released.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Clear clicked tag
            Tag = null;
        }

        /// <summary>
        /// Handles the event that occurs when the mouse pointer is moved over the control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((e.Button != MouseButtons.Left) || (Tag == null))
            {
                return;
            }
            
            // Get clicked tab
            TabPage clickedTab = (TabPage)Tag;
            int clickedIndex = TabPages.IndexOf(clickedTab);

            // Start drag and drop
            DoDragDrop(clickedTab, DragDropEffects.All);
        }

        /// <summary>
        /// Handles the event that occurs when an object is dragged over the control's bounds.
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            // Check whether there is no tab being dragged
            if (drgevent.Data.GetData(typeof(TabPage)) == null)
            {
                return;
            }

            // Get drag tab and its index
            TabPage dragTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));
            int dragTabIndex = TabPages.IndexOf(dragTab);

            // Check whether a tab is being dragged
            if (dragTabIndex < 0)
            {
                return;
            }

            // Check whether the mouse if over a tab
            int hoverTabIndex = GetHoverTabIndex();
            if (hoverTabIndex < 0)
            {
                drgevent.Effect = DragDropEffects.None;
                return;
            }
            TabPage hoverTab = TabPages[hoverTabIndex];
            drgevent.Effect = DragDropEffects.Move;

            // Check whether this is the start of the drag
            if (dragTab == hoverTab)
            {
                return;
            }

            // Swap tabs
            Rectangle dragTabRect = GetTabRect(dragTabIndex);
            Rectangle hoverTabRect = GetTabRect(hoverTabIndex);
            if (dragTabRect.Width < hoverTabRect.Width)
            {
                if (dragTabIndex < hoverTabIndex && ((drgevent.X - Location.X) > ((hoverTabRect.X + hoverTabRect.Width) - dragTabRect.Width)))
                {
                    SwapTabPages(dragTab, hoverTab); // Swap tab locations
                }
                else if (dragTabIndex > hoverTabIndex && ((drgevent.X - Location.X) < (hoverTabRect.X + dragTabRect.Width)))
                {
                    SwapTabPages(dragTab, hoverTab); // Swap tab locations
                }
            }
            else
            {
                SwapTabPages(dragTab, hoverTab); // Swap tab locations
            }

            // Select the dragged tab
            SelectedIndex = TabPages.IndexOf(dragTab);
        }

        /// <summary>
        /// Returns the index of the TabPage that the mouse pointer is over.
        /// </summary>
        /// <returns>Index of the TabPage that the mouse pointer is over</returns>
        private int GetHoverTabIndex()
        {
            // Find tab the cursor is over
            for (int i = 0; i < TabPages.Count; i++)
            {
                if (GetTabRect(i).Contains(PointToClient(Cursor.Position)))
                {
                    return i;
                }  
            }

            return -1;
        }

        /// <summary>
        /// Swaps the location of two provided TabPages.
        /// </summary>
        /// <param name="srcTab">TabPage 1</param>
        /// <param name="dstTab">TabPage 2</param>
        private void SwapTabPages(TabPage srcTab, TabPage dstTab)
        {
            // Swap tab indexes
            int srcIndex = TabPages.IndexOf(srcTab);
            int dstIndex = TabPages.IndexOf(dstTab);

            if (TabPages[dstIndex].Design() != null)
            {
                TabPages[dstIndex].Design().TabPageIndex = srcIndex;
            }
            TabPages[dstIndex] = srcTab;

            if (TabPages[srcIndex].Design() != null)
            {
                TabPages[srcIndex].Design().TabPageIndex = dstIndex;
            }
            TabPages[srcIndex] = dstTab;

            Refresh();
        }
    }
}