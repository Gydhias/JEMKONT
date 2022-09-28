using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Player
{
    public class PlayerBehavior : MonoBehaviour
    {
        public MeshRenderer PlayerBody;

        public GridPosition PlayerPosition = GridPosition.zero;
        public static int MovementPoints = 5;

        public void Init(GridPosition startingPosition, Vector3 worldPosition)
        {
            this.PlayerPosition = startingPosition;
            this.gameObject.transform.position = new Vector3(worldPosition.x, 0f, worldPosition.z);
        }

        public void MovePlayer()
        {

        }
    }
}