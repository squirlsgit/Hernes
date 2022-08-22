using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Weapons.Guns;
namespace Hernes
{
    public class GunBase : HVRGunBase, IShooter
    {
        [Header("Hernes Gun Base")]
        [SerializeField]
        protected string _Type = "gun";
        public string Type
        {
            get
            {
                return _Type;
            }
        }
        [SerializeField]
        protected GameObject _Agent;
        public GameObject Agent
        {
            get
            {
                return _Agent;
            }
        }
        protected override void OnHit(RaycastHit hit, Vector3 direction)
        {
            base.OnHit(hit, direction);
            hit.collider.GetParentObject().GetComponent<IHittable>()?.OnHit(_Type, _Agent, weapon: gameObject);
        }
    }

}