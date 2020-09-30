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

namespace LoadingScreen.Components
{
    /// <summary>
    /// Draws a render texture of a model with particle system mist.
    /// </summary>
    public class ModelViewer : LoadingScreenComponent
    {
        #region ModelIDs

        private static class ModelIDs
        {
            static readonly uint[] Exterior = new uint[]
            {
                21203, 41222, 41241, 43022, 43085, 43206, 43307, 43728, 43202, 43730, 462, 402, 430, 223, 300,
                440, 62310, 845, 852, 20727, 305, 41235, 41501, 43045
            };

            static readonly uint[] Building = new uint[]
            {
                41001, 41004, 41027, 41120, 43732, 20620, 41117, 41003, 41017, 40719, 40721, 41009
            };

            static readonly uint[] Dungeon = new uint[]
            {
                41048, 41021, 41123, 41303, 41402, 41407, 62324, 74224, 41301, 74112, 56000, 61018, 60506, 61020,
                41300, 41403
            };

            public static uint[] Get(int loadingType)
            {
                switch (loadingType)
                {
                    case LoadingType.Building:  return Building;
                    case LoadingType.Dungeon:   return Dungeon;
                    default:                    return Exterior;
                }
            }
        }

        #endregion

        #region Fields

        readonly GameObject modelViewerGo;
        readonly Texture2D texture;

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
            uint modelID = GetModelID();
            GameObject model = LoadModel(go.transform, modelID);
            SetLayer(model.transform, go.layer);
            model.transform.Rotate(Vector3.up, 135 + GetRotation(modelID), Space.Self);
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer)
            {
                Bounds bounds = renderer.bounds;
                Vector3 pos = model.transform.position;
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
                    float rightOffset =  (pos.x - offset.x + extents.x) - camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight * 0.5f, pos.z)).x;
                    if (rightOffset > 0)
                        pos.x -= rightOffset;
                }

                // Assign position
                model.transform.position = pos;

                // Do mist. Shader must use ColorMask RGBA, otherwise the final color on render texture is (r*0, g*0, b*0).
                Transform mist = go.transform.Find("Mist");
                //Vector3 mistPos = mist.position;
                //mistPos.z = pos.z;
                //mist.position = mistPos;
                mist.GetComponent<ParticleSystem>().Play();
            }

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
            renderTexture = null;
            DestroyGameObject(go);
        }

        private uint GetModelID()
        {
#if UNITY_EDITOR
            if (LoadingScreen.Instance.OverrideModelID != -1)
                return (uint)LoadingScreen.Instance.OverrideModelID;
#endif

            uint[] modelIDs = ModelIDs.Get(Parent.CurrentLoadingType);
            return modelIDs[Random.Range(0, modelIDs.Length)];
        }

        private GameObject LoadModel(Transform parent, uint modelID)
        {
            return MeshReplacement.ImportCustomGameobject(modelID, parent, Matrix4x4.identity) ??
                GameObjectHelper.CreateDaggerfallMeshGameObject(modelID, parent);
        }

        #endregion

        #region Static Methods

        private static Camera GetCamera(GameObject go)
        {
            return go.transform.Find("Camera").GetComponent<Camera>();
        }

        private static void SetLayer(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
                SetLayer(transform.GetChild(i), layer);
        }

        private static void DestroyGameObject(GameObject go)
        {
            foreach (Transform transform in go.transform)
                DestroyGameObject(transform.gameObject);

            GameObject.Destroy(go);
        }

        private static int GetRotation(uint modelID)
        {
#if UNITY_EDITOR
            if (LoadingScreen.Instance.OverrideModelRotation != -1)
                return LoadingScreen.Instance.OverrideModelRotation;
#endif

            switch (modelID)
            {
                case 56000:
                case 305:
                case 41235:
                case 41501:
                    return -90;

                case 41001:
                case 43202:
                case 300:
                case 440:
                case 61020:
                case 20727:
                case 40721:
                    return 90;

                case 43307:
                case 223:
                case 708:
                case 74112:
                case 61018:
                case 845:
                case 852:
                    return 180;

                default:
                    return 0;
            }
        }

        #endregion
    }
}