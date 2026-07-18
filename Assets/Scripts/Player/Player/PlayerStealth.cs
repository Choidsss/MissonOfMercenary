using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerStealth : MonoBehaviour
    {
        [SerializeField] AudioSource _gunshotSound;
        [SerializeField] InputReader _inputReader;

        [Header("Player Hide Ops")]
        [SerializeField] float _radius;
        [SerializeField] LayerMask _enemyLayer;

        bool _isHidden = false;
        Vector3 _shotPosition;

        public bool IsHidden { get { return _isHidden; } }

        public Vector3 ShotPosition => _shotPosition;

        //private void OnEnable()
        //{
        //    _inputReader.OnshotEvent += ShotPositionStored;
        //}

        //private void OnDisable()
        //{
        //    _inputReader.OnshotEvent -= ShotPositionStored;
        //}

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            PlayerMakeSound();
        }

        //void ShotPositionStored(float shoot)
        //{
        //    AssultRifle ar = GetComponentInChildren<AssultRifle>();

        //    if(ar == null) { return; }

        //    _shotPosition = ar.gameObject.transform.position;
        //}

        void PlayerDetectEnemyArea()
        {
            Collider[] cds = Physics.OverlapSphere(transform.position, _radius, _enemyLayer);

            if(cds == null) { return; }

            foreach (Collider col in cds)
            {
                GameObject go = col.gameObject;
                EnemyChase ec = go.GetComponent<EnemyChase>();

                if (ec.DoFind)
                {
                    _isHidden = false;
                }
            }
        }

        void PlayerMakeSound()
        {
            AssultRifle ar = GetComponentInChildren<AssultRifle>();
            if(ar == null) { return; }

            if (ar.IsShot)
            {
                //_isHidden의 상태변화 유지 시켜야 함
                _isHidden = false;
                _shotPosition = ar.gameObject.transform.position;
            }


        }
    }
}
