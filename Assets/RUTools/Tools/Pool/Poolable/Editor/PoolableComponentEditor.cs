﻿using UnityEditor;
using RUT.Editor;

namespace RUT.Tools.Pool
{
    [CustomEditor(typeof(PoolableComponent), true), CanEditMultipleObjects]
    public class PoolableComponentEditor : RUToolsEditor<PoolableComponent>
    {
    }
}