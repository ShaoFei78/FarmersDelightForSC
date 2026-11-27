using System.Xml.Linq;
using Engine;

namespace Game {
    /// <summary>9x3 storage UI for FD cabinets and baskets (27 slots).</summary>
    public class FDStorageWidget : CanvasWidget {
        readonly ComponentChest m_chest;
        readonly GridPanelWidget m_chestGrid;
        readonly GridPanelWidget m_inventoryGrid;

        public FDStorageWidget(IInventory inventory, ComponentChest chest) {
            m_chest = chest;
            LoadContents(this, ContentManager.Get<XElement>("Widgets/FDStorageWidget"));
            m_chestGrid = Children.Find<GridPanelWidget>("ChestGrid");
            m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");

            int slot = 0;
            for (int row = 0; row < m_chestGrid.RowsCount; row++) {
                for (int col = 0; col < m_chestGrid.ColumnsCount; col++) {
                    InventorySlotWidget widget = new();
                    widget.AssignInventorySlot(chest, slot++);
                    m_chestGrid.Children.Add(widget);
                    m_chestGrid.SetWidgetCell(widget, new Point2(col, row));
                }
            }

            int index = 10;
            for (int row = 0; row < m_inventoryGrid.RowsCount; row++) {
                for (int col = 0; col < m_inventoryGrid.ColumnsCount; col++) {
                    InventorySlotWidget widget = new();
                    widget.AssignInventorySlot(inventory, index++);
                    m_inventoryGrid.Children.Add(widget);
                    m_inventoryGrid.SetWidgetCell(widget, new Point2(col, row));
                }
            }
        }

        public override void Update() {
            if (!m_chest.IsAddedToProject) {
                ParentWidget.Children.Remove(this);
            }
        }
    }
}
