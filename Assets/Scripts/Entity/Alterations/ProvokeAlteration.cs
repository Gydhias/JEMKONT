using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class ProvokeAlteration : Alteration {
        public ProvokeAlteration(int Cooldown) : base(Cooldown) {
        }
        public override bool ClassicCountdown => false;

        public override List<Type> Overridden() {
            return null;
        }

        public override List<Type> Overrides() {
            return new List<Type>() {
                typeof(ConfusionAlteration)
            };
        }
    }
}