using UnityEngine;
using UnityEditor;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using LoadingScreen.Components;

namespace LoadingScreen
{
    public class ModelViewerEditorWindow : EditorWindow
    {
        GameObject parent;
        Camera camera;
        RenderTexture texture;
        Vector2Int resolution = new Vector2Int(1920, 1080);
        int modelID = -1;
        int rotation = -1;
        GameObject go;

        [MenuItem("Daggerfall Tools/Mods/Loading Screen/Model Viewer Editor")]
        public static void Init()
        {
            GetWindow<ModelViewerEditorWindow>();
        }

        private void OnEnable()
        {
            parent = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Mods/LoadingScreen/Resources/ModelViewer.prefab"));
            texture = new RenderTexture(resolution.x, resolution.y, 16);
            camera = parent.GetComponentInChildren<Camera>();
            camera.enabled = false;
            camera.renderingPath = Camera.main.renderingPath;
            camera.targetTexture = texture;
        }

        private void OnGUI()
        {
            bool changedResolution = false;

            resolution = EditorGUILayout.Vector2IntField("Resolution", resolution);
            if (GUILayout.Button("OK"))
            {
                texture.Release();
                camera.targetTexture = texture = new RenderTexture(resolution.x, resolution.y, 16);
                changedResolution = true;
            }

            GUILayoutHelper.Horizontal(() =>
            {
                modelID = EditorGUILayout.IntField("ModelID", modelID);
                rotation = EditorGUILayout.IntField("Rotation", rotation);
                if (GUILayout.Button("OK") || changedResolution)
                {
                    if (go)
                        DestroyImmediate(go);

                    if (go = GameObjectHelper.CreateDaggerfallMeshGameObject((uint)modelID, parent.transform))
                    {
                        ModelViewer.SetLayer(go.transform, parent.layer);
                        ModelViewer.SetPosition(go.transform, (uint)modelID, camera, 0.3f, rotation);
                        camera.Render();
                    }
                }
            });

            EditorGUILayout.LabelField(new GUIContent(texture), GUILayout.Height(512));
        }

        private void OnDisable()
        {
            texture.Release();
            texture = null;
            DestroyImmediate(parent);
        }
    }
}
