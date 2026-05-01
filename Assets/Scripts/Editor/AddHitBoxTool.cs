#if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AddHitBoxTool : EditorWindow
{
    [MenuItem("Tools/Add Hitbox To Children")]
    
    static void AddHitbox()
    {
        // Hierarchy 오브젝트와 Project 프리팹 둘 다 처리
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            // 프리팹 에셋인 경우
            selected = Selection.activeObject as GameObject;
        }

        if (selected == null)
        {
            Debug.LogWarning("오브젝트를 선택해주세요");
            return;
        }

        Rigidbody[] RigidBodies = selected.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in RigidBodies)
        {
            if (rb.GetComponent<EnemyHitBox>() == null)
            {
                rb.AddComponent<EnemyHitBox>();
            }
        }

        Debug.Log($"{RigidBodies.Length} 개의 컴포넌트 추가가 완료되었습니다.");
    }
}
#endif