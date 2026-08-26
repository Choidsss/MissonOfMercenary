# WeaponPickupColliderSettingEditor 코드

실제 적용 위치:

`Assets/Scripts/Editor/WeaponPickupColliderSettingEditor.cs`

기존 내용을 전부 지우고 아래 코드로 교체하면 됩니다.

```csharp
using UnityEditor;
using UnityEngine;

namespace MIssionOfMercenary
{
    [CustomEditor(typeof(WeaponPickupColliderSetting))]
    public class WeaponPickupColliderSettingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Pickup Collider"))
            {
                GeneratePickupCollider(
                    (WeaponPickupColliderSetting)target
                );
            }
        }

        private static void GeneratePickupCollider(
            WeaponPickupColliderSetting setting)
        {
            Transform weaponRoot = setting.transform;

            Renderer[] renderers =
                weaponRoot.GetComponentsInChildren<Renderer>(true);

            if (!TryCalculateLocalBounds(
                    weaponRoot,
                    renderers,
                    out Bounds bounds))
            {
                Debug.LogWarning(
                    "총기 하위에서 크기를 계산할 Renderer를 찾지 못했습니다.",
                    setting
                );

                return;
            }

            Transform triggerTransform =
                weaponRoot.Find(setting.TriggerObjectName);

            if (triggerTransform == null)
            {
                GameObject triggerObject =
                    new GameObject(setting.TriggerObjectName);

                Undo.RegisterCreatedObjectUndo(
                    triggerObject,
                    "Create Pickup Trigger"
                );

                triggerTransform = triggerObject.transform;
                triggerTransform.SetParent(weaponRoot, false);
            }

            Undo.RecordObject(
                triggerTransform,
                "Reset Pickup Trigger Transform"
            );

            triggerTransform.localPosition = Vector3.zero;
            triggerTransform.localRotation = Quaternion.identity;
            triggerTransform.localScale = Vector3.one;

            BoxCollider boxCollider =
                triggerTransform.GetComponent<BoxCollider>();

            if (boxCollider == null)
            {
                boxCollider = Undo.AddComponent<BoxCollider>(
                    triggerTransform.gameObject
                );
            }
            else
            {
                Undo.RecordObject(
                    boxCollider,
                    "Update Pickup Collider"
                );
            }

            float padding = Mathf.Max(0f, setting.Padding);

            boxCollider.center = bounds.center;
            boxCollider.size =
                bounds.size + Vector3.one * padding * 2f;
            boxCollider.isTrigger = true;
            boxCollider.enabled = true;

            EditorUtility.SetDirty(triggerTransform);
            EditorUtility.SetDirty(boxCollider);
            EditorUtility.SetDirty(setting);

            Debug.Log(
                $"Pickup Collider 생성 완료: {weaponRoot.name}",
                setting
            );
        }

        private static bool TryCalculateLocalBounds(
            Transform weaponRoot,
            Renderer[] renderers,
            out Bounds result)
        {
            result = default;
            bool initialized = false;

            foreach (Renderer renderer in renderers)
            {
                // 총구 화염이나 궤적 같은 이펙트는 총기 크기에서 제외합니다.
                if (renderer is ParticleSystemRenderer ||
                    renderer is TrailRenderer ||
                    renderer is LineRenderer)
                {
                    continue;
                }

                Bounds rendererBounds = renderer.localBounds;
                Vector3 min = rendererBounds.min;
                Vector3 max = rendererBounds.max;

                // Renderer Bounds의 모서리 8개를 총기 루트 좌표로 변환합니다.
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            Vector3 rendererLocalCorner = new Vector3(
                                x == 0 ? min.x : max.x,
                                y == 0 ? min.y : max.y,
                                z == 0 ? min.z : max.z
                            );

                            Vector3 worldCorner =
                                renderer.transform.TransformPoint(
                                    rendererLocalCorner
                                );

                            Vector3 weaponLocalCorner =
                                weaponRoot.InverseTransformPoint(
                                    worldCorner
                                );

                            if (!initialized)
                            {
                                result = new Bounds(
                                    weaponLocalCorner,
                                    Vector3.zero
                                );

                                initialized = true;
                            }
                            else
                            {
                                result.Encapsulate(weaponLocalCorner);
                            }
                        }
                    }
                }
            }

            return initialized;
        }
    }
}
```

## 사용 순서

1. 위 코드를 `WeaponPickupColliderSettingEditor.cs`에 적용합니다.
2. Unity의 컴파일이 끝날 때까지 기다립니다.
3. 총기 프리팹의 루트에 `WeaponPickupColliderSetting`을 추가합니다.
4. Inspector에서 `Generate Pickup Collider` 버튼을 누릅니다.
5. 루트 아래에 생성된 `PickupTrigger/BoxCollider`를 확인합니다.
6. 필요하면 `Padding`을 변경하고 버튼을 다시 누릅니다.

버튼을 다시 눌러도 오브젝트를 중복 생성하지 않고 기존 콜라이더를 갱신합니다.

