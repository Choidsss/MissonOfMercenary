using MissionOfMercenary;
using Unity.Cinemachine;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyChase : MonoBehaviour
    {
        //Enemy Detected Player Make A Sound.

        bool _hasSoundTarget = false;
        Vector3 _sondTargetPosition; 

        //public bool IsEnemyHeardSound { get; private set; }

        public bool DoFind { get; private set; } = false;
        public bool HasSoundTarget { get { return _hasSoundTarget; } }
        public Vector3 SoundTargetPosition { get { return _sondTargetPosition; } }

        private void Update()
        {
            EnemyHeardSound();
        }

        //총 소리를 들어서 위치정보가 담겼냐 안담겼냐
        public void EnemyHeardSound()
        {
            if (!HasSoundTarget)
            {
                DoFind = false;
                //IsEnemyHeardSound = false;
            }
            else
            {
                DoFind = true;
                //IsEnemyHeardSound = true;
            }
        }

        //GiveToEnemy에서 부르고 입력값을 넣어서 그 위치를 저장하고 Player의 소리를 들었다는 bool변수 관리(이동은 다른코드에서 하며 위치정보를 담는것만 함)
        //DoFind 는 따로 관리함
        public void SetSoundTarget(Vector3 position)
        {
            _sondTargetPosition = position;
            _hasSoundTarget = true;
        }

        public void ClearSoundTarget()
        {
            _sondTargetPosition = Vector3.zero;
            _hasSoundTarget = false;
        }
    }
}
