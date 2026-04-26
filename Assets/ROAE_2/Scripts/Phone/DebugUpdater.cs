using UnityEngine;
using TMPro;

public class TMPDebugger : MonoBehaviour
{
    void Start()
    {
        TMP_SubMeshUI[] allSubMeshes = FindObjectsOfType<TMP_SubMeshUI>(true);

        foreach (var subMesh in allSubMeshes)
        {
            if (subMesh.textComponent == null)
            {
                Debug.LogError("❌ TMP_SubMeshUI fără textComponent: " + subMesh.name, subMesh.gameObject);
            }
            else if (subMesh.textComponent.fontSharedMaterial == null)
            {
                Debug.LogError("❌ TMP_SubMeshUI fără fontSharedMaterial: " + subMesh.name, subMesh.gameObject);
            }
        }

        Debug.Log($"✅ Verificate {allSubMeshes.Length} submesh-uri TMP în scenă.");
    }
}
