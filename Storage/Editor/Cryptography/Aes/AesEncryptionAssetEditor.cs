using Raccoons.Storage.Cryptography.Aes;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(AesEncryptionAsset))]
public class AesEncryptionAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (AesEncryptionAsset)target;
        if (GUILayout.Button("Verify", GUILayout.Height(40)))
        {
            script.Verify();
        }
        if (GUILayout.Button("Regenerate", GUILayout.Height(40)))
        {
            script.Regenerate();
        }
    }
}
