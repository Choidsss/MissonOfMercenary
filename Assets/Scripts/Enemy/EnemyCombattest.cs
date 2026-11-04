using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyCombattest : MonoBehaviour
    {
        [SerializeField] EnemyCombatController _enemy;
        [SerializeField] Transform _player;
        [SerializeField] Transform _firePosition;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_enemy == null || _player == null || _firePosition == null)
            {
                Debug.LogError(
                    "Enemy, Player, Fire Position을 모두 연결하세요.",
                    this
                );
                return;
            }

            bool initialized = _enemy.Initialize(_player, _firePosition);

            if (!initialized)
            {
                Debug.LogError(
                    "적 전투 초기화 실패: 적의 컴포넌트 연결과 NavMesh 위치를 확인하세요.",
                    _enemy
                );
            }
        }
    }
}
