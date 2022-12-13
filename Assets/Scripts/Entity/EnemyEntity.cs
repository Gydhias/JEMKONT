using Jemkont.GridSystem;
using Jemkont.Managers;
using MyBox;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Jemkont.Entity
{
    public class EnemyEntity : CharacterEntity
    {
        public EntityPreset EnemyStyle;

        public override void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0)
        {
            base.Init(stats, refCell, refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;
        }

        public override void StartTurn() {
            this.ReinitializeStat(EntityStatistics.Movement);
            this.ReinitializeStat(EntityStatistics.Mana);
            var TargetPosition = GetTargetPosition();

            GridManager.Instance.FindPath(this,TargetPosition, true);

            if(GridManager.Instance.Path.Count > 0 && this.Movement > 0)
            {
                string mainGrid = this.CurrentGrid is CombatGrid cGrid ? cGrid.ParentGrid.UName : CurrentGrid.UName;
                string innerGrid = mainGrid == this.CurrentGrid.UName ? string.Empty : this.CurrentGrid.UName;

                NetworkManager.Instance.EntityAsksForPath(this.UID, GridManager.Instance.Path[GridManager.Instance.Path.Count - 1], mainGrid, innerGrid);
            }
            //TODO: ENEMY SPELL
            //OPTIONAL TODO : TAKE INTO ACCOUNT ENEMY SPELL RANGE TO NOT MOVE HIM IF HES IN RANGE
        }
        /// <summary>
        /// gets the position the enemy should go, depending on it's AI.
        /// </summary>
        public GridPosition GetTargetPosition() {
            return AlliesOrdered()[0].EntityCell.PositionInGrid;
        }
        /// <summary>
        /// Returns entities ordered from closest to farthest
        /// </summary>
        public CharacterEntity[] AlliesOrdered() {
            var Allies = CurrentGrid.GridEntities.FindAll(x => x.IsAlly);
            int[] distances = new int[Allies.Count];
            for (int i = 0;i < Allies.Count;i++) {
                CharacterEntity item = Allies[i];
                GridManager.Instance.FindPath(this,item.EntityCell.PositionInGrid,true);
                distances[i] = GridManager.Instance.Path.Count;
            }
            CharacterEntity[] orderedAllies = new CharacterEntity[distances.Length];
            int ActualPlayer = 0;
            while (distances.Length > 0) {
                int minIndex = distances.IndexOfItem(distances.Min());
                orderedAllies[ActualPlayer] = Allies[minIndex];
                distances = distances.RemoveAt(minIndex);
            }
            //Distances[i] = x tel que x = la distance entre l'enemy et le players[i]. trouver i => Distance[i] est le plus petit de tous, return players[i]
            return orderedAllies;
        }
    }
}