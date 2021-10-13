// -----------------------------------------------------------------------
// <copyright file="ImpItem.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using Mistaken.RoundLogger;
using UnityEngine;

namespace Mistaken.ImpactGrenade
{
    /// <summary>
    /// Grenade that explodes on impact.
    /// </summary>
    public class ImpItem : CustomGrenade
    {
        /*
        /// <inheritdoc/>
        public override SessionVarType SessionVarType => SessionVarType.CI_IMPACT;

        /// <inheritdoc/>
        public override Upgrade[] Upgrades => new Upgrade[]
        {
            new Upgrade
            {
                Chance = 100,
                Durability = null,
                Input = ItemType.GrenadeFrag,
                KnobSetting = Scp914.Scp914Knob.Fine,
            },
        };*/

        /// <inheritdoc/>
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;

        /// <inheritdoc/>
        public override bool ExplodeOnCollision { get; set; }

        /// <inheritdoc/>
        public override float FuseTime { get; set; } = 3;

        /// <inheritdoc/>
        public override uint Id { get; set; } = 9;

        /// <inheritdoc/>
        public override string Name { get; set; } = "Impact Grenade";

        /// <inheritdoc/>
        public override string Description { get; set; } = "Grenade that explodes on impact";

        /// <inheritdoc/>
        public override float Weight { get; set; } = 0.01f;

        /// <inheritdoc/>
        public override SpawnProperties SpawnProperties { get; set; }

        /// <inheritdoc/>
        public override Pickup Spawn(Vector3 position)
        {
            var pickup = base.Spawn(position);
            pickup.Scale = ImpHandler.Size;
            TimedGrenadePickup grenade = (TimedGrenadePickup)pickup.Base;
            if (grenade != null)
                grenade.Info.Serial = pickup.Serial;

            return pickup;
        }

        /// <inheritdoc/>
        public override Pickup Spawn(Vector3 position, Item item)
        {
            var pickup = base.Spawn(position, item);
            pickup.Scale = ImpHandler.Size;
            TimedGrenadePickup grenade = (TimedGrenadePickup)pickup.Base;
            if (grenade != null)
                grenade.Info.Serial = pickup.Serial;

            return pickup;
        }

        /// <inheritdoc/>
        protected override void ShowPickedUpMessage(Player player)
        {
            RLogger.Log("IMPACT GRENADE", "PICKUP", $"{player.PlayerToString()} Picked up an impact grenade");
        }

        /// <inheritdoc/>
        protected override void OnThrowing(ThrowingItemEventArgs ev)
        {
            if (ev.RequestType != ThrowRequest.BeginThrow)
            {
                RLogger.Log("IMPACT GRENADE", "THROW", $"{ev.Player.PlayerToString()} threw an impact grenade");
                ServerThrowPatch.ThrowedItems.Add(ev.Item.Base);
                ev.Player.RemoveItem(ev.Item);
            }
        }

        /// <inheritdoc/>
        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            RLogger.Log("IMPACT GRENADE", "EXPLODED", $"Impact grenade exploded");
            foreach (var player in ev.TargetsToAffect)
                RLogger.Log("IMPACT GRENADE", "HURT", $"{player.PlayerToString()} was hurt by an impact grenade");
        }

        /// <inheritdoc/>
        protected override void ShowSelectedMessage(Player player)
        {
            ImpHandler.Instance.RunCoroutine(this.UpdateInterface(player));
        }

        private IEnumerator<float> UpdateInterface(Player player)
        {
            yield return Timing.WaitForSeconds(0.1f);
            while (this.Check(player.CurrentItem))
            {
                player.SetGUI("impact", PseudoGUIPosition.BOTTOM, "Trzymasz <color=yellow>Granat Uderzeniowy</color>");
                yield return Timing.WaitForSeconds(1);
            }

            player.SetGUI("impact", PseudoGUIPosition.BOTTOM, null);
        }
    }
}
