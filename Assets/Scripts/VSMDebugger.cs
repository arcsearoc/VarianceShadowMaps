using UnityEngine;

[System.Serializable]
public class VSMDebugger : MonoBehaviour
{
    [Header("调试选项")]
    public bool showShadowMap = false;
    public bool showShadowCameraView = false;
    public VSMShadowManager vsmManager;
    
    [Header("GUI设置")]
    public int shadowMapDisplaySize = 256;
    
    void OnGUI()
    {
        if (vsmManager == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        
        GUILayout.Label("VSM阴影系统调试", GUI.skin.box);
        
        // 显示阴影贴图
        showShadowMap = GUILayout.Toggle(showShadowMap, "显示阴影贴图");
        if (showShadowMap && vsmManager.shadowMap != null)
        {
            GUILayout.Label("原始阴影贴图:");
            GUI.DrawTexture(new Rect(10, 80, shadowMapDisplaySize, shadowMapDisplaySize), vsmManager.shadowMap);
            

        }
        
        // 阴影参数调试
        GUILayout.Space(10);
        GUILayout.Label("阴影参数:");
        
        GUILayout.Label($"阴影强度: {vsmManager.shadowStrength:F2}");
        vsmManager.shadowStrength = GUILayout.HorizontalSlider(vsmManager.shadowStrength, 0f, 1f);
        
        GUILayout.Label($"最小方差: {vsmManager.minVariance:F6}");
        vsmManager.minVariance = GUILayout.HorizontalSlider(vsmManager.minVariance, 0.00001f, 0.01f);
        
        GUILayout.Label($"光线渗漏减少: {vsmManager.lightBleedingReduction:F2}");
        vsmManager.lightBleedingReduction = GUILayout.HorizontalSlider(vsmManager.lightBleedingReduction, 0f, 1f);
        
        GUILayout.Label($"深度偏移: {vsmManager.depthBias:F3}");
        vsmManager.depthBias = GUILayout.HorizontalSlider(vsmManager.depthBias, 0f, 0.1f);
        

        
        // 系统信息
        GUILayout.Space(10);
        GUILayout.Label("系统信息:", GUI.skin.box);
        
        if (vsmManager.shadowCamera != null)
        {
            GUILayout.Label($"阴影相机位置: {vsmManager.shadowCamera.transform.position}");
            GUILayout.Label($"正交大小: {vsmManager.shadowCamera.orthographicSize:F1}");
        }
        
        if (vsmManager.shadowMap != null)
        {
            GUILayout.Label($"阴影贴图: {vsmManager.shadowMap.width}x{vsmManager.shadowMap.height}");
            GUILayout.Label($"格式: {vsmManager.shadowMap.format}");
        }
        
        GUILayout.Label($"接收材质数量: {(vsmManager.receiverMaterials != null ? vsmManager.receiverMaterials.Length : 0)}");
        
        GUILayout.EndArea();
    }
    
    void Start()
    {
        if (vsmManager == null)
        {
            vsmManager = FindObjectOfType<VSMShadowManager>();
        }
    }
    
    void Update()
    {
        // 快捷键切换调试显示
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showShadowMap = !showShadowMap;
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showShadowCameraView = !showShadowCameraView;
        }
    }
}