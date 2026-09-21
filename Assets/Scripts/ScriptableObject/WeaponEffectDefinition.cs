using UnityEngine;

namespace MIssionOfMercenary
{
    [CreateAssetMenu(menuName = "Weapons/WeaponEffectDefinition")]
    public class WeaponEffectDefinition : ScriptableObject
    {
        //[Header("Muzzle Transform")]
        //[SerializeField] Transform _muzzle;

        [Header("Muzzle Flash")]
        [SerializeField] GameObject _muzzleFlash;

        [Header("Bullet Mark")]
        [SerializeField] GameObject _mark;

        [Header("Bullet")]
        [SerializeField] GameObject _bullet;

        [Header("Bullet Trail")]
        [SerializeField] GameObject _trail;

        //public Transform Muzzle => _muzzle;
        public GameObject MuzzleFlash => _muzzleFlash;
        public GameObject Mark => _mark;
        public GameObject Bullet => _bullet;
        public GameObject Trail => _trail;
    }
}
