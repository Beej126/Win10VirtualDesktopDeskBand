using CSDeskBand;
using CSDeskBand.ContextMenu;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace Win10VirtualDesktopDeskBand
{
    [ComVisible(true)]
    [Guid("AA01ACB3-6CCC-497C-9CE6-9211F2EDFC10")]
    [CSDeskBandRegistration(Name = "Virtual Desktop Widget")]
    public class Deskband : CSDeskBandWpf
    {
        public Deskband()
        {
            Options.ContextMenuItems = ContextMenuItems;
        }

        protected override UIElement UIElement => new DeskBandUserControl();

        private List<DeskBandMenuItem> ContextMenuItems
        {
            get
            {
                var action = new DeskBandMenuAction("Action - Toggle submenu");
                var separator = new DeskBandMenuSeparator();
                var submenuAction = new DeskBandMenuAction("Submenu Action - Toggle checkmark");
                var submenu = new DeskBandMenu("Submenu")
                {
                    Items = { submenuAction }
                };

                action.Clicked += (sender, args) => submenu.Enabled = !submenu.Enabled;
                submenuAction.Clicked += (sender, args) => submenuAction.Checked = !submenuAction.Checked;

                return new List<DeskBandMenuItem>() { action, separator, submenu };
            }
        }
    }
}
