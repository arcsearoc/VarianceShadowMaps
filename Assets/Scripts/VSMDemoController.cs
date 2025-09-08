using UnityEngine;
using System.Collections.Generic;

public class VSMDemoController : MonoBehaviour
{
    [Header("演示设置")]
    public Transform rotatingObject;
    public float rotationSpeed = 30f;
    public VSMShadowManager vsmManager;
    
    [Header("场景创建")]
    public bool autoCreateSceneOnStart = false;
    
    void Start()
    {
        if (autoCreateSceneOnStart)
        {
            CreateVSMTestScene();
        }
    }
    
    void Update()
    {
        // 旋转对象
        if (rotatingObject != null)
        {
            rotatingObject.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    [ContextMenu("Create VSM Test Scene")]
    public void CreateVSMTestScene()
    {
        // 确保有VSM管理器
        if (vsmManager == null)
        {
            vsmManager = GetComponent<VSMShadowManager>();
            if (vsmManager == null)
            {
                vsmManager = gameObject.AddComponent<VSMShadowManager>();
            }
        }
        
        // 创建材质
        if (vsmManager.shadowMapMaterial == null)
        {
            vsmManager.shadowMapMaterial = vsmManager.CreateShadowMapMaterial();
        }
        
        // --- 修复材质分配逻辑 ---
        // 创建一个列表来收集所有接收阴影的材质
        var receiverMaterials = new List<Material>();

        // 创建新光源（禁用Unity自带阴影）
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        light.shadows = LightShadows.None; // 禁用Unity自带阴影
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        vsmManager.directionalLight = light;
        
        // 创建地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5, 1, 5);
        // 为地面创建并应用新材质，然后将其添加到列表中
        ApplyMaterial(ground, vsmManager.CreateReceiverMaterial(), receiverMaterials);
        DisableDefaultLightingAndProbesSettings(ground);

        // 创建几个立方体作为阴影投射物
        GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube1.name = "Cube1";
        cube1.transform.position = new Vector3(0, 1, 0);
        ApplyMaterial(cube1, vsmManager.CreateReceiverMaterial(), receiverMaterials);
        DisableDefaultLightingAndProbesSettings(cube1);

        GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube2.name = "Cube2";
        cube2.transform.position = new Vector3(2, 1.5f, 2);
        cube2.transform.localScale = new Vector3(1, 2, 1);
        ApplyMaterial(cube2, vsmManager.CreateReceiverMaterial(), receiverMaterials);
        DisableDefaultLightingAndProbesSettings(cube2);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere";
        sphere.transform.position = new Vector3(-2, 1, 2);
        ApplyMaterial(sphere, vsmManager.CreateReceiverMaterial(), receiverMaterials);
        DisableDefaultLightingAndProbesSettings(sphere);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "Cylinder";
        cylinder.transform.position = new Vector3(2, 1, -2);
        ApplyMaterial(cylinder, vsmManager.CreateReceiverMaterial(), receiverMaterials);
        DisableDefaultLightingAndProbesSettings(cylinder);

        // 将收集到的所有材质告知VSM管理器
        vsmManager.receiverMaterials = receiverMaterials.ToArray();
        
        // 设置旋转对象
        rotatingObject = cube1.transform;
        
        // 创建相机
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.transform.position = new Vector3(0, 5, -10);
        cameraObj.transform.rotation = Quaternion.Euler(20, 0, 0);
        cameraObj.tag = "MainCamera";
        
        Debug.Log("VSM测试场景创建完成！");
    }
    
    // 应用材质到游戏对象，禁用Unity自带阴影，并将材质添加到列表中
    private void ApplyMaterial(GameObject obj, Material material, List<Material> materialList)
    {
        if (material != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
                materialList.Add(material); // 将新创建的材质实例添加到列表中
            }
        }
    }

    //禁用Unity自带的阴影投射和接收
    void DisableDefaultLightingAndProbesSettings(GameObject go)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion; 
        }
    }
}