using DownBelow.Entity;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    [Serializable]
    public class Spell
    {
        public SpellCondition ConditionData;
        public SpellAction ActionData;

        public bool ApplyToCell = true;
        public bool ApplyToSelf = false;
        [EnableIf("@!this.ApplyToEnemies && !this.ApplyToAll")]
        public bool ApplyToAllies = false;
        [EnableIf("@!this.ApplyToAllies && !this.ApplyToAll")]
        public bool ApplyToEnemies = false;
        [EnableIf("@!this.ApplyToEnemies && !this.ApplyToAllies")]
        public bool ApplyToAll = false;

        // The instantiated spell
        [HideInInspector]
        public SpellAction CurrentAction;
        [HideInInspector]
        public CharacterEntity Caster;

        public Spell(SpellAction ActionData,SpellCondition ConditionData = null,bool ApplyToCell = true, bool ApplyToSelf = false,bool ApplyToAllies=false, bool ApplyToAll=false) {
            this.ActionData = ActionData;
            this.ConditionData = ConditionData;
            this.ApplyToCell = ApplyToCell;
            this.ApplyToSelf = ApplyToSelf;
            this.ApplyToAllies = ApplyToAllies;
            this.ApplyToAll = ApplyToAll;
        }
        public void ExecuteSpell(CharacterEntity caster, GridSystem.Cell cellTarget)
        {
            this.Caster = caster;

            this.CurrentAction = UnityEngine.Object.Instantiate(this.ActionData, Vector3.zero, Quaternion.identity, CombatManager.Instance.CurrentPlayingEntity.gameObject.transform);
            this.CurrentAction.Execute(this.GetTargets(caster, cellTarget), this);            
        }

        public List<CharacterEntity> GetTargets(CharacterEntity caster, GridSystem.Cell cellTarget)
        {
            List<CharacterEntity> targets = new List<CharacterEntity>();
            //TODO: If getTarget doesn't get a target in a cell, cancel the spell?
            if (this.ApplyToCell && cellTarget.EntityIn != null && !cellTarget.EntityIn.Camouflage)
                targets.Add(cellTarget.EntityIn);

            if (this.ApplyToSelf && !targets.Contains(caster))
                targets.Add(caster);

            if (this.ApplyToAllies)
            {
                foreach (CharacterEntity item in CombatManager.Instance.PlayingEntities)
                {
                    if(item.IsAlly && !targets.Contains(item))
                        targets.Add(item);
                }
            }

            if (this.ApplyToEnemies)
            {
                foreach (CharacterEntity item in CombatManager.Instance.PlayingEntities)
                {
                    if (!item.IsAlly && !targets.Contains(item))
                        targets.Add(item);
                }
            }

            if(this.ApplyToAll)
            {
                foreach (CharacterEntity item in CombatManager.Instance.PlayingEntities)
                {
                    if (!targets.Contains(item))
                        targets.Add(item);
                }
            }

            return targets;
        }
    }
}