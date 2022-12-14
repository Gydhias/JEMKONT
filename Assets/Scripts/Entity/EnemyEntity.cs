using EODE.Wonderland;

using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using DownBelow.GridSystem;
using DownBelow.Managers;
using MyBox;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Math = System.Math;


namespace DownBelow.Entity {
    public class EnemyEntity : CharacterEntity 
    {

        public EntityPreset EnemyStyle;
        CharacterEntity cachedAllyToAttack;

        public CharacterEntity CurrentTarget;

        #region Movement
        public enum MovementType {Straight, StraightToRange, Flee, Kite};
        public MovementType movementType = MovementType.Straight;
        private Dictionary<MovementType, System.Action> MovementBehaviors = new Dictionary<MovementType, System.Action>();
        #endregion

        #region Attack
        public enum AttackType { ClosestRandom, FarthestRandom, LowestRandom, HighestRandom, Random};
        public AttackType attackType = AttackType.ClosestRandom;
        private Dictionary<AttackType, System.Action> AttackBehaviors = new Dictionary<AttackType, System.Action>();
        #endregion

        #region Target
        public AttackType TargetType = AttackType.ClosestRandom;
        private Dictionary<AttackType, System.Action> TargetBehaviors = new Dictionary<AttackType, System.Action>();
        #endregion


        public override void Init(EntityStats stats,Cell refCell,WorldGrid refGrid,int order = 0) {
            base.Init(stats,refCell,refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;
            MovementBehaviors.Add(MovementType.Straight, MovementStraight);
            MovementBehaviors.Add(MovementType.StraightToRange, MovementStraightToRange);
            AttackBehaviors.Add(AttackType.ClosestRandom, AttackClosestRandom);
            TargetBehaviors.Add(AttackType.ClosestRandom, TargetClosestRandom);
            TargetBehaviors.Add(AttackType.FarthestRandom, TargetFarthestRandom);
            
        }

        public override void StartTurn()
        {
            CanAutoAttack = !Disarmed;//CanAutoAttack = true if !Disarmed
            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);
            if (Stunned || Sleeping)
            {
                EndTurn();
            }

            TargetBehaviors[TargetType]();
            MovementBehaviors[movementType]();
            AttackBehaviors[attackType]();
            Debug.Log("ENDTURN");
            EndTurn();
            CombatManager.Instance.ProcessEndTurn(this.UID);

            /*
            var TargetPosition = GetTargetPosition();
            Debug.Log("123456 1: " + TargetPosition.latitude +" : " + TargetPosition.longitude);
            GridManager.Instance.FindPath(this,TargetPosition,true);

            Debug.Log("123456 2: " + this.Speed);
            if (GridManager.Instance.Path.Count > 0 && this.Speed > 0) {
                Debug.Log("123456 3: " + this.Speed);
                GridManager.Instance.ShowPossibleCombatMovements(this);
                if (!Confused) NetworkManager.Instance.EntityAsksForPath(this, GridManager.Instance.Path[GridManager.Instance.Path.Count - 1], this.CurrentGrid);
            }
            //Moved (or not if was in range); and will now Autoattack:
            if (CanAutoAttack) {
                if (cachedAllyToAttack != null) {
                    //LETSGOOOOOO FIREEEEEEEEEEEEE
                    AutoAttack(cachedAllyToAttack.EntityCell);
                }
            }*/
            //TODO: ENEMY SPELL?
        }

        // All Movement Behaviours
        #region Movement Behaviours
        /// <summary>
        /// Will Go straight to the target's location
        /// </summary>
        private void MovementStraight()
        {
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            GridManager.Instance.FindPath(this, targPosition);
            if (GridManager.Instance.Path.Count > this.Speed)
            {
                NetworkManager.Instance.EntityAsksForPath(this, GridManager.Instance.Path[this.Speed], this.CurrentGrid);
            }
            NetworkManager.Instance.EntityAsksForPath(this, this.CurrentGrid);
        }

        /// <summary>
        /// Will Go straight to the target stops when in range
        /// </summary>
        private void MovementStraightToRange()
        {
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            List<Cell> Path =  GridManager.Instance.FindPathAndReturn(this, targPosition, false, this.Range);
            if (Path.Count == 0)
            {
                return;
            }
            if (Path.Count > this.Speed)
            {
                NetworkManager.Instance.EntityAsksForPath(this, Path[this.Speed], this.CurrentGrid);
            }
            NetworkManager.Instance.EntityAsksForPath(this, Path[Path.Count - 1], this.CurrentGrid);
        }
        #endregion

        // All Attack Behaviours
        #region Attack Behaviours
        /// <summary>
        /// Will attack the closest player (random btwn two at same range)
        /// </summary>
        private void AttackClosestRandom()
        {
            if (!CanAutoAttack)
            {
                return;
            }
            TargetBehaviors[attackType]();
            if (cachedAllyToAttack != null)
            {
                AutoAttack(cachedAllyToAttack.EntityCell);
            }
        }
        #endregion

        // All Target Behaviours (target is used for the movement)
        #region Target Behaviours
        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private void TargetClosestRandom()
        {
            CharacterEntity[] PlayersByDistance = PlayersOrderedByDistance("Min", out int sameDist);
            var ClosestPlayer = PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];

            CurrentTarget = ClosestPlayer;
        }

        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private void TargetFarthestRandom()
        {
            CharacterEntity[] PlayersByDistance = PlayersOrderedByDistance("Max", out int sameDist);
            var FarthestPlayer = PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];

            CurrentTarget = FarthestPlayer;
        }
        #endregion


        #region Player Sensors
        /// <summary>
        /// Returns Players ordered from closest to farthest / farthest to Closest. If an entity is provoking, clones it to put in index 0 of the array.
        /// Arguments : Type = "Min" or "Max" depending on getting the closest or farthest
        /// </summary>
        public CharacterEntity[] PlayersOrderedByDistance(string type,out int sameDist) {
            var Players = CurrentGrid.GridEntities.FindAll(x => x.IsAlly);
            var Provoking = Players.FindAll(x => x.Provoke);
            int[] distances = new int[Players.Count];
            //Get all distances to player (from path) and set them in distances
            for (int i = 0; i < Players.Count; i++) {
                CharacterEntity item = Players[i];
                GridManager.Instance.FindPath(this,item.EntityCell.PositionInGrid);
                distances[i] = GridManager.Instance.Path.Count;
            }
            CharacterEntity[] orderedAllies = new CharacterEntity[distances.Length];
            int ActualPlayer = 0;
            int TotalOfSameDist = 0;
            bool recordSameDist = true;
            int LastDist = -1;
            int length = distances.Length;
            //for all distances take the smallest 
            for (int i = 0; i < length; i++)
            {
                int Dist = SetPlayerPositionInTargetList(i, type, ref distances, Players, ref orderedAllies, ActualPlayer);
                if (LastDist == Dist && recordSameDist)
                {
                    TotalOfSameDist++;
                }
                else if (i > 0)
                {
                    recordSameDist = false;
                }
                ActualPlayer++;
                LastDist = Dist;
            }
            sameDist = TotalOfSameDist;
            return orderedAllies;
        }

        private int SetPlayerPositionInTargetList(int i, string type, ref int[] distances, List<CharacterEntity> Players, ref CharacterEntity[] orderedAllies, int ActualPlayer)
        {
            int Dist;
            // Depending on the type of distance (close or far) get the good distances
            if (type == "Min")
            {
                Dist = distances.Min();
            }
            else
            {
                Dist = distances.Max();
            }
            int Index = distances.IndexOfItem(Dist);
            //if the player is provoking then make it first priority
            if (Players[Index].Provoke)
            {
                MakeProvokedFirst(Players[Index], ref orderedAllies, Players);
            }
            else
            {
                orderedAllies[ActualPlayer] = Players[Index];
            }
            ArrayUtility.RemoveAt(ref distances, Index);
            return Dist;
        }

        private void MakeProvokedFirst(CharacterEntity provoked, ref CharacterEntity[] array, List<CharacterEntity> players)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (players[i].Provoke)
                {
                    continue;
                }
                ArrayUtility.Insert(ref array, i, provoked);
                break;
            }
        }
        #endregion

        public override Spell AutoAttackSpell() {
            return new Spell(CombatManager.Instance.PossibleAutoAttacks.Find(x => x.Is<DamageStrengthSpellAction>()));

            Cell RandomCellInRange() {
                List<Cell> cells = new();
                foreach (var item in CurrentGrid.Cells) {
                    cells.Add(item);
                }
                return cells.FindAll(x => (Mathf.Abs(x.PositionInGrid.latitude - this.EntityCell.PositionInGrid.latitude) + Mathf.Abs(x.PositionInGrid.longitude - this.EntityCell.PositionInGrid.longitude)) >= Speed).GetRandom();

            }
        }
    }
}