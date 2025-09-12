# Unity方差阴影映射(VSM)实现

**中文版本** | [English Version](README.md)

Unity中的方差阴影映射(Variance Shadow Maps, VSM)技术实现，提供平滑的阴影边缘效果。

## 核心原理

VSM通过存储深度值及其平方值，使用Chebyshev不等式计算阴影概率，实现柔和的阴影边缘。

**主要特点：**

- RG浮点纹理存储深度和深度平方
- Chebyshev不等式计算阴影概率
- 光线渗漏减少参数

## 项目文件

```
Assets/
├── Shaders/
│   ├── VSMShadowMap.shader    # 生成阴影贴图
│   └── VSMLighting.shader     # VSM阴影渲染
└── Scripts/
    ├── VSMShadowManager.cs    # VSM管理器
    ├── VSMDebugger.cs         # 调试工具
    └── VSMDemoController.cs   # 演示控制器
```

## 快速开始

1. **场景设置**
   
   - 添加平面作为地面
   - 放置物体作为阴影投射物
   - 创建平行光源

2. **VSM配置**
   
   - 创建空物体，添加`VSMShadowManager`组件
   - 运行`VSMDemoController.CreateVSMTestScene()`自动创建测试场景

3. **参数调整**
   
   - **Shadow Strength**: 阴影强度 (0-1)
   - **Min Variance**: 边缘柔和度 (0.00001-0.01)
   - **Light Bleeding Reduction**: 光线渗漏减少 (0-1)
   - **Depth Bias**: 深度偏移 (0-0.1)

## 性能优化

- 降低阴影贴图分辨率
- 调整阴影相机范围
- 合理设置参数值
