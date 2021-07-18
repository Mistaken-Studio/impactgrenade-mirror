// -----------------------------------------------------------------------
// <copyright file="ImpComponent.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Grenades;
using UnityEngine;

namespace Mistaken.ImpactGrenade
{
    /// <summary>
    /// Handles explosion on impact.
    /// </summary>
    public class ImpComponent : MonoBehaviour
    {
        private bool used;

        private void OnCollisionEnter(Collision collision)
        {
            if (!this.used && this.TryGetComponent<FragGrenade>(out FragGrenade frag))
                frag.NetworkfuseTime = 0.01f;
            else if (!this.used && this.TryGetComponent<FlashGrenade>(out FlashGrenade flash))
                flash.NetworkfuseTime = 0.01f;
            this.used = true;
        }
    }
}
