// Project:         Loading Screen for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=469
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/loadingscreen-du-mod
// Original Author: TheLacus (TheLacus@yandex.com)
// Contributors:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using FullSerializer;
using UnityEngine.PostProcessing;

namespace LoadingScreen.Components
{
    /// <summary>
    /// Draws a render texture of a model with particle system mist.
    /// </summary>
    public class ModelViewer : LoadingScreenComponent
    {
        #region Fields

        readonly Dictionary<string, (uint[] IDs, int[] Rotations)> modelDatabase = new Dictionary<string, (uint[], int[])>();

        readonly GameObject modelViewerGo;
        readonly Texture2D texture;
        readonly PostProcessingBehaviour postProcessingBehaviour;

        float horizontalPosition = 0.5f;

        #endregion

        #region Properties

        /// <summary>
        /// Horizontal position of model in range 0-1.
        /// </summary>
        public float HorizontalPosition
        {
            get { return horizontalPosition; }
            set { horizontalPosition = Mathf.Clamp01(value); }
        }

        public bool FilmicTonemapping
        {
            set { postProcessingBehaviour.profile.colorGrading.enabled = value; }
        }

        #endregion

        #region Constructors

        public ModelViewer(Rect virtualRect)
            :base(virtualRect)
        {
            modelViewerGo = LoadingScreen.Mod.GetAsset<GameObject>("ModelViewer", false);
            texture = new Texture2D((int)rect.size.x, (int)rect.size.y, TextureFormat.ARGB32, false);

            Camera camera = GetCamera(modelViewerGo);
            camera.enabled = false;
            camera.renderingPath = Camera.main.renderingPath;
            camera.targetTexture = null;
            Camera.main.cullingMask = Camera.main.cullingMask & ~(1 << modelViewerGo.layer);

            postProcessingBehaviour = camera.GetComponent<PostProcessingBehaviour>();

            var databaseAsset = LoadingScreen.Mod.GetAsset<TextAsset>("ModelViewerDatabase");
            fsResult fsResult = ModManager._serializer.TryDeserialize(fsJsonParser.Parse(databaseAsset.text), ref modelDatabase);
            if (!fsResult.Succeeded)
                Debug.LogError($"LoadingScreen: Failed to deserialize ModelViewerDatabase:\n{fsResult.FormattedMessages}");
        }

        #endregion

        #region Public Methods

        public override void Draw()
        {
            GUI.DrawTexture(rect, texture);
        }

        public override void OnLoadingScreen(SaveData_v1 saveData)
        {
            MakeTexture();
        }

        public override void OnLoadingScreen(DaggerfallTravelPopUp sender)
        {
            MakeTexture();
        }

        public override void OnLoadingScreen(PlayerEnterExit.TransitionEventArgs args)
        {
            MakeTexture();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Make a screenshot of a random model defined in <see cref="modelIDs"/>.
        /// </summary>
        private void MakeTexture()
        {
            // Setup components
            GameObject go = GameObject.Instantiate(modelViewerGo, LoadingScreen.Instance.transform);
            Camera camera = GetCamera(go);

            // Load model
            (uint id, int rotation) = GetModelInfo();
            GameObject model = LoadModel(go.transform, id);
            SetLayer(model.transform, go.layer);
            SetPosition(model.transform, rotation, camera, horizontalPosition);

            // Do mist. Shader must use ColorMask RGBA, otherwise the final color on render texture is (r*0, g*0, b*0).
            Transform mist = go.transform.Find("Mist");
            mist.GetComponent<ParticleSystem>().Play();

            // Make texture
            RenderTexture renderTexture = RenderTexture.GetTemporary((int)rect.size.x, (int)rect.size.y, 16, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 8);
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = camera.targetTexture;
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply(false);

            // Dispose components
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
            DestroyGameObject(go);
        }

        private (uint ID, int Rotation) GetModelInfo()
        {
#if UNITY_EDITOR
            if (LoadingScreen.Instance.OverrideModelID != -1 && LoadingScreen.Instance.OverrideModelRotation != -1)
                return ((uint)LoadingScreen.Instance.OverrideModelID, LoadingScreen.Instance.OverrideModelRotation);
#endif

            (uint[] ids, int[] rotations) = GetModelInfoItems(Parent.CurrentLoadingType);
            int index = Random.Range(0, ids.Length);
            return (ids[index], rotations[index]);
        }

        private (uint[] IDs, int[] Rotations) GetModelInfoItems(int loadingType)
        {
            switch (loadingType)
            {
                case LoadingType.Building: return modelDatabase["Building"];
                case LoadingType.Dungeon: return modelDatabase["Dungeon"];
                default: return modelDatabase["Exterior"];
            }
        }

        private GameObject LoadModel(Transform parent, uint modelID)
        {
            return MeshReplacement.ImportCustomGameobject(modelID, parent, Matrix4x4.identity) ??
                GameObjectHelper.CreateDaggerfallMeshGameObject(modelID, parent);
        }

        #endregion

        #region Public Helpers

        public static void SetLayer(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
                SetLayer(transform.GetChild(i), layer);
        }

        public static void SetPosition(Transform transform, int rotation, Camera camera, float horizontalPosition)
        {
            transform.Rotate(Vector3.up, 135 + rotation, Space.Self);
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer)
            {
                Bounds bounds = renderer.bounds;
                Vector3 pos = transform.position;
                Vector3 size = bounds.size;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;
                Vector3 offset = new Vector3(pos.x - center.x, pos.y - center.y, pos.z - center.z);

                // Calc position to fill camera frustum.
                pos.y = pos.y + offset.y;
                pos.z = size.y * 0.5f / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) + (size.z * 0.5f + offset.z);
                pos.x = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth * horizontalPosition, camera.pixelHeight * 0.5f, pos.z)).x + offset.x;

                // Horizontally clamp on screen
                if (horizontalPosition <= 0.5f)
                {
                    float leftOffset = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight * 0.5f, pos.z)).x - (pos.x - offset.x - extents.x);
                    if (leftOffset > 0)
                        pos.x += leftOffset;
                }
                else
                {
                    float rightOffset = (pos.x - offset.x + extents.x) - camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight * 0.5f, pos.z)).x;
                    if (rightOffset > 0)
                        pos.x -= rightOffset;
                }

                // Assign position
                transform.position = pos;
            }
        }

        #endregion

        #region Static Methods

        private static Camera GetCamera(GameObject go)
        {
            return go.transform.Find("Camera").GetComponent<Camera>();
        }

        private static void DestroyGameObject(GameObject go)
        {
            foreach (Transform transform in go.transform)
                DestroyGameObject(transform.gameObject);

            GameObject.Destroy(go);
        }

        #endregion
    }
}