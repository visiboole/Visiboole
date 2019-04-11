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
        public NewTabControl() : base()
        {
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(125, 23);
            DrawMode = TabDrawMode.OwnerDrawFixed;
            AllowDrop = true;
            ShowToolTips = true;

            MouseDown += MouseDownEvent;
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
            TabPages[dstIndex].Design().TabPageIndex = srcIndex;
            TabPages[dstIndex] = srcTab;
            TabPages[srcIndex].Design().TabPageIndex = dstIndex;
            TabPages[srcIndex] = dstTab;
            Refresh();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            TabPage newTabPage = TabPages[e.Index];
            string tabName = newTabPage.Text;
            newTabPage.ToolTipText = tabName;

            // Draw close
            Rectangle newTabFullRect = GetTabRect(e.Index);
            e.Graphics.DrawImage(VisiBoole.Properties.Resources.Close, newTabFullRect.Right - 18, newTabFullRect.Height - 17);

            // Draw tab text
            Rectangle newTabTextRect = new Rectangle(new Point(newTabFullRect.X + 2, newTabFullRect.Y + 1), new Size(newTabFullRect.Width - 16, newTabFullRect.Height));
            TextRenderer.DrawText(e.Graphics, tabName, new Font("Segoe UI", 11F), newTabTextRect, Color.Black, TextFormatFlags.WordEllipsis);

            // Draw tab background
            Rectangle lastTabRect = GetTabRect(TabPages.Count - 1);
            Rectangle backgroundRect = new Rectangle();
            backgroundRect.Location = new Point(lastTabRect.Right, 0);
            backgroundRect.Size = new Size(Right - backgroundRect.Left, lastTabRect.Height);
            Color backgroundColor = VisiBoole.Properties.Settings.Default.Theme == "Light" ? Color.AliceBlue : Color.FromArgb(66, 66, 66);
            e.Graphics.FillRectangle(new SolidBrush(backgroundColor), backgroundRect);

            base.OnDrawItem(e);
        }
    }
}
