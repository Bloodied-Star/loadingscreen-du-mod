using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using FullSerializer;
using LoadingScreen.Components;

namespace LoadingScreen
{
    public enum ModelViewerItemRotation
    {
        None,
        Plus90,
        Minus90,
        Plus180
    }

    public class ModelViewerEditorWindow : EditorWindow
    {
        Dictionary<string, (List<int>, List<int>)> modelDatabase = new Dictionary<string, (List<int>, List<int>)>();

        GameObject parent;
        Camera camera;
        RenderTexture texture;
        Vector2Int resolution = new Vector2Int(1920, 1080);
        int modelID = -1;
        int rotation = -1;
        GameObject go;
        TextAsset modelDatabaseAsset;

        string[] databaseKeys;
        string[] currentDatabaseIDs;
        int currentDatabaseKey;
        int currentDatabaseItem;

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

            modelDatabaseAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Game/Mods/LoadingScreen/Resources/ModelViewerDatabase.json");
            fsData fsData = fsJsonParser.Parse(modelDatabaseAsset.text);
            ModManager._serializer.TryDeserialize(fsData, ref modelDatabase).AssertSuccess();
            databaseKeys = modelDatabase.Keys.ToArray();
        }

        private void OnGUI()
        {
            bool changedResolution = false;

            GUILayoutHelper.Horizontal(() =>
            {
                EditorGUILayout.LabelField("Resolution");
                resolution = EditorGUILayout.Vector2IntField("", resolution);
                if (GUILayout.Button("OK"))
                {
                    texture.Release();
                    camera.targetTexture = texture = new RenderTexture(resolution.x, resolution.y, 16);
                    changedResolution = true;
                }
            });

            GUILayoutHelper.Horizontal(() =>
            {
                int currentDatabaseKey = EditorGUILayout.Popup(this.currentDatabaseKey, databaseKeys);
                if (currentDatabaseKey != this.currentDatabaseKey)
                {
                    this.currentDatabaseKey = currentDatabaseKey;
                    currentDatabaseIDs = null;
                }

                if (currentDatabaseIDs == null)
                    currentDatabaseIDs = modelDatabase[databaseKeys[currentDatabaseKey]].Item1.Select(x => x.ToString()).ToArray();
                currentDatabaseItem = EditorGUILayout.Popup(currentDatabaseItem, currentDatabaseIDs);

                if (GUILayout.Button("OK"))
                {
                    (List<int> ids, List<int> rotations) = modelDatabase[databaseKeys[currentDatabaseKey]];
                    modelID = ids[currentDatabaseItem];
                    rotation = rotations[currentDatabaseItem];
                }
            });

            GUILayoutHelper.Horizontal(() =>
            {
                modelID = EditorGUILayout.IntField("ModelID", modelID);

                ModelViewerItemRotation modelViewerItemRotation;
                switch (rotation)
                {
                    case 90:
                        modelViewerItemRotation = ModelViewerItemRotation.Plus90;
                        break;

                    case -90:
                        modelViewerItemRotation = ModelViewerItemRotation.Minus90;
                        break;

                    case 180:
                        modelViewerItemRotation = ModelViewerItemRotation.Plus180;
                        break;

                    case 0:
                    default:
                        modelViewerItemRotation = ModelViewerItemRotation.None;
                        break;
                }

                switch ((ModelViewerItemRotation)EditorGUILayout.EnumPopup("Rotation", modelViewerItemRotation))
                {
                    case ModelViewerItemRotation.Plus90:
                        rotation = 90;
                        break;

                    case ModelViewerItemRotation.Minus90:
                        rotation = -90;
                        break;

                    case ModelViewerItemRotation.Plus180:
                        rotation = 180;
                        break;

                    case ModelViewerItemRotation.None:
                    default:
                        rotation = 0;
                        break;
                }


                if (GUILayout.Button("OK") || changedResolution)
                {
                    if (go)
                        DestroyImmediate(go);

                    if (go = GameObjectHelper.CreateDaggerfallMeshGameObject((uint)modelID, parent.transform))
                    {
                        ModelViewer.SetLayer(go.transform, parent.layer);
                        ModelViewer.SetPosition(go.transform, rotation, camera, 0.3f);
                        camera.Render();
                    }
                }
            });

            EditorGUILayout.LabelField(new GUIContent(texture), GUILayout.Height(512));

            GUILayoutHelper.Horizontal(() =>
            {
                if (GUILayout.Button("Add/Save"))
                {
                    (List<int> ids, List<int> rotations) = modelDatabase[databaseKeys[currentDatabaseKey]];
                    int index = ids.IndexOf(modelID);
                    if (index != -1)
                    {
                        rotations[index] = rotation;
                    }
                    else
                    {
                        ids.Add(modelID);
                        rotations.Add(rotation);
                        currentDatabaseIDs = null;
                    }
                }

                if (GUILayout.Button("Remove"))
                {
                    (List<int> ids, List<int> rotations) = modelDatabase[databaseKeys[currentDatabaseKey]];
                    int index = ids.IndexOf(modelID);
                    if (index != -1)
                    {
                        ids.RemoveAt(index);
                        rotations.RemoveAt(index);
                        currentDatabaseIDs = null;
                    }
                }
            });
        }

        private void OnDisable()
        {
            ModManager._serializer.TrySerialize(modelDatabase, out fsData fsData).AssertSuccess();
            File.WriteAllText(AssetDatabase.GetAssetPath(modelDatabaseAsset), fsJsonPrinter.PrettyJson(fsData));
            EditorUtility.SetDirty(modelDatabaseAsset);

            texture.Release();
            texture = null;
            DestroyImmediate(parent);
        }
    }
}
