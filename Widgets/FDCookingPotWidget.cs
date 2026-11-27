using System.Xml.Linq;
using Engine;

namespace Game {
    public class FDCookingPotWidget : CanvasWidget {
        readonly ComponentFDCookingPot m_pot;
        readonly GridPanelWidget m_inventoryGrid;
        readonly GridPanelWidget m_potGrid;
        readonly InventorySlotWidget m_resultSlot;
        readonly InventorySlotWidget m_containerSlot;
        readonly ValueBarWidget m_progress;

        public FDCookingPotWidget(IInventory inventory, ComponentFDCookingPot pot) {
            m_pot = pot;
            LoadContents(this, ContentManager.Get<XElement>("Widgets/FDCookingPotWidget"));
            m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
            m_potGrid = Children.Find<GridPanelWidget>("PotGrid");
            m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
            m_containerSlot = Children.Find<InventorySlotWidget>("ContainerSlot");
            m_progress = Children.Find<ValueBarWidget>("Progress");

            for (int i = 0; i < 6; i++) {
                InventorySlotWidget slot = new();
                slot.AssignInventorySlot(pot, i);
                m_potGrid.Children.Add(slot);
                m_potGrid.SetWidgetCell(slot, new Point2(i % 3, i / 3));
            }
            m_resultSlot.AssignInventorySlot(pot, ComponentFDCookingPot.ResultSlot);
            m_containerSlot.AssignInventorySlot(pot, ComponentFDCookingPot.ContainerSlot);

            int index = 10;
            for (int row = 0; row < m_inventoryGrid.RowsCount; row++) {
                for (int col = 0; col < m_inventoryGrid.ColumnsCount; col++) {
                    InventorySlotWidget slot = new();
                    slot.AssignInventorySlot(inventory, index++);
                    m_inventoryGrid.Children.Add(slot);
                    m_inventoryGrid.SetWidgetCell(slot, new Point2(col, row));
                }
            }
        }

        public override void Update() {
            m_progress.Value = m_pot.Progress;
            if (!m_pot.IsAddedToProject) {
                ParentWidget.Children.Remove(this);
            }
        }
    }
}
