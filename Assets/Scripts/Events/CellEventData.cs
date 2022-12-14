using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class CellEventData : EventData<CellEventData>
    {
        public Cell Cell;
        public bool InCurrentGrid;

        public CellEventData(Cell Cell)
        {
            this.Cell = Cell;
            this.InCurrentGrid = this.Cell.RefGrid == GameManager.Instance.SelfPlayer.CurrentGrid;
        }
    }
}
