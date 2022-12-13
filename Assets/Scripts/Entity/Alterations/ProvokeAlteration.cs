using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class ProvokeAlteration : Alteration {
        public ProvokeAlteration(int Cooldown) : base(Cooldown) {
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Provoke;
        }
        public override bool ClassicCountdown => false;

    }
}