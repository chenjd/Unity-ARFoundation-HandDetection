using System;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Add this component to a <c>Camera</c> to copy the color camera's texture onto the background.
    /// </summary>
    /// <remarks>
    /// This is the component-ized version of <c>UnityEngine.XR.ARBackgroundRenderer</c>.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(ARCameraManager))]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.5/api/UnityEngine.XR.ARFoundation.ARCameraBackground.html")]
    public sealed class ARCameraBackground : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("m_OverrideMaterial")]
        bool m_UseCustomMaterial;

        /// <summary>
        /// When <c>false</c>, a material is generated automatically from the shader included in the platform-specific package.
        /// When <c>true</c>, <see cref="customMaterial"/> is used instead, overriding the automatically generated one.
        /// This is not necessary for most AR experiences.
        /// </summary>
        public bool useCustomMaterial
        {
            get { return m_UseCustomMaterial; }
            set
            {
                m_UseCustomMaterial = value;
                UpdateMaterial();
            }
        }

        [SerializeField, FormerlySerializedAs("m_Material")]
        Material m_CustomMaterial;

        /// <summary>
        /// If <see cref="useCustomMaterial"/> is <c>true</c>, this <c>Material</c> will be used
        /// instead of the one included with the platform-specific AR package.
        /// </summary>
        public Material customMaterial
        {
            get { return m_CustomMaterial; }
            set
            {
                m_CustomMaterial = value;
                UpdateMaterial();
            }
        }

        /// <summary>
        /// The current <c>Material</c> used for background rendering.
        /// </summary>
        public Material material
        {
            get
            {
                return m_BackgroundRenderer.backgroundMaterial;
            }
            private set
            {
                m_BackgroundRenderer.backgroundMaterial = value;
            }
        }

        [SerializeField]
        bool m_UseCustomRendererAsset;

        /// <summary>
        /// Whether to use a <see cref="ARBackgroundRendererAsset"/>. This can assist with
        /// usage of the light weight render pipeline.
        /// </summary>
        public bool useCustomRendererAsset
        {
            get { return m_UseCustomRendererAsset; }
            set
            {
                m_UseCustomRendererAsset = value;
                SetupBackgroundRenderer();
            }
        }

        [SerializeField]
        ARBackgroundRendererAsset m_CustomRendererAsset;

        /// <summary>
        /// Get the custom <see cref="ARBackgroundRendererAsset "/> to use. This can
        /// assist with usage of the light weight render pipeline.
        /// </summary>
        public ARBackgroundRendererAsset customRendererAsset
        {
            get { return m_CustomRendererAsset; }
            set
            {
                m_CustomRendererAsset = value;
                SetupBackgroundRenderer();
            }
        }

        ARFoundationBackgroundRenderer m_BackgroundRenderer { get; set; }

        Material CreateMaterialFromSubsystemShader()
        {
            if (m_CameraSetupThrewException)
                return null;

            // Try to create a material from the plugin's provided shader.
            if (String.IsNullOrEmpty(m_CameraManager.shaderName))
                return null;

            var shader = Shader.Find(m_CameraManager.shaderName);
            if (shader == null)
            {
                // If an exception is thrown, then something is irrecoverably wrong.
                // Set this flag so we don't try to do this every frame.
                m_CameraSetupThrewException = true;

                throw new InvalidOperationException(string.Format(
                    "Could not find shader named \"{0}\" required for video overlay on camera subsystem.",
                    m_CameraManager.shaderName));
            }

            return new Material(shader);
        }

        void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            UpdateMaterial();

            var mat = material;
            var count = eventArgs.textures.Count;
            for (int i = 0; i < count; ++i)
            {
                mat.SetTexture(
                    eventArgs.propertyNameIds[i],
                    eventArgs.textures[i]);
            }

            mode = ARRenderMode.MaterialAsBackground;

            if (eventArgs.displayMatrix.HasValue)
                mat.SetMatrix(k_DisplayTransformId, eventArgs.displayMatrix.Value);

            if (eventArgs.projectionMatrix.HasValue)
                m_Camera.projectionMatrix = eventArgs.projectionMatrix.Value;
        }

        void SetupBackgroundRenderer()
        {
            if (useRenderPipeline)
            {
                if (m_LwrpBackgroundRenderer == null)
                {
                    m_LwrpBackgroundRenderer = m_CustomRendererAsset.CreateARBackgroundRenderer();
                    m_CustomRendererAsset.CreateHelperComponents(gameObject);
                }

                m_BackgroundRenderer = m_LwrpBackgroundRenderer;
            }
            else
            {
                if (m_LegacyBackgroundRenderer == null)
                    m_LegacyBackgroundRenderer = new ARFoundationBackgroundRenderer();

                m_BackgroundRenderer = m_LegacyBackgroundRenderer;
            }

            m_BackgroundRenderer.mode = mode;
            m_BackgroundRenderer.camera = m_Camera;
        }

        void Awake()
        {
            m_Camera = GetComponent<Camera>();
            m_CameraManager = GetComponent<ARCameraManager>();
            SetupBackgroundRenderer();
        }

        void OnEnable()
        {
            UpdateMaterial();
            m_CameraManager.frameReceived += OnCameraFrameReceived;
            ARSession.stateChanged += OnSessionStateChanged;
        }

        void OnDisable()
        {
            mode = ARRenderMode.StandardBackground;
            m_CameraManager.frameReceived -= OnCameraFrameReceived;
            ARSession.stateChanged -= OnSessionStateChanged;
            m_CameraSetupThrewException = false;

            // We are no longer setting the projection matrix
            // so tell the camera to resume its normal projection
            // matrix calculations.
            m_Camera.ResetProjectionMatrix();
        }

        void OnSessionStateChanged(ARSessionStateChangedEventArgs eventArgs)
        {
            // If the session goes away then return to using standard background mode
            if (eventArgs.state < ARSessionState.SessionInitializing && m_BackgroundRenderer != null)
                mode = ARRenderMode.StandardBackground;
        }

        void UpdateMaterial()
        {
            if (useRenderPipeline)
            {
                material = lwrpMaterial;
            }
            else
            {
                material = m_UseCustomMaterial ? m_CustomMaterial : subsystemMaterial;
            }
        }

        bool m_CameraSetupThrewException;

        Camera m_Camera;

        ARCameraManager m_CameraManager;

        Material m_SubsystemMaterial;

        private Material subsystemMaterial
        {
            get
            {
                if (m_SubsystemMaterial == null)
                    m_SubsystemMaterial = CreateMaterialFromSubsystemShader();

                return m_SubsystemMaterial;
            }
        }

        Material m_LwrpMaterial;

        Material lwrpMaterial
        {
            get
            {
                if (m_LwrpMaterial != null)
                    return m_LwrpMaterial;

                if (m_UseCustomRendererAsset && m_CustomRendererAsset != null)
                {
                    m_LwrpMaterial = m_CustomRendererAsset.CreateCustomMaterial();
                }

                return m_LwrpMaterial;
            }
        }

        ARFoundationBackgroundRenderer m_LegacyBackgroundRenderer;

        ARFoundationBackgroundRenderer m_LwrpBackgroundRenderer;

        ARRenderMode m_Mode;

        ARRenderMode mode
        {
            get { return m_Mode; }
            set
            {
                m_Mode = value;
                if (m_LwrpBackgroundRenderer != null)
                    m_LwrpBackgroundRenderer.mode = m_Mode;
                if (m_LegacyBackgroundRenderer != null)
                    m_LegacyBackgroundRenderer.mode = m_Mode;
            }
        }

        bool useRenderPipeline
        {
            get
            {
                return
                    m_UseCustomRendererAsset &&
                    (m_CustomRendererAsset != null) &&
                    (GraphicsSettings.renderPipelineAsset != null);
            }
        }

        const string k_DisplayTransformName = "_UnityDisplayTransform";

        static readonly int k_DisplayTransformId = Shader.PropertyToID(k_DisplayTransformName);
    }
}
