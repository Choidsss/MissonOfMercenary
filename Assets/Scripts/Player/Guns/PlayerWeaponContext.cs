using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerWeaponContext : MonoBehaviour
    {
        [Header("Shared Transforms")]
        [SerializeField] Transform _weaponPivot;

        [Header("Player Controllers")]
        [SerializeField] WeaponIKController _weaponIKController;
        [SerializeField] TryGetAimHit _aimHit;

        [Header("Shared Effect Pools")]
        [SerializeField] PlayerBulletTrailPooling _bulletTrailPooling;
        [SerializeField] BulletMarkPooling _bulletMarkPooling;

        public Transform WeaponPivot { get { return _weaponPivot; } }
        public WeaponIKController WeaponIKController { get { return _weaponIKController; } }
        public TryGetAimHit AimHit { get { return _aimHit; } }
        public PlayerBulletTrailPooling BulletTrailPooling { get { return _bulletTrailPooling; } }
        public BulletMarkPooling BulletMarkPooling { get { return _bulletMarkPooling; } }

    }
}
