using MissionOfMercenary;
using Unity.Cinemachine;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyChase : MonoBehaviour
    {
        //Enemy Detected Player Make A Sound.

        bool _hasSoundTarget = false;
        Vector3 _sondTargetPosition = Vector3.zero; 

        public bool DoFind { get; private set; } = false;
        public bool HasSoundTarget { get { return _hasSoundTarget; } }
        public Vector3 SoundTargetPosition { get { return _sondTargetPosition; } }

        private void Update()
        {
            EnemyHeardSound();
        }

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

        public void SetSoundTarget(Vector3 position)
        {
            _sondTargetPosition = position;
            _hasSoundTarget = true;
            Debug.Log($"HasSoundTarget Value = {HasSoundTarget}");
        }

        public void ClearSoundTarget()
        {
            _sondTargetPosition = Vector3.zero;
            _hasSoundTarget = false;
            Debug.Log($"HasSoundTarget Value = {HasSoundTarget}");
        }
    }
}
