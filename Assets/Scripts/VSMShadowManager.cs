using UnityEngine;

[ExecuteInEditMode]
public class VSMShadowManager : MonoBehaviour
{
    [Header("光照设置")]
    public Light directionalLight;
    public LayerMask shadowCasterLayers = -1;
    
    [Header("阴影贴图设置")]
    public int shadowMapResolution = 1024;
    public float shadowMapSize = 10f;
    public Material shadowMapMaterial;
    public Material[] receiverMaterials;
    

    
    [Header("阴影参数")]
    [Range(0, 1)]
    public float shadowStrength = 0.7f;
    [Range(0.00001f, 0.01f)]
    public float minVariance = 0.00001f;
    [Range(0, 1)]
    public float lightBleedingReduction = 0.2f;
    [Range(0, 0.1f)]
    public float depthBias = 0.01f;

    public RenderTexture shadowMap;
    public Camera shadowCamera;

    void OnEnable()
    {
        SetupShadowCamera();
        CreateShadowMap();
    }

    void OnDisable()
    {
        ReleaseShadowMap();
        if (shadowCamera != null)
        {
            DestroyImmediate(shadowCamera.gameObject);
            shadowCamera = null;
        }
    }
    
    private void SetupShadowCamera()
    {
        if (shadowCamera == null)
        {
            GameObject shadowCamObj = new GameObject("VSM Shadow Camera");
            //shadowCamObj.hideFlags = HideFlags.HideAndDontSave;
            shadowCamObj.transform.parent = transform;
            shadowCamera = shadowCamObj.AddComponent<Camera>();
            shadowCamera.enabled = false;
            shadowCamera.orthographic = true;
            shadowCamera.clearFlags = CameraClearFlags.SolidColor;
            shadowCamera.backgroundColor = new Color(1, 1, 1, 1);
            shadowCamera.cullingMask = shadowCasterLayers;
            shadowCamera.nearClipPlane = 0.1f;
            shadowCamera.farClipPlane = 100f;
        }
    }
    


    void CreateShadowMap()
    {
        if (shadowMap != null)
        {
            shadowMap.Release();
        }

        // 创建阴影贴图
        try
        {
            shadowMap = new RenderTexture(shadowMapResolution, shadowMapResolution, 24, RenderTextureFormat.RGFloat);
            shadowMap.filterMode = FilterMode.Bilinear;
            shadowMap.wrapMode = TextureWrapMode.Clamp;
            shadowMap.name = "VSM_ShadowMap";
            shadowMap.Create();
            
            Debug.Log("阴影贴图创建成功 - 分辨率: " + shadowMapResolution + "x" + shadowMapResolution);
            

        }
        catch (System.Exception e)
        {
            Debug.LogError("创建阴影贴图失败: " + e.Message);
        }
    }

    void ReleaseShadowMap()
    {
        if (shadowMap != null)
        {
            shadowMap.Release();
            shadowMap = null;
        }


    }

    void Start()
    {
        // 确保在开始时就创建好所有资源
        SetupShadowCamera();
        CreateShadowMap();
        
        Debug.Log("VSM阴影系统初始化完成");
    }

    void LateUpdate()
    {
        if (directionalLight == null || shadowMapMaterial == null)
        {
            return;
        }
        RenderShadowMap();
        UpdateReceiverMaterials();
    }
    
    private void RenderShadowMap()
    {
        // 计算场景边界，确保阴影相机能覆盖所有物体
        Bounds sceneBounds = CalculateSceneBounds();
        
        // 从光源方向看向场景中心
        Vector3 lightDirection = directionalLight.transform.forward;
        Vector3 sceneCenter = sceneBounds.center;
        
        // 将阴影相机放置在场景后方足够远的位置
        float cameraDistance = sceneBounds.size.magnitude + 10f;
        shadowCamera.transform.position = sceneCenter - lightDirection * cameraDistance;
        shadowCamera.transform.rotation = directionalLight.transform.rotation;

        // 设置正交相机的大小，确保能覆盖整个场景
        float orthoSize = Mathf.Max(sceneBounds.size.x, sceneBounds.size.z) * 0.7f;
        shadowCamera.orthographicSize = Mathf.Max(orthoSize, shadowMapSize);
        
        // 设置近远裁剪面，确保包含所有阴影投射物
        shadowCamera.nearClipPlane = 0.1f;
        shadowCamera.farClipPlane = cameraDistance + sceneBounds.size.magnitude + 5f;

        // 渲染阴影贴图
        if (shadowMap == null)
        {
            CreateShadowMap();
            if (shadowMap == null) return;
        }
        
        // 清除阴影贴图
        RenderTexture.active = shadowMap;
        GL.Clear(true, true, new Color(1, 1, 1, 1));
        RenderTexture.active = null;
        
        shadowCamera.targetTexture = shadowMap;
        shadowCamera.RenderWithShader(shadowMapMaterial.shader, "RenderType");
        
        // 调试信息（每60帧输出一次）
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"阴影相机设置 - 位置: {shadowCamera.transform.position}, 旋转: {shadowCamera.transform.rotation.eulerAngles}, 正交大小: {shadowCamera.orthographicSize}, 场景边界: {sceneBounds}");
        }
    }
    
    private Bounds CalculateSceneBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        bool boundsInitialized = false;
        
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.gameObject.activeInHierarchy)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        
        // 确保边界至少包含地面
        bounds.Encapsulate(new Vector3(-10, 0, -10));
        bounds.Encapsulate(new Vector3(10, 0, 10));
        
        return bounds;
    }
    

    
    private void UpdateReceiverMaterials()
    {
        // 计算从世界空间到光源空间的变换矩阵
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(shadowCamera.projectionMatrix, false);
        Matrix4x4 worldToShadowClip = proj * shadowCamera.worldToCameraMatrix;
        
        // 纹理空间变换矩阵：从[-1,1]到[0,1]
        Matrix4x4 texScaleBias = Matrix4x4.identity;
        texScaleBias.m00 = 0.5f; texScaleBias.m11 = 0.5f; texScaleBias.m22 = 0.5f;
        texScaleBias.m03 = 0.5f; texScaleBias.m13 = 0.5f; texScaleBias.m23 = 0.5f;
        
        Matrix4x4 worldToLight = texScaleBias * worldToShadowClip;

        // 更新接收阴影的材质
        if (receiverMaterials != null)
        {
            int validMaterialCount = 0;
            foreach (Material mat in receiverMaterials)
            {
                if (mat != null)
                {
                    // 使用原始阴影贴图
                    RenderTexture shadowTexture = shadowMap;
                    
                    if (shadowTexture == null)
                    {
                        Debug.LogError("阴影贴图为空，无法应用到材质");
                        continue;
                    }
                    
                    // 应用阴影贴图和参数到材质
                    mat.SetTexture("_ShadowMap", shadowTexture);
                    mat.SetMatrix("_ShadowMapWorldToLight", worldToLight);
                    mat.SetFloat("_ShadowStrength", shadowStrength);
                    mat.SetFloat("_MinVariance", minVariance);
                    mat.SetFloat("_LightBleedingReduction", lightBleedingReduction);
                    mat.SetFloat("_DepthBias", depthBias);
                    
                    // 确保着色器知道使用阴影贴图
                    mat.EnableKeyword("_SHADOWS_ON");
                    
                    validMaterialCount++;
                    
                    // 输出调试信息
                    if (Time.frameCount % 100 == 0 && validMaterialCount == 1)
                    {
                        Debug.Log("阴影贴图应用到材质: " + mat.name + 
                                  ", 阴影贴图: " + (shadowTexture != null ? shadowTexture.name : "null") +
                                  ", 阴影强度: " + shadowStrength);
                    }
                }
            }
            
            if (Time.frameCount % 100 == 0) // 每100帧输出一次调试信息
            {
                Debug.Log("已更新 " + validMaterialCount + " 个接收阴影的材质 - 阴影强度: " + shadowStrength + 
                          ", 最小方差: " + minVariance + 
                          ", 光线渗漏减少: " + lightBleedingReduction);
            }
        }
        else
        {
            Debug.LogWarning("没有接收阴影的材质");
        }
        
        // 更新所有渲染器，确保它们使用正确的材质
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // 强制刷新材质
                renderer.sharedMaterial.SetFloat("_DummyValue", Time.time);
            }
        }
    }
    
    // 创建VSM所需的材质
    public Material CreateShadowMapMaterial()
    {
        Shader shadowMapShader = Shader.Find("Unlit/VSMShadowMap");
        
        if (shadowMapShader != null)
        {
            Material mat = new Material(shadowMapShader);
            mat.name = "VSM Shadow Map Material";
            Debug.Log("阴影贴图着色器加载成功: " + shadowMapShader.name);
            return mat;
        }
        
        Debug.LogError("阴影贴图着色器 'Unlit/VSMShadowMap' 未找到！");
        return null;
    }
    
    // 创建接收阴影的材质
    public Material CreateReceiverMaterial()
    {
        Shader receiverShader = Shader.Find("Unlit/VSMLighting");
        
        if (receiverShader != null)
        {
            Material mat = new Material(receiverShader);
            mat.name = "VSM Receiver Material";
            Debug.Log("阴影接收着色器加载成功: " + receiverShader.name);
            return mat;
        }
        
        Debug.LogError("阴影接收着色器 'Unlit/VSMLighting' 未找到！");
        return null;
    }
    
    // 在Scene视图中绘制阴影相机的视锥体
    void OnDrawGizmos()
    {
        if (shadowCamera != null && directionalLight != null)
        {
            // 绘制阴影相机的视锥体
            Gizmos.color = Color.yellow;
            Gizmos.matrix = shadowCamera.transform.localToWorldMatrix;
            
            if (shadowCamera.orthographic)
            {
                float size = shadowCamera.orthographicSize;
                float near = shadowCamera.nearClipPlane;
                float far = shadowCamera.farClipPlane;
                
                // 绘制正交相机的视锥体
                Vector3[] nearCorners = new Vector3[]
                {
                    new Vector3(-size, -size, near),
                    new Vector3(size, -size, near),
                    new Vector3(size, size, near),
                    new Vector3(-size, size, near)
                };
                
                Vector3[] farCorners = new Vector3[]
                {
                    new Vector3(-size, -size, far),
                    new Vector3(size, -size, far),
                    new Vector3(size, size, far),
                    new Vector3(-size, size, far)
                };
                
                // 绘制近平面
                for (int i = 0; i < 4; i++)
                {
                    Gizmos.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);
                }
                
                // 绘制远平面
                for (int i = 0; i < 4; i++)
                {
                    Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % 4]);
                }
                
                // 绘制连接线
                for (int i = 0; i < 4; i++)
                {
                    Gizmos.DrawLine(nearCorners[i], farCorners[i]);
                }
            }
            
            Gizmos.matrix = Matrix4x4.identity;
            
            // 绘制光源方向
            Gizmos.color = Color.red;
            Vector3 lightPos = directionalLight.transform.position;
            Vector3 lightDir = directionalLight.transform.forward;
            Gizmos.DrawRay(lightPos, lightDir * 5f);
        }
    }
}