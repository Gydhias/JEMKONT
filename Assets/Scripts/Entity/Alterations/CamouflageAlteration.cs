using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class CamouflageAlteration : Alteration {
        public CamouflageAlteration(int Cooldown) : base(Cooldown) {
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Camouflage;
        }
        public override bool ClassicCountdown => false;
    }
}