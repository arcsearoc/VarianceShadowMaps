# Unity Variance Shadow Maps (VSM) Implementation

[中文版本](README_CN.md) | **English Version**

Unity implementation of Variance Shadow Maps (VSM) technology, providing smooth shadow edge effects.

## Core Principles

VSM stores depth values and their squared values, using Chebyshev's inequality to calculate shadow probability, achieving soft shadow edges.

**Key Features:**

- RG floating-point texture stores depth and depth squared
- Chebyshev's inequality for shadow probability calculation
- Light bleeding reduction parameters

## Project Files

```
Assets/
├── Shaders/
│   ├── VSMShadowMap.shader    # Generate shadow maps
│   └── VSMLighting.shader     # VSM shadow rendering
└── Scripts/
    ├── VSMShadowManager.cs    # VSM manager
    ├── VSMDebugger.cs         # Debug tools
    └── VSMDemoController.cs   # Demo controller
```

## Quick Start

1. **Scene Setup**
   
   - Add a plane as ground
   - Place objects as shadow casters
   - Create directional light source

2. **VSM Configuration**
   
   - Create empty GameObject, add `VSMShadowManager` component
   - Run `VSMDemoController.CreateVSMTestScene()` to automatically create test scene

3. **Parameter Adjustment**
   
   - **Shadow Strength**: Shadow intensity (0-1)
   - **Min Variance**: Edge softness (0.00001-0.01)
   - **Light Bleeding Reduction**: Light bleeding reduction (0-1)
   - **Depth Bias**: Depth offset (0-0.1)

## Performance Optimization

- Reduce shadow map resolution
- Adjust shadow camera range
- Set reasonable parameter values
