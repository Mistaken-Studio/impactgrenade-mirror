// -----------------------------------------------------------------------
// <copyright file="FlashImpHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Grenades;
using Mistaken.API.Diagnostics;
using Mistaken.RoundLogger;
using UnityEngine;

namespace Mistaken.ImpactGrenade
{
    /// <summary>
    /// Flash grenade exploding on impact.
    /// </summary>
    public class FlashImpHandler : Module
    {
        /// <inheritdoc cref="Module.Module(IPlugin{IConfig})"/>
        public FlashImpHandler(IPlugin<IConfig> plugin)
            : base(plugin)
        {
        }

        /// <inheritdoc/>
        public override string Name => "FlashImpHandler";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ThrowingGrenade += this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => this.Player_ThrowingGrenade(ev));
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ThrowingGrenade -= this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => this.Player_ThrowingGrenade(ev));
        }

        private void Player_ThrowingGrenade(ThrowingGrenadeEventArgs ev)
        {
            if (ev.Type == Exiled.API.Enums.GrenadeType.Flashbang && ev.Player.CurrentItem.durability != 149000f)
            {
                this.CallDelayed(
                    1f,
                    () =>
                    {
                        if (ev.Player.GetEffectActive<CustomPlayerEffects.Scp268>())
                            ev.Player.DisableEffect<CustomPlayerEffects.Scp268>();
                        RLogger.Log("IMPACT FLASH", "THROW", $"{ev.Player.PlayerToString()} threw an impact flash");
                        Grenade grenade = UnityEngine.Object.Instantiate(ev.Player.GrenadeManager.availableGrenades[1].grenadeInstance).GetComponent<Grenade>();
                        grenade.fuseDuration = 999;
                        grenade.InitData(ev.Player.GrenadeManager, Vector3.zero, ev.Player.CameraTransform.forward, ev.IsSlow ? 0.5f : 1f);
                        Mirror.NetworkServer.Spawn(grenade.gameObject);
                        grenade.GetComponent<Rigidbody>().AddForce(new Vector3(grenade.NetworkserverVelocities.linear.x * 1.5f, grenade.NetworkserverVelocities.linear.y / 2f, grenade.NetworkserverVelocities.linear.z * 1.5f), ForceMode.VelocityChange);
                        ev.Player.RemoveItem(ev.Player.CurrentItem);
                        grenade.gameObject.AddComponent<ImpComponent>();
                    },
                    "ThrowingGrenade");
                ev.IsAllowed = false;
            }
        }
    }
}
