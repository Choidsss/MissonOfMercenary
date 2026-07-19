using UnityEngine;

namespace MIssionOfMercenary
{
    public class SoundPositionGiveToEnemy : MonoBehaviour
    {
        AssultRifle _ar;

        [Header("OverlapSphere Size Setting")]
        [SerializeField] float _radius;
        [SerializeField] LayerMask _enemyLayer;

        Vector3 _soundPosition;

        public Vector3 SoundPosition => _soundPosition;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _ar = GetComponent<AssultRifle>();
        }

        public void PositionGiveToEnemy()
        {
            if (_ar == null) { return; }
            if (!_ar.IsShot) { return; }

            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _enemyLayer, QueryTriggerInteraction.Collide);
            _soundPosition = _ar.transform.position;

            //foreach (Collider col in cols)
            //{
            //    col.ga
            //}

            //여기서는 오버랩스피어로 주변에 있는 적들을 알아낸다음, 그 적들한테 내가 총을 쏜 위치를 프로퍼티로 알려줌
            //적 한테는 빈 벡터3 포지션을 넣어두고, 이 자료형에 저 위치정보를 전달함
            //적의 입장에서는 평소에는 돌아다니다가 이 위치에 정보가 담길 경우, 이 위치를 우선적으로 타겟삼아 이동하도록 만들면 될듯
        }
    }
}
