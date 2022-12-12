﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public class DrawDebugTools : MonoBehaviour
{
    #region ========== Variables ==========
    private static DrawDebugTools instance;
    // Target camera 
    public Camera m_ActiveCamera;

    // Params
    public bool m_IsDrawingEnabled = true;

    // Lines
    private List<BatchedLine> m_BatchedLines;

    // Materials
    private Material m_LineMaterial;
    private Material m_AlphaMaterial;

    // Text
    public List<DebugText> m_DebugTextesList;
    private Font m_Debug2DTextFont;
    private Font m_Debug3DTextFont;

    // Debug camera
    private GameObject m_DebugCamera = null;
    private GameObject m_MainCamera = null;
    private bool m_DebugCameraIsActive = false;
    private float m_DebugCameraPitch = 0.0f;
    private float m_DebugCameraYaw = 0.0f;
    private float m_DebugCameraMovSpeedMultiplier = 0.1f;
    private Vector2 m_DebugCameraMovSpeedMultiplierRange = new Vector2(0.01f, 10.0f);

    // Debug float 
    private List<DebugFloatGraph> m_FloatGraphsList;
    private float m_GraphWidth = 300.0f;
    private float m_GraphHeight = 100.0f;
    private float m_GraphMargin_Right = 5.0f;
    private float m_GraphMargin_Buttom = 5.0f;
    private float m_GraphMargin_Top = 20;
    private float m_GraphTextAlpha = 0.5f;

    // Log message
    private float m_LogMessageMarginTop = 20.0f;
    private float m_LogMessageMarginLeft = 20.0f;
    private List<DebugLogMessage> m_LogMessagesList;
    private float m_LineSpacing = 15.0f;

    public static DrawDebugTools Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.FindObjectOfType<DrawDebugTools>();
            return instance;
        }

        set
        {
            instance = value;
        }
    }

    #endregion

    #region ========== Initialization ==========
    private void Awake()
    {
        Instance = this;
        // Init batched lines
        m_BatchedLines = new List<BatchedLine>();

        // Init debug text list
        m_DebugTextesList = new List<DebugText>();

        m_FloatGraphsList = new List<DebugFloatGraph>();

        // Init log messsages list
        m_LogMessagesList = new List<DebugLogMessage>();

        // Add DDTCamer component to the active camera
        if (m_ActiveCamera == null)
            m_ActiveCamera = Camera.main;
        DDTCamera DDTCam = m_ActiveCamera.gameObject.AddComponent<DDTCamera>();
        DDTCam.SetDrawDebugTools(this);
        DDTCam.SetActive(true);
        m_MainCamera = DDTCam.gameObject;
    }

    private void Start()
    {
        InitializeMaterials();

        // Initialize fonts
        string FontName = "Arial";
        m_Debug2DTextFont = Font.CreateDynamicFontFromOSFont(FontName, 12);
        m_Debug2DTextFont.RequestCharactersInTexture(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~", 12, FontStyle.Normal);

        m_Debug3DTextFont = Font.CreateDynamicFontFromOSFont(FontName, 32);
        m_Debug3DTextFont.RequestCharactersInTexture(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~", 32, FontStyle.Normal);

    }

    private void InitializeMaterials()
    {
        if (!m_LineMaterial)
        {
            Shader Shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(Shader);
            m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;

            //Turn on alpha blending
            m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_LineMaterial.SetOverrideTag("ZWrite", "On");
            m_LineMaterial.SetOverrideTag("ZTest", "LEqual");

            //Turn backface culling off
            m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            //Turn off depth writes
            m_LineMaterial.SetInt("_ZWrite", 0);


            Shader = Shader.Find("Hidden/Internal-GUITexture");
            m_AlphaMaterial = new Material(Shader);

            m_AlphaMaterial.hideFlags = HideFlags.HideAndDontSave;

            //  Turn on alpha blending
            m_AlphaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_AlphaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            //  Turn backface culling off
            m_AlphaMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            //  Turn off depth writes
            m_AlphaMaterial.SetInt("_ZWrite", 0);
        }
    }

    #endregion

    #region ========== OnPostRender Function ==========    
    //private void OnPostRender()
    //{
    //    if(!m_DebugCameraIsActive)
    //        Draw();        
    //}

    public void Draw()
    {
        if (m_IsDrawingEnabled)
        {
            HandleDrawingListOfLines();
            HandleDrawingListOfTextes();
            HandleDrawingListOfFloatGraphs();
        }
    }
    #endregion

    #region ========== Update Function ==========
    private void Update()
    {
        HandleListOfLogMessagesList();
        HandleDebugCamera();
    }
    #endregion

    #region ========== Camera Debug ==========
    private void HandleDebugCamera()
    {
        // Toggle debug camera 
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ToggleDebugCamera();
        }

        if (m_DebugCameraIsActive)
        {
            // Time manipulation
            if (Input.GetKeyDown(KeyCode.F10))    // Freeze time
                Time.timeScale = Time.timeScale == 0.0f ? 1.0f : 0.0f;

            float ClampedTimeScale = Time.timeScale;
            if (Input.GetKeyDown(KeyCode.F11))    // Slow time
                ClampedTimeScale -= 0.1f;

            if (Input.GetKeyDown(KeyCode.F12))    // Speed up time
                ClampedTimeScale += 0.1f;
            Time.timeScale = Mathf.Clamp(ClampedTimeScale, 0.0f, 100.0f);

            // Change camera movement speed
            float SpeedMultiplierSensitivity = m_DebugCameraMovSpeedMultiplier < 1.0f ? 2.0f : 10.0f;
            m_DebugCameraMovSpeedMultiplier += Input.mouseScrollDelta.y * SpeedMultiplierSensitivity * Time.unscaledDeltaTime;
            m_DebugCameraMovSpeedMultiplier = Mathf.Clamp(m_DebugCameraMovSpeedMultiplier, m_DebugCameraMovSpeedMultiplierRange.x, m_DebugCameraMovSpeedMultiplierRange.y);

            // Camera translation and rotation
            float MoveSpeed = 50.0f * m_DebugCameraMovSpeedMultiplier;
            float RotateSpeed = 100.0f;

            Vector3 DirectionSpeed = Vector3.zero;
            DirectionSpeed.z = Input.GetAxisRaw("Vertical") * MoveSpeed * Time.unscaledDeltaTime;
            DirectionSpeed.x = Input.GetAxisRaw("Horizontal") * MoveSpeed * Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.E))
                DirectionSpeed.y = 0.8f * MoveSpeed * Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.Q))
                DirectionSpeed.y = -0.8f * MoveSpeed * Time.unscaledDeltaTime;

            // Set debug cam position
            m_DebugCamera.transform.position += m_DebugCamera.transform.right * DirectionSpeed.x + m_DebugCamera.transform.forward * DirectionSpeed.z + Vector3.up * DirectionSpeed.y;

            // Set debug cam rotation
            if (Input.GetMouseButton(0))
            {
                m_DebugCameraYaw += Input.GetAxis("Mouse X") * RotateSpeed * Time.unscaledDeltaTime;
                m_DebugCameraPitch += -Input.GetAxis("Mouse Y") * RotateSpeed * Time.unscaledDeltaTime;
                m_DebugCamera.transform.eulerAngles = new Vector3(m_DebugCameraPitch, m_DebugCameraYaw, 0.0f);
            }

            // Debug camera raycast
            RaycastHit DebugHitInfos;
            if (Physics.Raycast(m_DebugCamera.transform.position, m_DebugCamera.transform.forward, out DebugHitInfos, 1000.0f))
            {
                // Draw normal line
                DrawLine(DebugHitInfos.point, DebugHitInfos.point + DebugHitInfos.normal, Color.red);
            }

            // Debug camera top text
            float TextTopMargin = 50.0f;

            float TextLinesHeight = 0.0f;
            DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Debug Camera", TextAnchor.LowerLeft, Color.cyan);

            TextLinesHeight += 20.0f;
            DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Position: " + m_DebugCamera.transform.position.ToString(), TextAnchor.LowerLeft, Color.yellow);

            TextLinesHeight += 15.0f;
            DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Rotation: " + m_DebugCamera.transform.eulerAngles.ToString(), TextAnchor.LowerLeft, Color.yellow);

            TextLinesHeight += 15.0f;
            DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Mov speed multiplier: " + m_DebugCameraMovSpeedMultiplier.ToString("00.00"), TextAnchor.LowerLeft, Color.yellow);

            TextLinesHeight += 15.0f;
            DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Time scale: " + Time.timeScale, TextAnchor.LowerLeft, Color.yellow);

            // Display camera raycast infos if we hit a gameobject with collider
            if (DebugHitInfos.collider != null)
            {
                TextLinesHeight += 30.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Debug RaycastHit Infos", TextAnchor.LowerLeft, Color.cyan);

                TextLinesHeight += 20.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit point: " + DebugHitInfos.point.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit normal: " + DebugHitInfos.normal.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit barycentricCoordinate: " + DebugHitInfos.barycentricCoordinate.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit distance: " + DebugHitInfos.distance.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit triangleIndex: " + DebugHitInfos.triangleIndex.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit textureCoord: " + DebugHitInfos.textureCoord.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit textureCoord2: " + DebugHitInfos.textureCoord2.ToString(), TextAnchor.LowerLeft, Color.yellow);

                TextLinesHeight += 15.0f;
                DrawString2D(new Vector2(20.0f, Screen.height - TextTopMargin - TextLinesHeight), "Ray hit Object name: \"" + DebugHitInfos.transform.name + "\"", TextAnchor.LowerLeft, Color.yellow);

                // Display materials names if any exits
                if (DebugHitInfos.transform.GetComponent<MeshRenderer>() != null || DebugHitInfos.transform.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    Material[] MatsArray;
                    if (DebugHitInfos.transform.GetComponent<MeshRenderer>() != null)
                        MatsArray = DebugHitInfos.transform.GetComponent<MeshRenderer>().materials;
                    else
                        MatsArray = DebugHitInfos.transform.GetComponent<SkinnedMeshRenderer>().materials;

                    TextLinesHeight += 30.0f;
                    DrawString2D(new Vector2(30.0f, Screen.height - TextTopMargin - TextLinesHeight), "Debug Mesh Materials", TextAnchor.LowerLeft, Color.cyan);
                    TextLinesHeight += 20.0f;
                    for (int i = 0; i < MatsArray.Length; i++)
                    {
                        DrawString2D(new Vector2(70.0f, Screen.height - TextTopMargin - TextLinesHeight), "Mat (" + i + "): " + MatsArray[i].name, TextAnchor.LowerLeft, Color.yellow);
                        TextLinesHeight += 15.0f;
                    }
                }
            }

            // Display debug camera controls
            TextLinesHeight = 100.0f;
            DrawString2D(new Vector2(20.0f, TextLinesHeight), "Controls", TextAnchor.LowerLeft, Color.cyan);
            TextLinesHeight -= 20.0f;
            DrawString2D(new Vector2(20.0f, TextLinesHeight), "Change mov speed: MouseWheel", TextAnchor.LowerLeft, Color.yellow);
            TextLinesHeight -= 15.0f;
            DrawString2D(new Vector2(20.0f, TextLinesHeight), "Toggle debug cam: F9", TextAnchor.LowerLeft, Color.yellow);
            TextLinesHeight -= 15.0f;
            DrawString2D(new Vector2(20.0f, TextLinesHeight), "Freeze time: F10", TextAnchor.LowerLeft, Color.yellow);
            TextLinesHeight -= 15.0f;
            DrawString2D(new Vector2(20.0f, TextLinesHeight), "Slow down time: F11", TextAnchor.LowerLeft, Color.yellow);
            TextLinesHeight -= 15.0f;
            DrawString2D(new Vector2(20.0f, TextLinesHeight), "Speed up time: F12", TextAnchor.LowerLeft, Color.yellow);
        }
    }

    public void ToggleDebugCamera()
    {
        if (m_DebugCameraIsActive)
        {
            // Delete debug camera 
            Destroy(m_DebugCamera);
            m_MainCamera.tag = "MainCamera";
            m_DebugCameraIsActive = false;
            if (m_MainCamera.GetComponent<DDTCamera>())
                m_MainCamera.GetComponent<DDTCamera>().SetActive(true);
        }
        else
        {
            // Create debug camera
            //m_MainCamera = Camera.main.gameObject;
            m_DebugCamera = new GameObject("DebugCamera");
            m_DebugCamera.transform.position = Camera.main.transform.position;
            m_DebugCamera.transform.rotation = Camera.main.transform.rotation;
            m_DebugCamera.transform.localScale = Camera.main.transform.localScale;

            // Set debug initial camera pitch & yaw rotation value
            m_DebugCameraPitch = m_MainCamera.transform.eulerAngles.x;
            m_DebugCameraYaw = m_MainCamera.transform.eulerAngles.y;

            // Switch cameras tag
            m_MainCamera.tag = "Untagged";
            if (m_MainCamera.GetComponent<DDTCamera>())
                m_MainCamera.GetComponent<DDTCamera>().SetActive(false);
            m_DebugCamera.tag = "MainCamera";

            // Set components
            Component[] ComponentsArray = m_MainCamera.GetComponents(typeof(Component));
            System.Type[] CompsToInclude = new System.Type[] { typeof(Camera) };
            for (int i = 0; i < ComponentsArray.Length; i++)
            {
                if (ComponentsArray[i] != null && CompsToInclude.Contains(ComponentsArray[i].GetType()))
                {
                    m_DebugCamera.AddComponent(ComponentsArray[i].GetType());
                }
            }

            //m_DebugCamera.AddComponent<DebugCamera>();
            DDTCamera DDTCam = m_DebugCamera.AddComponent<DDTCamera>();
            DDTCam.SetDrawDebugTools(this);
            DDTCam.SetActive(true);

            // Set debug camera new far clip plane
            m_DebugCamera.GetComponent<Camera>().farClipPlane = 10000.0f;

            // Set cam flag
            m_DebugCamera.GetComponent<Camera>().clearFlags = m_MainCamera.GetComponent<Camera>().clearFlags;
            m_DebugCamera.GetComponent<Camera>().backgroundColor = m_MainCamera.GetComponent<Camera>().backgroundColor;

            // Set debug camera active flag
            m_DebugCameraIsActive = true;

            // Set time scale to 0
            Time.timeScale = 0.0f;
        }
    }
    #endregion

    #region ========== Drawing Functions ==========
    /// <summary>
    /// Call this method to draw a wire sphere
    /// </summary>
    /// <param name="Center">Position of the sphere</param>
    /// <param name="Radius">Raduis of the sphere</param>
    /// <param name="Segments">Segments count that form the sphere</param>
    /// <param name="Color">Color of the sphere</param>
    /// <param name="LifeTime">Lifetime before stop drawing the sphere</param>
    public void DrawSphere(Vector3 Center, float Radius, int Segments, Color Color, float LifeTime = 0.0f)
    {
        DrawSphere(Center, Quaternion.identity, Radius, Segments, Color, LifeTime);
    }

    /// <summary>
    /// Call this method to draw a wire sphere
    /// </summary>
    /// <param name="Center">Position of the sphere</param>
    /// <param name="Rotation"></param>
    /// <param name="Radius">Raduis of the sphere</param>
    /// <param name="Segments">Segments count that form the sphere</param>
    /// <param name="Color">Color of the sphere</param>
    /// <param name="LifeTime">Lifetime before stop drawing the sphere</param>
    public void DrawSphere(Vector3 Center, Quaternion Rotation, float Radius, int Segments, Color Color, float LifeTime = 0.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        List<BatchedLine> Lines;
        Lines = new List<BatchedLine>();

        for (int i = 0; i < Segments; i++)
        {
            float PolarAngle = AngleInc;
            float AzimuthalAngle = AngleInc * i;

            float Point_1_X = Mathf.Sin(PolarAngle) * Mathf.Cos(AzimuthalAngle);
            float Point_1_Y = Mathf.Cos(PolarAngle);
            float Point_1_Z = Mathf.Sin(PolarAngle) * Mathf.Sin(AzimuthalAngle);

            float Point_2_X;
            float Point_2_Y;
            float Point_2_Z;

            for (int J = 0; J < Segments; J++)
            {

                Point_2_X = Mathf.Sin(PolarAngle) * Mathf.Cos(AzimuthalAngle);
                Point_2_Y = Mathf.Cos(PolarAngle);
                Point_2_Z = Mathf.Sin(PolarAngle) * Mathf.Sin(AzimuthalAngle);

                float Point_3_X = Mathf.Sin(PolarAngle) * Mathf.Cos(AzimuthalAngle + AngleInc);
                float Point_3_Y = Mathf.Cos(PolarAngle);
                float Point_3_Z = Mathf.Sin(PolarAngle) * Mathf.Sin(AzimuthalAngle + AngleInc);

                Vector3 Point_1 = new Vector3(Point_1_X, Point_1_Y, Point_1_Z) * Radius + Center;
                Vector3 Point_2 = new Vector3(Point_2_X, Point_2_Y, Point_2_Z) * Radius + Center;
                Vector3 Point_3 = new Vector3(Point_3_X, Point_3_Y, Point_3_Z) * Radius + Center;

                Lines.Add(new BatchedLine(Point_1, Point_2, Center, Rotation, Color, LifeTime));
                Lines.Add(new BatchedLine(Point_2, Point_3, Center, Rotation, Color, LifeTime));

                Point_1_X = Point_2_X;
                Point_1_Y = Point_2_Y;
                Point_1_Z = Point_2_Z;

                PolarAngle += AngleInc;
            }
        }

        m_BatchedLines.AddRange(Lines);
    }

    /// <summary>
    /// Draw a 3D line in space
    /// </summary>
    /// <param name="LineStart">Position of the line start</param>
    /// <param name="LineEnd">Position of the line end</param>
    /// <param name="Color">Color of the line</param>
    /// <param name="LifeTime">Line life time</param>
    public void DrawLine(Vector3 LineStart, Vector3 LineEnd, Color Color, float LifeTime = 0.0f)
    {
        m_BatchedLines.Add(new BatchedLine(LineStart, LineEnd, Vector3.zero, Quaternion.identity, Color, LifeTime));
    }

    /// <summary>
    /// Draw a 3D point in space
    /// </summary>
    /// <param name="Position">Position of the point</param>
    /// <param name="Size">Size of the point</param>
    /// <param name="Color">Color of the point</param>
    /// <param name="LifeTime">Point life time</param>
    public void DrawPoint(Vector3 Position, float Size, Color Color, float LifeTime = 0.0f)
    {
        // X
        InternalDrawLine(Position + new Vector3(-Size / 2.0f, 0.0f, 0.0f), Position + new Vector3(Size / 2.0f, 0.0f, 0.0f), Position, Quaternion.identity, Color, LifeTime);
        // Y
        InternalDrawLine(Position + new Vector3(0.0f, -Size / 2.0f, 0.0f), Position + new Vector3(0.0f, Size / 2.0f, 0.0f), Position, Quaternion.identity, Color, LifeTime);
        // Z
        InternalDrawLine(Position + new Vector3(0.0f, 0.0f, -Size / 2.0f), Position + new Vector3(0.0f, 0.0f, Size / 2.0f), Position, Quaternion.identity, Color, LifeTime);
    }

    /// <summary>
    /// Draw directional arrow
    /// </summary>
    /// <param name="ArrowStart">Arrow start position</param>
    /// <param name="ArrowEnd">Arrow end position</param>
    /// <param name="ArrowSize">Arrow size</param>
    /// <param name="Color">Arrow color</param>
    /// <param name="LifeTime">Arrow life time</param>
    public void DrawDirectionalArrow(Vector3 ArrowStart, Vector3 ArrowEnd, float ArrowSize, Color Color, float LifeTime = 0.0f)
    {
        InternalDrawLine(ArrowStart, ArrowEnd, ArrowStart, Quaternion.identity, Color, LifeTime);

        Vector3 Dir = (ArrowEnd - ArrowStart).normalized;
        Vector3 Right = Vector3.Cross(Vector3.up, Dir);

        InternalDrawLine(ArrowEnd, ArrowEnd + (Right - Dir.normalized) * ArrowSize, ArrowStart, Quaternion.identity, Color, LifeTime);
        InternalDrawLine(ArrowEnd, ArrowEnd + (-Right - Dir.normalized) * ArrowSize, ArrowStart, Quaternion.identity, Color, LifeTime);
    }

    /// <summary>
    /// Draw a box
    /// </summary>
    /// <param name="Center">Center position of the box</param>
    /// <param name="Rotation">Rotaion of the box</param>
    /// <param name="Extent">The extent of the box</param>
    /// <param name="Color">Color of the box</param>
    /// <param name="LifeTime">Box life time</param>
    public void DrawBox(Vector3 Center, Quaternion Rotation, Vector3 Extent, Color Color, float LifeTime = 0.0f)
    {
        InternalDrawLine(Center + new Vector3(Extent.x, Extent.y, Extent.z), Center + new Vector3(Extent.x, -Extent.y, Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(Extent.x, -Extent.y, Extent.z), Center + new Vector3(-Extent.x, -Extent.y, Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, -Extent.y, Extent.z), Center + new Vector3(-Extent.x, Extent.y, Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, Extent.y, Extent.z), Center + new Vector3(Extent.x, Extent.y, Extent.z), Center, Rotation, Color, LifeTime);

        InternalDrawLine(Center + new Vector3(Extent.x, Extent.y, -Extent.z), Center + new Vector3(Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(Extent.x, -Extent.y, -Extent.z), Center + new Vector3(-Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, -Extent.y, -Extent.z), Center + new Vector3(-Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, Extent.y, -Extent.z), Center + new Vector3(Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);

        InternalDrawLine(Center + new Vector3(Extent.x, Extent.y, Extent.z), Center + new Vector3(Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(Extent.x, -Extent.y, Extent.z), Center + new Vector3(Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, -Extent.y, Extent.z), Center + new Vector3(-Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, Extent.y, Extent.z), Center + new Vector3(-Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, LifeTime);
    }

    /// <summary>
    /// Draw a 3D circle
    /// </summary>
    /// <param name="Center">Centre position of the circle</param>
    /// <param name="Rotation">Rotation of the circle</param>
    /// <param name="Radius">Radius of the circle</param>
    /// <param name="Segments">Segments count in the circle</param>
    /// <param name="Color">Color of the circle</param>
    /// <param name="LifeTime">Circle life time</param>
    public void DrawCircle(Vector3 Center, Quaternion Rotation, float Radius, int Segments, Color Color, float LifeTime = 0.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        float Angle = 0.0f;
        for (int i = 0; i < Segments; i++)
        {
            Vector3 Point_1 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
            Angle += AngleInc;
            Vector3 Point_2 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
            InternalDrawLine(Point_1, Point_2, Center, Rotation, Color, LifeTime);
        }
    }

    /// <summary>
    /// Draw a 3D circle
    /// </summary>
    /// <param name="Center">Centre position of the circle</param>
    /// <param name="Radius">Radius of the circle</param>
    /// <param name="Segments">Segments count in the circle</param>
    /// <param name="Color">Color of the circle</param>
    /// <param name="DrawPlaneAxis">Plane axis to draw circle in (XZ, XY, YZ)</param>
    /// <param name="LifeTime">Circle life time</param>
    public void DrawCircle(Vector3 Center, float Radius, int Segments, Color Color, EDrawPlaneAxis DrawPlaneAxis = EDrawPlaneAxis.XZ, float LifeTime = 0.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        float Angle = 0.0f;
        switch (DrawPlaneAxis)
        {
            case EDrawPlaneAxis.XZ:
                for (int i = 0; i < Segments; i++)
                {
                    Vector3 Point_1 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
                    Angle += AngleInc;
                    Vector3 Point_2 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
                    InternalDrawLine(Point_1, Point_2, Center, Quaternion.identity, Color, LifeTime);
                }
                break;
            case EDrawPlaneAxis.XY:
                for (int i = 0; i < Segments; i++)
                {
                    Vector3 Point_1 = Center + Radius * new Vector3(0.0f, Mathf.Sin(Angle), Mathf.Cos(Angle));
                    Angle += AngleInc;
                    Vector3 Point_2 = Center + Radius * new Vector3(0.0f, Mathf.Sin(Angle), Mathf.Cos(Angle));
                    InternalDrawLine(Point_1, Point_2, Center, Quaternion.identity, Color, LifeTime);
                }
                break;
            case EDrawPlaneAxis.YZ:
                for (int i = 0; i < Segments; i++)
                {
                    Vector3 Point_1 = Center + Radius * new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle));
                    Angle += AngleInc;
                    Vector3 Point_2 = Center + Radius * new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle));
                    InternalDrawLine(Point_1, Point_2, Center, Quaternion.identity, Color, LifeTime);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Draw a 3D coordinates
    /// </summary>
    /// <param name="Position">Position of the coordinates</param>
    /// <param name="Rotation">Rotation of the coordinates</param>
    /// <param name="Scale">Scale of the coordinate</param>
    /// <param name="LifeTime">Coordinates lifetime</param>
    public void Draw3DCoordinates(Vector3 Position, Quaternion Rotation, float Scale, float LifeTime = 0.0f)
    {
        InternalDrawLine(Position, Position + new Vector3(Scale, 0.0f, 0.0f), Position, Rotation, Color.red, LifeTime);
        InternalDrawLine(Position, Position + new Vector3(0.0f, Scale, 0.0f), Position, Rotation, Color.green, LifeTime);
        InternalDrawLine(Position, Position + new Vector3(0.0f, 0.0f, Scale), Position, Rotation, Color.blue, LifeTime);
    }

    /// <summary>
    /// Draw a 3D cylinder
    /// </summary>
    /// <param name="Start">Cylinder start position</param>
    /// <param name="End">Cylinder end position</param>
    /// <param name="Radius">Cylinder radius</param>
    /// <param name="Segments">Cylinder segments count</param>
    /// <param name="Color">Color of the cylinder</param>
    /// <param name="LifeTime">Cylinder life time</param>
    public void DrawCylinder(Vector3 Start, Vector3 End, float Radius, int Segments, Color Color, float LifeTime = 0.0f)
    {
        Vector3 Center = (Start + End) / 2.0f;
        InternalDrawCylinder(Start, End, Quaternion.identity, Center, Radius, Segments, Color, LifeTime);
    }

    /// <summary>
    /// Draw a 3D cone
    /// </summary>
    /// <param name="Position">Cone position</param>
    /// <param name="Direction">Cone direction</param>
    /// <param name="Length">Cone length</param>
    /// <param name="AngleWidth">Cone angle with</param>
    /// <param name="AngleHeight">Cone angle height</param>
    /// <param name="Segments">Cone segments count</param>
    /// <param name="Color">Cone color</param>
    /// <param name="LifeTime">Cone life time</param>
    public void DrawCone(Vector3 Position, Vector3 Direction, float Length, float AngleWidth, float AngleHeight, int Segments, Color Color, float LifeTime = 0.0f)
    {
        Segments = Mathf.Max(Segments, 4);

        float SmallNumber = 0.001f;
        float Angle1 = Mathf.Clamp(AngleHeight * Mathf.Deg2Rad, SmallNumber, Mathf.PI - SmallNumber);
        float Angle2 = Mathf.Clamp(AngleWidth * Mathf.Deg2Rad, SmallNumber, Mathf.PI - SmallNumber);

        float SinX2 = Mathf.Sin(0.5f * Angle1);
        float SinY2 = Mathf.Sin(0.5f * Angle2);

        float SqrSinX2 = SinX2 * SinX2;
        float SqrSinY2 = SinY2 * SinY2;

        float TanX2 = Mathf.Tan(0.5f * Angle1);
        float TanY2 = Mathf.Tan(0.5f * Angle2);

        Vector3[] ConeVerts;
        ConeVerts = new Vector3[Segments];

        for (int i = 0; i < Segments; i++)
        {
            float AngleFragment = (float)i / (float)(Segments);
            float ThiAngle = 2.0f * Mathf.PI * AngleFragment;
            float PhiAngle = Mathf.Atan2(Mathf.Sin(ThiAngle) * SinY2, Mathf.Cos(ThiAngle) * SinX2);
            float SinPhiAngle = Mathf.Sin(PhiAngle);
            float CosPhiAngle = Mathf.Cos(PhiAngle);
            float SqrSinPhi = SinPhiAngle * SinPhiAngle;
            float SqrCosPhi = CosPhiAngle * CosPhiAngle;

            float RSq = SqrSinX2 * SqrSinY2 / (SqrSinX2 * SqrSinPhi + SqrSinY2 * SqrCosPhi);
            float R = Mathf.Sqrt(RSq);
            float Sqr = Mathf.Sqrt(1 - RSq);
            float Alpha = R * CosPhiAngle;
            float Beta = R * SinPhiAngle;


            ConeVerts[i].x = (1 - 2 * RSq);
            ConeVerts[i].y = 2 * Sqr * Alpha;
            ConeVerts[i].z = 2 * Sqr * Beta;
        }

        Vector3 ConeDirection = Direction.normalized;

        Vector3 AngleFromDirection = Quaternion.LookRotation(ConeDirection, Vector3.up).eulerAngles - new Vector3(0.0f, 90.0f, 0.0f);
        Quaternion Q = Quaternion.Euler(new Vector3(AngleFromDirection.z, AngleFromDirection.y, -AngleFromDirection.x));
        Matrix4x4 M = Matrix4x4.TRS(Position, Q, Vector3.one * Length);

        Vector3 CurrentPoint = Vector3.zero;
        Vector3 PrevPoint = Vector3.zero;
        Vector3 FirstPoint = Vector3.zero;

        for (int i = 0; i < Segments; i++)
        {
            CurrentPoint = M.MultiplyPoint(ConeVerts[i]);
            DrawLine(Position, CurrentPoint, Color, LifeTime);

            if (i == 0)
            {
                FirstPoint = CurrentPoint;
            }
            else
            {
                DrawLine(PrevPoint, CurrentPoint, Color, LifeTime);
            }
            PrevPoint = CurrentPoint;
        }

        DrawLine(CurrentPoint, FirstPoint, Color, LifeTime);
    }

    /// <summary>
    /// Draw 2D text on screen
    /// </summary>
    /// <param name="Position">2D position of the text</param>
    /// <param name="Text">Text string</param>
    /// <param name="Anchor">Text anchor</param>
    /// <param name="TextColor">Text color</param>
    /// <param name="LifeTime">Text life time</param>
    public void DrawString2D(Vector2 Position, string Text, TextAnchor Anchor, Color TextColor, float LifeTime = 0.0f)
    {
        InternalAddDebugText(Text, Anchor, Position - new Vector2(0.0f, 1.0f), Quaternion.identity, Color.black, 1.0f, LifeTime, true);
        InternalAddDebugText(Text, Anchor, Position, Quaternion.identity, TextColor, 1.0f, LifeTime, true);
    }

    /// <summary>
    /// Dray 3D text
    /// </summary>
    /// <param name="Position">Position of the text</param>
    /// <param name="Rotation">Rotation of the text</param>
    /// <param name="Text">Text string</param>
    /// <param name="Anchor">Text anchor</param>
    /// <param name="TextColor">Text color</param>
    /// <param name="TextSize">Text size</param>
    /// <param name="LifeTime">Text life time</param>
    public void DrawString3D(Vector3 Position, Quaternion Rotation, string Text, TextAnchor Anchor, Color TextColor, float TextSize = 1.0f, float LifeTime = 0.0f)
    {
        InternalAddDebugText(Text, Anchor, Position, Rotation, TextColor, TextSize * 0.01f, LifeTime, false);
    }

    /// <summary>
    /// Draw camera frustum
    /// </summary>
    /// <param name="Camera">Target camera</param>
    /// <param name="Color">Frustum color</param>
    /// <param name="LifeTime">Frustum life time</param>
    public void DrawFrustum(Camera Camera, Color Color, float LifeTime = 0.0f)
    {
        Plane[] FrustumPlanes = m_DebugCameraIsActive ? GeometryUtility.CalculateFrustumPlanes(m_MainCamera.GetComponent<Camera>()) : GeometryUtility.CalculateFrustumPlanes(Camera);
        Vector3[] NearPlaneCorners = new Vector3[4];
        Vector3[] FarePlaneCorners = new Vector3[4];

        Plane TempPlane = FrustumPlanes[1]; FrustumPlanes[1] = FrustumPlanes[2]; FrustumPlanes[2] = TempPlane;

        for (int i = 0; i < 4; i++)
        {
            NearPlaneCorners[i] = GetIntersectionPointOfPlanes(FrustumPlanes[4], FrustumPlanes[i], FrustumPlanes[(i + 1) % 4]);
            FarePlaneCorners[i] = GetIntersectionPointOfPlanes(FrustumPlanes[5], FrustumPlanes[i], FrustumPlanes[(i + 1) % 4]);
        }

        for (int i = 0; i < 4; i++)
        {
            InternalDrawLine(NearPlaneCorners[i], NearPlaneCorners[(i + 1) % 4], Vector3.zero, Quaternion.identity, Color, LifeTime);
            InternalDrawLine(FarePlaneCorners[i], FarePlaneCorners[(i + 1) % 4], Vector3.zero, Quaternion.identity, Color, LifeTime);
            InternalDrawLine(NearPlaneCorners[i], FarePlaneCorners[i], Vector3.zero, Quaternion.identity, Color, LifeTime);
        }
    }

    /// <summary>
    /// Draw 3D capsule
    /// </summary>
    /// <param name="Center">Center position of the capsule</param>
    /// <param name="HalfHeight">Capsule half height</param>
    /// <param name="Radius">Capsule radius</param>
    /// <param name="Rotation">Capsule rotation</param>
    /// <param name="Color">Capsule color</param>
    /// <param name="LifeTime">Capsule life time</param>
    public void DrawCapsule(Vector3 Center, float HalfHeight, float Radius, Quaternion Rotation, Color Color, float LifeTime = 0.0f)
    {
        int Segments = 16;

        Matrix4x4 M = Matrix4x4.TRS(Vector3.zero, Rotation, Vector3.one);

        Vector3 AxisX = M.MultiplyVector(Vector3.right);
        Vector3 AxisY = M.MultiplyVector(Vector3.up);
        Vector3 AxisZ = M.MultiplyVector(Vector3.forward);

        float HalfMaxed = Mathf.Max(HalfHeight - Radius, 0.1f);
        Vector3 TopPoint = Center + HalfMaxed * AxisY;
        Vector3 BottomPoint = Center - HalfMaxed * AxisY;

        InternalDrawCapsuleCircle(TopPoint, AxisX, AxisZ, Color, Radius, Segments, LifeTime);
        InternalDrawCapsuleCircle(BottomPoint, AxisX, AxisZ, Color, Radius, Segments, LifeTime);

        InternalDrawHalfCircle(TopPoint, AxisX, AxisY, Color, Radius, Segments, LifeTime);
        InternalDrawHalfCircle(TopPoint, AxisZ, AxisY, Color, Radius, Segments, LifeTime);

        InternalDrawHalfCircle(BottomPoint, AxisX, -AxisY, Color, Radius, Segments, LifeTime);
        InternalDrawHalfCircle(BottomPoint, AxisZ, -AxisY, Color, Radius, Segments, LifeTime);

        InternalDrawLine(TopPoint + Radius * AxisX, BottomPoint + Radius * AxisX, Vector3.zero, Quaternion.identity, Color, LifeTime);
        InternalDrawLine(TopPoint - Radius * AxisX, BottomPoint - Radius * AxisX, Vector3.zero, Quaternion.identity, Color, LifeTime);
        InternalDrawLine(TopPoint + Radius * AxisZ, BottomPoint + Radius * AxisZ, Vector3.zero, Quaternion.identity, Color, LifeTime);
        InternalDrawLine(TopPoint - Radius * AxisZ, BottomPoint - Radius * AxisZ, Vector3.zero, Quaternion.identity, Color, LifeTime);
    }

    /// <summary>
    /// Draw a 3D representation of the main camera
    /// </summary>
    /// <param name="Color">Camera shape color</param>
    /// <param name="Scale">Scale of the shape</param>
    /// <param name="LifeTime">Shape life time</param>
    public void DrawActiveCamera(Color Color, float LifeTime = 0.0f)
    {
        Camera ActiveCam = Camera.main;
        if (m_DebugCameraIsActive)
            ActiveCam = m_MainCamera.GetComponent<Camera>();
        InternalDrawCamera(ActiveCam, Color, LifeTime);
    }

    /// <summary>
    /// Draw a 3D representation of a camera
    /// </summary>
    /// /// <param name="Cam">Camera to draw</param>
    /// <param name="Color">Camera representation color</param>
    /// <param name="Scale">Scale of the shape</param>
    /// <param name="LifeTime">Shape life time</param>
    public void DrawCamera(Camera Cam, Color Color, float LifeTime = 0.0f)
    {
        InternalDrawCamera(Cam, Color, LifeTime);
    }

    /// <summary>
    /// Draw a grid in 3D space
    /// </summary>
    /// <param name="Position">Position of the grid</param>
    /// <param name="GridSize">Grid size</param>
    /// <param name="CellSize">Grid cell size</param>
    /// <param name="LifeTime">Grid life time</param>
    public void DrawGrid(Vector3 Position, float GridSize, float CellSize, float LifeTime)
    {
        DrawGrid(Position, Vector3.up, GridSize, CellSize, LifeTime);
    }

    /// <summary>
    /// Draw a grid in 3D space
    /// </summary>
    /// <param name="Position">Position of the grid</param>
    /// <param name="Normal">Normal vector of the grid</param>
    /// <param name="GridSize">Grid size</param>
    /// <param name="CellSize">Grid cell size</param>
    /// <param name="LifeTime">Grid life time</param>
    public void DrawGrid(Vector3 Position, Vector3 Normal, float GridSize, float CellSize, float LifeTime = 0.0f)
    {
        Color MajorLinesColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        Color OtherLinesColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        float HalfGridSize = GridSize / 2.0f;
        Quaternion GridRot = Quaternion.LookRotation(Normal);

        // Draw rectangle
        InternalDrawLine(new Vector3(Position.x - HalfGridSize, Position.y - HalfGridSize, Position.z),
                        new Vector3(Position.x - HalfGridSize, Position.y + HalfGridSize, Position.z),
                        Position, GridRot, MajorLinesColor, LifeTime);

        InternalDrawLine(new Vector3(Position.x - HalfGridSize, Position.y + HalfGridSize, Position.z),
                        new Vector3(Position.x + HalfGridSize, Position.y + HalfGridSize, Position.z),
                        Position, GridRot, MajorLinesColor, LifeTime);

        InternalDrawLine(new Vector3(Position.x + HalfGridSize, Position.y + HalfGridSize, Position.z),
                        new Vector3(Position.x + HalfGridSize, Position.y - HalfGridSize, Position.z),
                        Position, GridRot, MajorLinesColor, LifeTime);

        InternalDrawLine(new Vector3(Position.x + HalfGridSize, Position.y - HalfGridSize, Position.z),
                        new Vector3(Position.x - HalfGridSize, Position.y - HalfGridSize, Position.z),
                        Position, GridRot, MajorLinesColor, LifeTime);

        // Draw centered axis
        InternalDrawLine(new Vector3(Position.x - HalfGridSize, Position.y, Position.z),
                new Vector3(Position.x + HalfGridSize, Position.y, Position.z),
                Position, GridRot, new Color(0.8f, 0.3f, 0.3f, 1.0f), LifeTime);
        InternalDrawLine(new Vector3(Position.x, Position.y - HalfGridSize, Position.z),
                new Vector3(Position.x, Position.y + HalfGridSize, Position.z),
                Position, GridRot, new Color(0.3f, 0.3f, 0.8f, 1.0f), LifeTime);

        int CellNum = (int)Mathf.Ceil((GridSize / CellSize) / 2.0f) - 1;

        // Draw grid lines
        for (int i = -CellNum; i <= CellNum; i++)
        {
            if (i == 0) continue;
            Vector3 V1 = new Vector3(Position.x + i * (CellSize), Position.y - HalfGridSize, Position.z);
            Vector3 V2 = new Vector3(Position.x + i * (CellSize), Position.y + HalfGridSize, Position.z);
            InternalDrawLine(V1, V2, Position, GridRot, OtherLinesColor, LifeTime);

            V1 = new Vector3(Position.x - HalfGridSize, Position.y + i * (CellSize), Position.z);
            V2 = new Vector3(Position.x + HalfGridSize, Position.y + i * (CellSize), Position.z);
            InternalDrawLine(V1, V2, Position, GridRot, OtherLinesColor, LifeTime);
        }

    }

    /// <summary>
    /// Draw a mesure tool to mesure distance between two points
    /// </summary>
    /// <param name="Start">Start position</param>
    /// <param name="End">End position</param>
    /// <param name="Color">Color of the mesure tool</param>
    /// <param name="LifeTime">Draw life time</param>
    public void DrawDistance(Vector3 Start, Vector3 End, Color Color, float TextSize = 1.0f, float LifeTime = 0.0f)
    {
        float Dist = Vector3.Distance(Start, End);
        Vector3 DistTextPos = (Start + End) / 2.0f;
        float DistEndSize = 0.3f;
        InternalDrawLine(Start, End, DistTextPos, Quaternion.identity, Color, LifeTime);

        Vector3 DistDir = (End - Start).normalized;
        Vector3 RightDir = Vector3.Cross(DistDir, Vector3.up);

        InternalDrawLine(Start - RightDir * DistEndSize, Start + RightDir * DistEndSize, DistTextPos, Quaternion.identity, Color, LifeTime);
        InternalDrawLine(End - RightDir * DistEndSize, End + RightDir * DistEndSize, DistTextPos, Quaternion.identity, Color, LifeTime);

        DrawString3D(DistTextPos, Quaternion.LookRotation(Camera.main.transform.position - DistTextPos), Dist.ToString(".00"), TextAnchor.MiddleCenter, Color.white, TextSize, LifeTime);
    }

    /// <summary>
    /// Log message text on screen
    /// </summary>
    /// <param name="LogMessage">Log message string</param>
    /// <param name="Color">Log messsage color</param>
    /// <param name="LifeTime">Log life time</param>
    public void Log(string LogMessage, Color Color, float LifeTime = 0.0f)
    {
        m_LogMessagesList.Insert(0, new DebugLogMessage(LogMessage, Color, LifeTime));
    }

    /// <summary>
    /// Draw a float graph on screen
    /// </summary>
    /// <param name="UniqueGraphName">A unique name for the graph</param>
    /// <param name="FloatValueToDebug">Float variable to debug</param>
    /// <param name="GraphHalfMinMaxRange">Min graph value</param>
    /// <param name="AutoAdjustMinMaxRange">Max graph value</param>
    /// <param name="SamplesCount">Graph sample count (Default = 50)</param>
    public void DrawFloatGraph(string UniqueGraphName, float FloatValueToDebug, float GraphHalfMinMaxRange = 100.0f, bool AutoAdjustMinMaxRange = false, int SamplesCount = 50)
    {
        bool IsFloatAlreadyExists = false;
        int FloatIndex = -1;
        float TimeBeforeRemoveInactiveGraph = 2.0f;

        for (int i = 0; i < m_FloatGraphsList.Count; i++)
        {
            if (Instance.m_FloatGraphsList[i].m_UniqueFloatName == UniqueGraphName)
            {
                IsFloatAlreadyExists = true;
                FloatIndex = i;
            }
        }

        if (IsFloatAlreadyExists)
        {
            Instance.m_FloatGraphsList[FloatIndex].AddValue(FloatValueToDebug);
        }
        else
        {
            Instance.m_FloatGraphsList.Add(new DebugFloatGraph(UniqueGraphName, SamplesCount, GraphHalfMinMaxRange, AutoAdjustMinMaxRange, TimeBeforeRemoveInactiveGraph));
        }
    }

    /// <summary>
    /// Draw colliders compoenent on game object and its children
    /// </summary>
    /// <param name="Object">Game object that its colliders will be drawn</param>
    /// <param name="Color">Colliders color</param>
    /// <param name="LifeTime">Drawing time</param>
    public void DrawObjectColliders(GameObject Object, Color Color, float LifeTime = 0.0f)
    {
        Collider[] CollidersArray = Object.GetComponentsInChildren<Collider>();
        foreach (var ColliderItem in CollidersArray)
        {
            // Check if box
            if (ColliderItem.GetType() == typeof(BoxCollider))
            {
                BoxCollider BoxColliderComp = (BoxCollider)ColliderItem;

                // Get parent scale
                Vector3 ParentScale = Vector3.one;
                if (ColliderItem.transform.parent != null)
                    ParentScale = ColliderItem.transform.parent.localScale;

                // Calculate position
                Vector3 ColliderPos = ColliderItem.transform.position +
                        ColliderItem.transform.right * BoxColliderComp.center.x * ParentScale.x +
                        ColliderItem.transform.forward * BoxColliderComp.center.z * ParentScale.z +
                        ColliderItem.transform.up * BoxColliderComp.center.y * ParentScale.y;

                // Calculate scaled size
                float SizeX = BoxColliderComp.size.x * BoxColliderComp.transform.localScale.x * ParentScale.x;
                float SizeY = BoxColliderComp.size.y * BoxColliderComp.transform.localScale.y * ParentScale.y;
                float SizeZ = BoxColliderComp.size.z * BoxColliderComp.transform.localScale.z * ParentScale.z;

                Vector3 BoxSize = new Vector3(SizeX, SizeY, SizeZ) * 0.5f;

                // Draw box
                DrawBox(ColliderPos, ColliderItem.transform.rotation, BoxSize, Color, LifeTime);
            }

            // Check if sphere
            if (ColliderItem.GetType() == typeof(SphereCollider))
            {
                SphereCollider SphereColliderComp = (SphereCollider)ColliderItem;

                // Get parent scale
                Vector3 ParentScale = Vector3.one;
                if (ColliderItem.transform.parent != null)
                    ParentScale = ColliderItem.transform.parent.localScale;

                // Calculate position
                Vector3 ColliderPos = ColliderItem.transform.position +
                    ColliderItem.transform.right * SphereColliderComp.center.x * ParentScale.x +
                    ColliderItem.transform.forward * SphereColliderComp.center.z * ParentScale.z +
                    ColliderItem.transform.up * SphereColliderComp.center.y * ParentScale.y;

                float ScaledRadius = SphereColliderComp.bounds.extents.x;

                // Draw shere
                DrawSphere(ColliderPos, ColliderItem.transform.rotation, ScaledRadius, 8, Color, LifeTime);
            }

            // Check if capsule
            if (ColliderItem.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider CapsuleColliderComp = (CapsuleCollider)ColliderItem;

                // Get parent scale
                Vector3 ParentScale = Vector3.one;
                if (ColliderItem.transform.parent != null)
                    ParentScale = ColliderItem.transform.parent.localScale;
                
                // Calculate position
                Vector3 ColliderPos = ColliderItem.transform.position +
                    ColliderItem.transform.right * CapsuleColliderComp.center.x * ParentScale.x +
                    ColliderItem.transform.forward * CapsuleColliderComp.center.z * ParentScale.z +
                    ColliderItem.transform.up * CapsuleColliderComp.center.y * ParentScale.y;

                float ScaledRadius = CapsuleColliderComp.radius * ColliderItem.transform.lossyScale.x;
                float ScaledHeight = CapsuleColliderComp.height * 0.5f * ColliderItem.transform.lossyScale.y;

                // Draw shere
                DrawCapsule(ColliderPos, ScaledHeight, ScaledRadius, ColliderItem.transform.rotation, Color, LifeTime);
            }
        }
    }

    /// <summary>
    /// Draw bounds
    /// </summary>
    /// <param name="InBounds">Bounds to draw</param>
    /// <param name="Color">Drawing colorolor</param>
    /// <param name="LifeTime">Draw life time</param>
    public void DrawBounds(Bounds InBounds, Color Color, float LifeTime = 0.0f)
    {
        DrawBox(InBounds.center, Quaternion.identity, InBounds.extents, Color, LifeTime);
    }
    
    #region Private Internal Functions
    private void InternalDrawLine(Vector3 LineStart, Vector3 LineEnd, Vector3 Center, Quaternion Rotation, Color Color, float LifeTime = 0.0f)
    {
        m_BatchedLines.Add(new BatchedLine(LineStart, LineEnd, Center, Rotation, Color, LifeTime));
    }

    private void InternalDrawCapsuleCircle(Vector3 Base, Vector3 X, Vector3 Z, Color Color, float Radius, int Segments, float LifeTime = 0.0f)
    {
        float AngleDelta = 2.0f * Mathf.PI / Segments;
        Vector3 LastPoint = Base + X * Radius;

        for (int i = 0; i < Segments; i++)
        {
            Vector3 Point = Base + (X * Mathf.Cos(AngleDelta * (i + 1)) + Z * Mathf.Sin(AngleDelta * (i + 1))) * Radius;
            InternalDrawLine(LastPoint, Point, Base, Quaternion.identity, Color, LifeTime);
            LastPoint = Point;
        }
    }

    private void InternalDrawCamera(Camera Camera, Color Color, float LifeTime = 0.0f)
    {
        if (Camera == null) return;

        float CamBoxDepth = 0.4f;
        float CamBoxHeight = 0.3f;
        float CamBoxWidth = 0.2f;
        float CamCylRadius = 0.25f;
        float CamCylDistance = 0.55f;

        Vector3 CamPos = Camera.transform.position;
        Quaternion CamRot = Camera.transform.rotation;

        // Box
        DrawBox(CamPos, CamRot, new Vector3(CamBoxWidth, CamBoxHeight, CamBoxDepth), Color, LifeTime);

        // Two cylinders
        Vector3 V1 = CamPos + new Vector3(CamBoxWidth / 2.0f, (CamBoxHeight) + CamCylRadius, -CamCylDistance / 2.0f);
        Vector3 V2 = CamPos + new Vector3(-CamBoxWidth / 2.0f, (CamBoxHeight) + CamCylRadius, -CamCylDistance / 2.0f);

        InternalDrawCylinder(V1, V2, CamRot, CamPos, CamCylRadius, 8, Color, LifeTime);
        V1 += new Vector3(0.0f, 0.0f, CamCylDistance);
        V2 += new Vector3(0.0f, 0.0f, CamCylDistance);
        InternalDrawCylinder(V1, V2, CamRot, CamPos, CamCylRadius, 8, Color, LifeTime);

        // Zoom
        Vector3 Extent = new Vector3(CamBoxWidth * 0.7f, CamBoxHeight * 0.7f, CamBoxDepth * 0.7f);
        Vector3 Center = CamPos + new Vector3(0.0f, 0.0f, CamBoxDepth);

        InternalDrawLine(Center + new Vector3(Extent.x, Extent.y, 0.0f), Center + new Vector3(Extent.x, -Extent.y, 0.0f), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(Extent.x, -Extent.y, 0.0f), Center + new Vector3(-Extent.x, -Extent.y, 0.0f), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, -Extent.y, 0.0f), Center + new Vector3(-Extent.x, Extent.y, 0.0f), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, Extent.y, 0.0f), Center + new Vector3(Extent.x, Extent.y, 0.0f), CamPos, CamRot, Color, LifeTime);

        float ZoomDepth = CamBoxDepth;
        float v = 3.0f;
        InternalDrawLine(Center + new Vector3(Extent.x * v, Extent.y * v, ZoomDepth), Center + new Vector3(Extent.x * v, -Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(Extent.x * v, -Extent.y * v, ZoomDepth), Center + new Vector3(-Extent.x * v, -Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x * v, -Extent.y * v, ZoomDepth), Center + new Vector3(-Extent.x * v, Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x * v, Extent.y * v, ZoomDepth), Center + new Vector3(Extent.x * v, Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);

        InternalDrawLine(Center + new Vector3(Extent.x, Extent.y, 0.0f), Center + new Vector3(Extent.x * v, Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(Extent.x, -Extent.y, 0.0f), Center + new Vector3(Extent.x * v, -Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, -Extent.y, 0.0f), Center + new Vector3(-Extent.x * v, -Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
        InternalDrawLine(Center + new Vector3(-Extent.x, Extent.y, 0.0f), Center + new Vector3(-Extent.x * v, Extent.y * v, ZoomDepth), CamPos, CamRot, Color, LifeTime);
    }

    private void InternalDrawCylinder(Vector3 Start, Vector3 End, Quaternion Rotation, Vector3 Center, float Radius, int Segments, Color Color, float LifeTime = 0.0f)
    {
        Segments = Mathf.Max(Segments, 4);

        Vector3 CylinderUp = ((End + new Vector3(0.0f, 0.0f, 0.01f)) - Start).normalized;
        Vector3 CylinderRight = Vector3.Cross(Vector3.up, CylinderUp).normalized;
        Vector3 CylinderForward = Vector3.Cross(CylinderRight, CylinderUp).normalized;
        float CylinderHeight = (End - Start).magnitude;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        // Debug End
        float Angle = 0.0f;
        Vector3 P_1;
        Vector3 P_2;
        Vector3 P_3;
        Vector3 P_4;

        Vector3 RotatedVect;
        for (int i = 0; i < Segments; i++)
        {
            RotatedVect = Quaternion.AngleAxis(Mathf.Rad2Deg * Angle, CylinderUp) * CylinderRight * Radius;

            P_1 = Start + RotatedVect;
            P_2 = P_1 + CylinderUp * CylinderHeight;

            // Draw lines
            InternalDrawLine(P_1, P_2, Center, Rotation, Color, LifeTime);

            Angle += AngleInc;
            RotatedVect = Quaternion.AngleAxis(Mathf.Rad2Deg * Angle, CylinderUp) * CylinderRight * Radius;

            P_3 = Start + RotatedVect;
            P_4 = P_3 + CylinderUp * CylinderHeight;

            // Draw lines
            InternalDrawLine(P_1, P_3, Center, Rotation, Color, LifeTime);
            InternalDrawLine(P_2, P_4, Center, Rotation, Color, LifeTime);
        }
    }

    private void InternalAddDebugText(string Text, TextAnchor Anchor, Vector3 Position, Quaternion Rotation, Color Color, float Size, float LifeTime, bool Is2DText)
    {
        m_DebugTextesList.Add(new DebugText(Text, Anchor, Position, Rotation, Color, Size, LifeTime, Is2DText));
    }

    private void InternalDrawHalfCircle(Vector3 Base, Vector3 X, Vector3 Z, Color Color, float Radius, int Segments, float LifeTime = 0.0f)
    {
        float AngleDelta = 2.0f * Mathf.PI / Segments;
        Vector3 LastPoint = Base + X * Radius;

        for (int i = 0; i < (Segments / 2); i++)
        {
            Vector3 Point = Base + (X * Mathf.Cos(AngleDelta * (i + 1)) + Z * Mathf.Sin(AngleDelta * (i + 1))) * Radius;
            InternalDrawLine(LastPoint, Point, Base, Quaternion.identity, Color, LifeTime);
            LastPoint = Point;
        }
    }

    #endregion
    #endregion

    #region ========== Handle Drawing Lines/Quads ==========
    private void HandleDrawingListOfLines()
    {
        if (m_BatchedLines.Count == 0)
            return;

        // Check material is set
        if (!m_LineMaterial)
        {
            InitializeMaterials();
        }

        // Draw lines
        GL.PushMatrix();
        m_LineMaterial.SetPass(0);
        GL.Begin(GL.LINES);

        // Set projection matrix
        Matrix4x4 M = Camera.main.projectionMatrix;

        for (int i = 0; i < m_BatchedLines.Count; i++)
        {
            M.SetTRS(Vector3.zero, m_BatchedLines[i].Rotation, Vector3.one);

            Vector3 S = m_BatchedLines[i].Start - m_BatchedLines[i].PivotPoint;
            Vector3 E = m_BatchedLines[i].End - m_BatchedLines[i].PivotPoint;

            Vector3 ST = M.MultiplyPoint(S);
            Vector3 ET = M.MultiplyPoint(E);

            ST += m_BatchedLines[i].PivotPoint;
            ET += m_BatchedLines[i].PivotPoint;

            GL.Color(m_BatchedLines[i].Color);
            GL.Vertex(ST);
            GL.Vertex(ET);
        }

        GL.End();

        GL.PopMatrix();

        // Update lines
        for (int i = m_BatchedLines.Count - 1; i >= 0; i--)
        {
            m_BatchedLines[i].RemainLifeTime -= Time.deltaTime;
            if (m_BatchedLines[i].RemainLifeTime <= 0.0f)
            {
                m_BatchedLines.RemoveAt(i);
            }
        }
    }

    private void HandleDrawingListOfTextes()
    {
        // Filter 2D textes
        List<DebugText> DebugText2DList = new List<DebugText>();
        List<DebugText> DebugText3DList = new List<DebugText>();
        for (int i = 0; i < m_DebugTextesList.Count; i++)
        {
            if (m_DebugTextesList[i].m_Is2DText)
            {
                DebugText2DList.Add(m_DebugTextesList[i]);
            }
            else
            {
                DebugText3DList.Add(m_DebugTextesList[i]);
            }
        }

        // Draw 3D text
        GL.PushMatrix();
        m_Debug3DTextFont.material.SetPass(0);
        GL.Begin(GL.QUADS);
        Vector3 V_1, V_2, V_3, V_4;

        for (int i = 0; i < DebugText3DList.Count; i++)
        {
            GL.Color(DebugText3DList[i].m_TextColor);
            Vector3 OriginPosition = DebugText3DList[i].GetTextOriginPosition(m_Debug3DTextFont);
            float Size = DebugText3DList[i].m_Size;
            Matrix4x4 M = Matrix4x4.identity;
            M.SetTRS(Vector3.zero, DebugText3DList[i].m_TextRotation, Vector3.one);
            Vector3 CharPos = OriginPosition;
            for (int j = 0; j < DebugText3DList[i].m_TextString.Length; j++)
            {
                char Char = DebugText3DList[i].m_TextString.ToCharArray()[j];

                CharacterInfo CharInfos;
                m_Debug3DTextFont.GetCharacterInfo(Char, out CharInfos);

                V_1 = CharPos - new Vector3(CharInfos.minX, CharInfos.minY, 0.0f) * Size;
                V_2 = CharPos + new Vector3(CharInfos.minX, CharInfos.maxY, 0.0f) * Size;
                V_3 = CharPos + new Vector3(-CharInfos.maxX, CharInfos.maxY, 0.0f) * Size;
                V_4 = CharPos + new Vector3(-CharInfos.maxX, CharInfos.minY, 0.0f) * Size;

                V_1 -= DebugText3DList[i].m_TextPosition;
                V_2 -= DebugText3DList[i].m_TextPosition;
                V_3 -= DebugText3DList[i].m_TextPosition;
                V_4 -= DebugText3DList[i].m_TextPosition;

                V_1 = M.MultiplyPoint(V_1);
                V_2 = M.MultiplyPoint(V_2);
                V_3 = M.MultiplyPoint(V_3);
                V_4 = M.MultiplyPoint(V_4);

                V_1 += DebugText3DList[i].m_TextPosition;
                V_2 += DebugText3DList[i].m_TextPosition;
                V_3 += DebugText3DList[i].m_TextPosition;
                V_4 += DebugText3DList[i].m_TextPosition;

                GL.TexCoord(CharInfos.uvBottomLeft);
                GL.Vertex(V_1);

                GL.TexCoord(CharInfos.uvTopLeft);
                GL.Vertex(V_2);

                GL.TexCoord(CharInfos.uvTopRight);
                GL.Vertex(V_3);

                GL.TexCoord(CharInfos.uvBottomRight);
                GL.Vertex(V_4);

                CharPos -= new Vector3(CharInfos.advance * Size, 0.0f, 0.0f);
            }

        }
        GL.End();
        GL.PopMatrix();


        // Draw 2D text
        GL.PushMatrix();
        m_Debug2DTextFont.material.SetPass(0);
        GL.Begin(GL.QUADS);
        for (int i = 0; i < DebugText2DList.Count; i++)
        {
            GL.LoadPixelMatrix();

            GL.Color(DebugText2DList[i].m_TextColor);
            Vector3 OriginPosition = DebugText2DList[i].GetTextOriginPosition(m_Debug2DTextFont);

            for (int j = 0; j < DebugText2DList[i].m_TextString.Length; j++)
            {
                char Char = DebugText2DList[i].m_TextString.ToCharArray()[j];

                CharacterInfo CharInfos;
                m_Debug2DTextFont.GetCharacterInfo(Char, out CharInfos);

                GL.TexCoord(CharInfos.uvBottomLeft);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.minX, CharInfos.minY, 0.0f));

                GL.TexCoord(CharInfos.uvTopLeft);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.minX, CharInfos.maxY, 0.0f));

                GL.TexCoord(CharInfos.uvTopRight);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.maxX, CharInfos.maxY, 0.0f));

                GL.TexCoord(CharInfos.uvBottomRight);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.maxX, CharInfos.minY, 0.0f));
                OriginPosition += new Vector3(CharInfos.advance, 0.0f, 0.0f);
            }

        }
        GL.End();
        GL.PopMatrix();

        // Update text life time
        for (int i = m_DebugTextesList.Count - 1; i >= 0; i--)
        {
            m_DebugTextesList[i].m_RemainLifeTime -= Time.deltaTime;
            if (m_DebugTextesList[i].m_RemainLifeTime <= 0.0f)
            {
                m_DebugTextesList.RemoveAt(i);
            }
        }
    }

    private void HandleDrawingListOfFloatGraphs()
    {
        Vector3 OriginPosition = Vector3.zero;
        DebugFloatGraph[] FloatGraphsArray = m_FloatGraphsList.ToArray();

        // Draw background
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        m_AlphaMaterial.SetPass(0);
        GL.Color(new Color(0.3f, 1.0f, 0.2f, 0.1f));
        GL.LoadPixelMatrix();

        int GraphPosIndexX = 1;
        int GraphPosIndexY = 0;
        for (int i = 0; i < FloatGraphsArray.Length; i++)
        {
            // Set origin position
            float TargetPosY = (m_GraphMargin_Buttom + m_GraphHeight + m_GraphMargin_Top) * GraphPosIndexY;
            if (TargetPosY > Screen.height - (m_GraphHeight + m_GraphMargin_Buttom))
            {
                GraphPosIndexX++;
                GraphPosIndexY = 0;
            }
            OriginPosition = new Vector3(Screen.width - (m_GraphWidth + m_GraphMargin_Right) * GraphPosIndexX, m_GraphMargin_Buttom + (m_GraphHeight + m_GraphMargin_Top) * GraphPosIndexY);
            GraphPosIndexY++;

            // Draw text
            float TextButtomMargin = 5.0f;

            // Draw graph title
            DrawString2D(OriginPosition + new Vector3(0.0f, m_GraphHeight + TextButtomMargin, 0.0f), FloatGraphsArray[i].m_UniqueFloatName, TextAnchor.LowerLeft, Color.white, 0.0f);

            // Draw min and max values
            InternalAddDebugText(FloatGraphsArray[i].m_GraphValueLengh.ToString(), TextAnchor.UpperLeft, OriginPosition + new Vector3(2.0f, m_GraphHeight, 0.0f), Quaternion.identity, Color.white * new Color(1.0f, 1.0f, 1.0f, m_GraphTextAlpha), 1.0f, 0.0f, true);
            InternalAddDebugText((-FloatGraphsArray[i].m_GraphValueLengh).ToString(), TextAnchor.LowerLeft, OriginPosition + new Vector3(2.0f, 2.0f, 0.0f), Quaternion.identity, Color.white * new Color(1.0f, 1.0f, 1.0f, m_GraphTextAlpha), 1.0f, 0.0f, true);

            // Draw bg points
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex(OriginPosition);

            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex(OriginPosition + new Vector3(0.0f, m_GraphHeight, 0.0f));

            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex(OriginPosition + new Vector3(m_GraphWidth, m_GraphHeight, 0.0f));

            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex(OriginPosition + new Vector3(m_GraphWidth, 0.0f, 0.0f));
        }

        GL.End();

        // Draw lines to show float value
        GL.Begin(GL.LINES);

        float GraphStepX;
        float GraphStepY;
        float OffsetY;

        GraphPosIndexX = 1;
        GraphPosIndexY = 0;
        for (int i = 0; i < FloatGraphsArray.Length; i++)
        {
            // Set origin position
            float TargetPosY = (m_GraphMargin_Buttom + m_GraphHeight + m_GraphMargin_Top) * GraphPosIndexY;
            if (TargetPosY > Screen.height - (m_GraphHeight + m_GraphMargin_Buttom))
            {
                GraphPosIndexX++;
                GraphPosIndexY = 0;
            }
            OriginPosition = new Vector3(Screen.width - (m_GraphWidth + m_GraphMargin_Right) * GraphPosIndexX, m_GraphMargin_Buttom + (m_GraphHeight + m_GraphMargin_Top) * GraphPosIndexY);
            GraphPosIndexY++;

            GraphStepX = m_GraphWidth / FloatGraphsArray[i].m_SamplesCount;
            GraphStepY = (m_GraphHeight / 2.0f) / FloatGraphsArray[i].m_GraphValueLengh;

            OffsetY = (m_GraphHeight / 2.0f);

            // Draw line in the half of the graph = 0
            GL.Color(new Color(0.3f, 1.0f, 0.2f, 0.2f));

            GL.Vertex(new Vector3(OriginPosition.x, OriginPosition.y + m_GraphHeight / 2.0f));
            GL.Vertex(new Vector3(OriginPosition.x + m_GraphWidth, OriginPosition.y + m_GraphHeight / 2.0f));

            // Draw curve
            if (m_FloatGraphsList[i].m_FloatValuesList.Count > 0)
            {
                GL.Color(new Color(1.0f, 1.0f, 0.2f, 0.9f));

                Vector3 LineStart = Vector3.zero;
                Vector3 LineEnd = Vector3.zero;
                float FloatValue = 0.0f;
                float X;
                float Y;

                for (int j = 0; j < m_FloatGraphsList[i].m_FloatValuesList.Count; j++)
                {
                    if (j == 0)
                    {
                        FloatValue = m_FloatGraphsList[i].m_FloatValuesList[j];
                        LineStart = OriginPosition + new Vector3(0.0f, Mathf.Clamp(OffsetY + FloatValue * GraphStepY, 0.0f, m_GraphHeight), 0.0f);
                    }

                    if (j < m_FloatGraphsList[i].m_FloatValuesList.Count - 1)
                        FloatValue = m_FloatGraphsList[i].m_FloatValuesList[j + 1];

                    X = (j + 1) * GraphStepX;
                    Y = Mathf.Clamp(OffsetY + FloatValue * GraphStepY, 0.0f, m_GraphHeight);

                    LineEnd = OriginPosition + new Vector3(X, Y, 0.0f);

                    GL.Vertex(LineStart);
                    GL.Vertex(LineEnd);

                    LineStart = LineEnd;
                }

                // draw value text
                InternalAddDebugText((FloatValue).ToString(".000"), FloatValue >= 0.0f ? TextAnchor.UpperRight : TextAnchor.LowerRight, new Vector3(OriginPosition.x + m_GraphWidth, Mathf.Clamp(LineEnd.y, OriginPosition.y, OriginPosition.y + m_GraphHeight), 0.0f), Quaternion.identity, Color.white * new Color(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, 0.0f, true);
            }
        }
        GL.End();
        GL.PopMatrix();
                
        // Update text life time
        for (int i = m_FloatGraphsList.Count - 1; i >= 0; i--)
        {
            m_FloatGraphsList[i].m_TimeBeforeRemoveCounter += Time.deltaTime;
            if (m_FloatGraphsList[i].m_TimeBeforeRemoveCounter >= m_FloatGraphsList[i].m_TimeBeforeRemove)
            {
                m_FloatGraphsList.RemoveAt(i);
            }
        }
    }

    private void HandleListOfLogMessagesList()
    {
        Vector3 OriginPosition = new Vector3(m_LogMessageMarginLeft, Screen.height - m_LogMessageMarginTop, 0.0f);

        for (int i = m_LogMessagesList.Count-1; i >= 0; i--)
        {
            Vector3 LogPosition = OriginPosition - new Vector3(0.0f, m_LineSpacing * i, 0.0f);
            if (LogPosition.y > 0.0f)
                DrawString2D(LogPosition, m_LogMessagesList[i].m_LogMessageText, TextAnchor.LowerLeft, m_LogMessagesList[i].m_Color, 0.0f);
        }

        // Update log life time
        for (int i = m_LogMessagesList.Count - 1; i >= 0; i--)
        {
            m_LogMessagesList[i].m_RemainingTime -= Time.unscaledDeltaTime;
            if (m_LogMessagesList[i].m_RemainingTime <= 0.0f)
            {
                m_LogMessagesList.RemoveAt(i);
            }
        }
    }
    #endregion

    #region ========== Helper Functions ==========
    private Vector3 GetIntersectionPointOfPlanes(Plane Plane_1, Plane Plane_2, Plane Plane_3)
    {
        return ((-Plane_1.distance * Vector3.Cross(Plane_2.normal, Plane_3.normal)) +
                (-Plane_2.distance * Vector3.Cross(Plane_3.normal, Plane_1.normal)) +
                (-Plane_3.distance * Vector3.Cross(Plane_1.normal, Plane_2.normal))) /
            (Vector3.Dot(Plane_1.normal, Vector3.Cross(Plane_2.normal, Plane_3.normal)));
    }

    public void FlushDebugLines()
    {
        // Delete all lines
        for (int i = m_BatchedLines.Count - 1; i >= 0; i--)
        {
            m_BatchedLines.RemoveAt(i);
        }
    } 
    #endregion
}


#region ========== Enums / Structures / Helper Classes ==========
[System.Serializable]
public class BatchedLine
{
    public Vector3 Start;
    public Vector3 End;
    public Vector3 PivotPoint;
    public Quaternion Rotation;
    public Color Color;
    public float RemainLifeTime;

    public BatchedLine(Vector3 InStart, Vector3 InEnd, Vector3 InPivotPoint, Quaternion InRotation, Color InColor, float InRemainLifeTime)
    {
        Start = InStart;
        End = InEnd;
        PivotPoint = InPivotPoint;
        Rotation = InRotation;
        Color = InColor;
        RemainLifeTime = InRemainLifeTime;
    }
};

public enum EDrawPlaneAxis
{
    XZ,
    XY,
    YZ
};

[System.Serializable]
public class DebugFloatGraph
{
    public string m_UniqueFloatName = "";
    public int m_SamplesCount = 0;
    public List<float> m_FloatValuesList;
    public float m_GraphValueLengh;
    public float m_MinValue;
    public float m_MaxValue;
    public bool m_AutoAdjustMinMaxRange = false;
    public float m_TimeBeforeRemove;
    public float m_TimeBeforeRemoveCounter = 0.0f;
    private int m_Y, m_X;
    public DebugFloatGraph()
    {
    }

    public DebugFloatGraph(string UniqueFloatName, int SamplesCount, float GraphValueLengh, bool AutoAdjustMinMaxRange, float TimeBeforeRemove)
    {
        m_UniqueFloatName = UniqueFloatName;
        m_SamplesCount = SamplesCount;
        m_FloatValuesList = new List<float>();
        m_GraphValueLengh = GraphValueLengh;
        m_AutoAdjustMinMaxRange = AutoAdjustMinMaxRange;
        m_TimeBeforeRemove = TimeBeforeRemove;
        m_Y = 0;
        m_X = 1;
    }

    public void AddValue(float NewFloatVal)
    {
        // Add value
        m_FloatValuesList.Add(NewFloatVal);

        // Set min and max
        if (Mathf.Abs(GetMaximumValue()) > Mathf.Abs(GetMinimumValue()))
        {
            m_MaxValue = Mathf.Abs(GetMaximumValue());
            m_MinValue = -Mathf.Abs(GetMaximumValue());
        }
        else
        {
            m_MaxValue = Mathf.Abs(GetMinimumValue());
            m_MinValue = -Mathf.Abs(GetMinimumValue());
        }

        // Auto adjust graph length
        if (m_AutoAdjustMinMaxRange)
        {
            if (Mathf.Abs(NewFloatVal) > m_GraphValueLengh)
            {
                m_GraphValueLengh = Mathf.Abs(NewFloatVal);
            }
        }

        // Remove first float from the array if we rech max samples
        if (GetDidReachMaxSamples())
            m_FloatValuesList.RemoveAt(0);

        // Reset counter
        m_TimeBeforeRemoveCounter = 0.0f;
    }

    public bool GetDidReachMaxSamples()
    {
        return m_FloatValuesList.Count > m_SamplesCount;
    }

    public float GetMinimumValue()
    {
        float SmallestValue = Mathf.Infinity;
        for (int i = 0; i < m_FloatValuesList.Count; i++)
        {
            if (m_FloatValuesList[i] < SmallestValue)
            {
                SmallestValue = m_FloatValuesList[i];
            }
        }
        return SmallestValue;
    }

    public float GetMaximumValue()
    {
        float BiggestValue = -Mathf.Infinity;
        for (int i = 0; i < m_FloatValuesList.Count; i++)
        {
            if (m_FloatValuesList[i] > BiggestValue)
            {
                BiggestValue = m_FloatValuesList[i];
            }
        }
        return BiggestValue;
    }
}

[System.Serializable]
public class DebugLogMessage
{
    public string   m_LogMessageText;
    public Color    m_Color;
    public float    m_RemainingTime;

    public DebugLogMessage(string LogMessageText, Color Color, float LifeTime)
    {
        m_LogMessageText = LogMessageText;
        m_Color = Color;
        m_RemainingTime = LifeTime;
    }
}

[System.Serializable]
public class DebugText
{
    public string m_TextString;
    public TextAnchor m_TextAnchor;
    public Vector3 m_TextPosition;
    public Quaternion m_TextRotation;
    public Color m_TextColor;
    public float m_Size;
    public float m_RemainLifeTime;
    public bool m_Is2DText;

    public DebugText()
    {
    }

    public DebugText(string Text, TextAnchor TextAnchor, Vector3 TextPosition, Quaternion TextRotation, Color Color, float Size, float LifeTime, bool Is2DText)
    {
        m_TextString = Text;
        m_TextAnchor = TextAnchor;
        m_TextPosition = TextPosition;
        m_TextRotation = TextRotation;
        m_TextColor = Color;
        m_Size = Size;
        m_RemainLifeTime = LifeTime;
        m_Is2DText = Is2DText;
    }

    // Get text anchored position
    public Vector3 GetTextOriginPosition(Font TextFont)
    {
        Vector3 OriginPos = m_TextPosition;

        float TextWidth = 0.0f;
        float TextHeight = 0.0f;

        // Get text size
        char[] CharsArray = m_TextString.ToCharArray();
        for (int i = 0; i < CharsArray.Length; i++)
        {
            CharacterInfo CharInfos;
            TextFont.GetCharacterInfo(CharsArray[i], out CharInfos);
            TextWidth += CharInfos.advance * m_Size;
            if (i == 0)
                TextHeight = CharInfos.glyphHeight * m_Size;
        }

        switch (m_TextAnchor)
        {
            case TextAnchor.UpperLeft:
                OriginPos += new Vector3(0.0f, -TextHeight, 0.0f);
                break;
            case TextAnchor.UpperCenter:
                OriginPos += new Vector3(TextWidth / 2.0f, -TextHeight, 0.0f);
                break;
            case TextAnchor.UpperRight:
                OriginPos += new Vector3(-TextWidth, -TextHeight, 0.0f);
                break;
            case TextAnchor.MiddleLeft:
                OriginPos += new Vector3(0.0f, -TextHeight / 2.0f, 0.0f);
                break;
            case TextAnchor.MiddleCenter:
                OriginPos += new Vector3(TextWidth / 2.0f, -TextHeight / 2.0f, 0.0f);
                break;
            case TextAnchor.MiddleRight:
                OriginPos += new Vector3(-TextWidth, -TextHeight / 2.0f, 0.0f);
                break;
            case TextAnchor.LowerLeft:
                // Default position
                break;
            case TextAnchor.LowerCenter:
                OriginPos += new Vector3(TextWidth / 2.0f, 0.0f, 0.0f);
                break;
            case TextAnchor.LowerRight:
                OriginPos += new Vector3(-TextWidth, 0.0f, 0.0f);
                break;
            default:
                break;
        }
        return OriginPos;
    }
}

public class DebugCamera : MonoBehaviour
{
    private void OnPostRender()
    {
        DrawDebugTools.Instance.Draw();
    }
}
#endregion
