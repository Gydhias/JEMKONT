using DownBelow.Entity;
using DownBelow.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellResult
    {
        public Dictionary<CharacterEntity, int> DamagesDealt;
        public Dictionary<CharacterEntity, int> DamagesOnShield;
        public int SelfDamagesReceived = 0;
        public int SelfDamagesOverShieldReceived = 0;

        public Dictionary<CharacterEntity, int> HealingDone;
        public int SelfHealingReceived = 0;

        public Dictionary<CharacterEntity, List<BuffType>> BuffGiven;
        public List<BuffType> SelfBuffReceived;
        private CharacterEntity caster;
        private SpellAction action;

        public void Init(List<CharacterEntity> targets, CharacterEntity caster, SpellAction action)
        {
            this.DamagesDealt = new Dictionary<CharacterEntity, int>();
            this.DamagesOnShield = new Dictionary<CharacterEntity, int>();
            this.HealingDone = new Dictionary<CharacterEntity, int>();
            this.BuffGiven = new Dictionary<CharacterEntity, List<BuffType>>();
            this.caster = caster;
            foreach (CharacterEntity entity in targets)
            {
                if (entity == caster)
                    continue;

                entity.OnHealthRemoved += this._updateDamages;
                entity.OnShieldRemoved += this._updateShieldDamages;
                entity.OnHealthAdded += this._updateHealings;

                entity.OnInspirationAdded += this._updateBuffs;
                entity.OnStrengthAdded += this._updateBuffs;
                entity.OnInspirationRemoved += this._updateBuffs;
                entity.OnStrengthRemoved += this._updateBuffs;
            }

            caster.OnHealthRemoved += this._updateSelfDamages;
            caster.OnShieldRemoved += this._updateSelfShieldDamages;
            caster.OnHealthAdded += this._updateSelfHealings;

            caster.OnInspirationAdded += this._updateSelfBuffs;
            caster.OnStrengthAdded += this._updateSelfBuffs;
            caster.OnInspirationRemoved += this._updateSelfBuffs;
            caster.OnStrengthRemoved += this._updateSelfBuffs;
        }

        private void _updateDamages(SpellEventData data)
        {
            if (!this.DamagesDealt.ContainsKey(data.Entity))
                this.DamagesDealt.Add(data.Entity, 0);
            action.FireOnDamageDealt(data);
            this.DamagesDealt[data.Entity] += data.Value;
        }

        private void _updateShieldDamages(SpellEventData data)
        {
            if (!this.DamagesOnShield.ContainsKey(data.Entity))
                this.DamagesOnShield.Add(data.Entity, 0);
            action.FireOnShieldRemoved(data);
            this.DamagesOnShield[data.Entity] += data.Value;
        }

        private void _updateHealings(SpellEventData data)
        {
            if (!this.HealingDone.ContainsKey(data.Entity))
                this.HealingDone.Add(data.Entity, 0);
            action.FireOnHealGiven(data);
            this.HealingDone[data.Entity] += data.Value;
        }

        private void _updateBuffs(SpellEventData data)
        {
            if (!this.BuffGiven.ContainsKey(data.Entity))
                this.BuffGiven.Add(data.Entity, new List<BuffType>());
            action.FireOnBuffGiven(data);
            this.BuffGiven[data.Entity].Add(data.Buff);
        }

        private void _updateSelfDamages(SpellEventData data)
        {
            this.SelfDamagesReceived += data.Value;
            action.FireOnDamageDealt(data);
        }
        private void _updateSelfShieldDamages(SpellEventData data)
        {
            this.SelfDamagesOverShieldReceived += data.Value;
            action.FireOnShieldRemoved(data);
        }
        
        private void _updateSelfHealings(SpellEventData data)
        {
            action.FireOnHealReceived(data);
            this.SelfHealingReceived += data.Value;
        }

        private void _updateSelfBuffs(SpellEventData data)
        {
            if(this.SelfBuffReceived == null)
                this.SelfBuffReceived = new List<BuffType>();
            action.FireOnBuffGiven(data);
            this.SelfBuffReceived.Add(data.Buff);
        }
    }
}
