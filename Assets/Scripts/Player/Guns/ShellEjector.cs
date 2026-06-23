using UnityEngine;
using System.Collections;

namespace MIssionOfMercenary
{
    public class ShellEjector : MonoBehaviour
    {
        [SerializeField] Transform _cartridgePosition; //탄피를 배출시킬 위치
        [SerializeField] GameObject _cartridge; //탄피 오브젝트
        [SerializeField] float _cartridgeUpForce; //포물선의 윗방향으로 주는 힘
        [SerializeField] float _cartridgeRightForce; //날아가는 방향으로 주는 힘
        [SerializeField] float _lifeTime; //생성시킨 뒤, 다시 Destroy 되는 시간 
        [SerializeField] float _torque; //회전하는 힘

        public void Ejector()
        {
            GameObject go = Instantiate(_cartridge, _cartridgePosition.position, _cartridgePosition.rotation);
            Rigidbody rb = go.GetComponent<Rigidbody>();

            if(rb == null){ Debug.Log("Does Not Exist RigidBody Component.");  return; }

            Vector3 cartridgeDir = Vector3.up * _cartridgeUpForce + Vector3.right;
            rb.AddForce(cartridgeDir.normalized * _cartridgeRightForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere, ForceMode.Impulse);
            Destroy(go, _lifeTime);
        }

        //I Don't Know Math Shit
        //IEnumerator ShellArcRoutine(GameObject obj)
        //{
        //    float elapsedTime = 0.0f; //현재 경과시간
        //    float durationTime = 0.0f; //포물선이 그려지는 시간
        //    Vector3 startPos = obj.transform.position;
        //    Vector3 endPos = startPos + _cartridgePosition.right * 0.3f;

        //    while(elapsedTime < durationTime)
        //    {
        //        elapsedTime += Time.deltaTime;
        //        float t = elapsedTime / durationTime;

        //        Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
        //        currentPos.y = Mathf.Sin(t * Mathf.PI) * 0.2f; // 호 높이???

        //        obj.transform.position = currentPos;
        //        elapsedTime += Time.deltaTime;
        //        yield return null;
        //    }
        //    Destroy(obj, _lifeTime);
        //}
    }
}
