﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace RUT.Editor
{
    /// <summary>
    /// RUTools editor helper class.
    /// </summary>
    public class RUToolsEditor<T> : UnityEditor.Editor
    {
        #region Private properties
        protected static readonly string _scriptFieldName = "m_Script";

        private static GUIStyle _logoStyle;
        private static GUIStyle _logoTextStyle;
        private static GUIStyle _separatorStyle;
        private static GUIStyle _logoBackgroundStyle;
        private static GUIStyle _mainBackgroundStyle;
        private static GUIStyle _titleBackgroundStyle;
        private static GUIStyle _additionalBackgroundStyle;
        private static GUIStyle _fieldButtonStyle;
        private static GUIStyle _fieldToggleStyle;
        private static readonly String _logoStyleName = "Logo";
        private static readonly String _logoTextStyleName = "LogoText";
        private static readonly String _separatorStyleName = "Separator";
        private static readonly String _logoBackgroundStyleName = "LogoBackground";
        private static readonly String _mainBackgroundStyleName = "MainBackground";
        private static readonly String _titleBackgroundStyleName = "TitleBackground";
        private static readonly String _additionalBackgroundStyleName = "AdditionalBackground";
        private static readonly String _fieldButtonStyleName = "FieldButton";
        private static readonly String _fieldToggleStyleName = "FieldToggle";

        private static readonly String _idMainSettingsExpanded = "MainSettingsExpanded";
        private static readonly String _idDerivedSettingsExpanded = "DerivedSettingsExpanded";

        private Type _baseType = typeof(T);
        private bool _showDerivedInspector = false;

        private string[] _excludedProperties;

        private bool _mainSettingsExpanded = false;
        private bool _derivedSettingsExpanded = false;

        private float _indentSpace = 15f;
        private List<int> _simulatedIndentation = new List<int>();
        #endregion

        #region API
        public virtual void DrawMainSettings()
        {
        }

        public virtual void DrawAdditionalSettings()
        {
            DrawPropertiesExcluding(serializedObject, _excludedProperties);
        }
        #endregion

        #region Unity
        public virtual void OnEnable()
        {
            //Get styles.
            RUToolsSkin currentSkin = RUToolsPreferences.GetSkin();

            if(_logoStyle == null)
                _logoStyle = currentSkin.GetStyle(_logoStyleName);
            if (_logoTextStyle == null)
                _logoTextStyle = currentSkin.GetStyle(_logoTextStyleName);
            if (_separatorStyle == null)
                _separatorStyle = currentSkin.GetStyle(_separatorStyleName);
            if (_logoBackgroundStyle == null)
                _logoBackgroundStyle = currentSkin.GetStyle(_logoBackgroundStyleName);
            if (_mainBackgroundStyle == null)
                _mainBackgroundStyle = currentSkin.GetStyle(_mainBackgroundStyleName);
            if (_titleBackgroundStyle == null)
                _titleBackgroundStyle = currentSkin.GetStyle(_titleBackgroundStyleName);
            if (_additionalBackgroundStyle == null)
                _additionalBackgroundStyle = currentSkin.GetStyle(_additionalBackgroundStyleName);
            if (_fieldButtonStyle == null)
                _fieldButtonStyle = currentSkin.GetStyle(_fieldButtonStyleName);
            if (_fieldToggleStyle == null)
                _fieldToggleStyle = currentSkin.GetStyle(_fieldToggleStyleName);

            //Get all base properties.
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            FieldInfo[] fields = target.GetType().GetFields(flags);

            IEnumerable<FieldInfo> baseFields = fields.Where(x => x.DeclaringType == _baseType);
            int baseFieldsCount = baseFields.Count();

            _excludedProperties = new string[baseFieldsCount + 1];

            int i = 0;
            foreach (FieldInfo f in baseFields)
            {
                _excludedProperties[i] = f.Name;
                i++;
            }
            _excludedProperties[i] = "m_Script";

            if (fields.Length > baseFieldsCount)
            {
                _showDerivedInspector = true;
            }

            //Get States
            RUToolsPreferences preferences = RUToolsPreferences.GetPreferences();
            _mainSettingsExpanded = Convert.ToBoolean(preferences.GetEditorState(target.GetType(), _idMainSettingsExpanded));
            _derivedSettingsExpanded = Convert.ToBoolean(preferences.GetEditorState(target.GetType(), _idDerivedSettingsExpanded));
        }
        public virtual void OnDisable()
        {
            RUToolsPreferences preferences = RUToolsPreferences.GetPreferences();
            preferences.SetEditorState(target.GetType(), _idMainSettingsExpanded, Convert.ToInt16(_mainSettingsExpanded));
            preferences.SetEditorState(target.GetType(), _idDerivedSettingsExpanded, Convert.ToInt16(_derivedSettingsExpanded));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //Base class custom inspector.
            GUILayout.Space(5);

            ShowSeparator(1, _separatorStyle);

            Rect backgroundBox = EditorGUILayout.BeginVertical();
            ShowFullInspectorBox(backgroundBox, _mainBackgroundStyle);

            //Logo.
            ShowLogo();

            Rect mainTitleBox = EditorGUILayout.BeginVertical();
            ShowFullInspectorBox(mainTitleBox, _titleBackgroundStyle);
            _mainSettingsExpanded = EditorGUILayout.Foldout(_mainSettingsExpanded, "Main", true);
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel += 1;

            if (_mainSettingsExpanded)
            {
                DrawMainSettings();
            }

            EditorGUI.indentLevel -= 1;


            EditorGUILayout.EndVertical();

            ShowSeparator(1, _separatorStyle);

            //Derived class default inspector.
            if (_showDerivedInspector)
            {
                Rect additionalBox = EditorGUILayout.BeginVertical();
                ShowFullInspectorBox(additionalBox, _additionalBackgroundStyle);

                Rect addTitleBox = EditorGUILayout.BeginVertical();
                ShowFullInspectorBox(addTitleBox, _titleBackgroundStyle);
                _derivedSettingsExpanded = EditorGUILayout.Foldout(_derivedSettingsExpanded, "Additional", true);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel += 1;

                if (_derivedSettingsExpanded)
                {
                    DrawAdditionalSettings();
                }

                EditorGUI.indentLevel -= 1;

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Private methods
        protected void ShowLogo()
        {
            Rect r = EditorGUILayout.BeginHorizontal();
            ShowFullInspectorBox(r, _logoBackgroundStyle);
            //GUILayout.Space(0);
            EditorGUILayout.LabelField("", _logoStyle, GUILayout.MaxWidth(_logoStyle.fixedWidth));
            EditorGUILayout.LabelField("RUTools", _logoTextStyle, GUILayout.MaxWidth(_logoTextStyle.fixedWidth));
            EditorGUILayout.EndHorizontal();
        }

        protected void ShowFullInspectorBox(Rect rect, GUIStyle style)
        {
            rect.xMin = 0;
            rect.xMax += 5;
            rect.yMin -= 2;
            rect.yMax += 2;
            GUI.Box(rect, "", style);
        }

        protected void ShowSeparator(int width, GUIStyle style)
        {
            Rect separator = EditorGUILayout.BeginVertical();
            GUILayout.Space(width);
            separator.xMin = 0;
            separator.xMax += 5;
            GUI.Box(separator, "", style);
            EditorGUILayout.EndVertical();
        }

        protected void DrawButtonedField(Action onButtonPressed, Action fieldToDraw)
        {
            Rect r = StartHorizontalButtonedLayout();

            if (GUI.Button(r, "", _fieldButtonStyle))
            {
                if(onButtonPressed != null)
                    onButtonPressed.Invoke();
            }

            if(fieldToDraw != null)
                fieldToDraw.Invoke();

            EndtHorizontalButtonedLayout();
        }
        protected bool DrawToggledField(bool value, Action<bool> onToggle, Action fieldToDraw)
        {
            Rect r = StartHorizontalButtonedLayout();

            bool previousToggleValue = value;
            value = GUI.Toggle(r, value, "", _fieldToggleStyle);

            if (value != previousToggleValue)
            {
                if(onToggle != null)
                    onToggle.Invoke(value);
            }

            if(fieldToDraw != null)
                fieldToDraw.Invoke();

            EndtHorizontalButtonedLayout();

            return value;
        }
        protected bool DrawToggledField(bool value, Action onUntoggled, Action onToggled)
        {
            Rect r = StartHorizontalButtonedLayout();

            value = GUI.Toggle(r, value, "", _fieldToggleStyle);

            if (!value)
            {
                if(onUntoggled != null)
                    onUntoggled.Invoke();
            }
            else
            {
                if(onToggled != null)
                    onToggled.Invoke();
            }

            EndtHorizontalButtonedLayout();

            return value;
        }

        //Doesn't work very well.
        protected void SimulateIndentation()
        {
            if (EditorGUI.indentLevel != 0)
                _simulatedIndentation.Add(EditorGUI.indentLevel);

            EditorGUI.indentLevel = 0;

            int indentLevel = 0;
            for (int i = 0; i < _simulatedIndentation.Count; ++i)
            {
                indentLevel += _simulatedIndentation[i];
            }

            GUILayout.Space(indentLevel * _indentSpace);
        }
        protected void ResetIndentation()
        {
            while (_simulatedIndentation.Count > 0)
            {
                EditorGUI.indentLevel += _simulatedIndentation[0];
                _simulatedIndentation.RemoveAt(0);
            }
        }

        private Rect StartHorizontalButtonedLayout(float offsetY = 0, float offsetX = 0)
        {
            Rect r = EditorGUILayout.BeginHorizontal();
            r.width = 10;
            r.height = 10;
            r.y += offsetY + 3;
            r.x += EditorGUI.indentLevel * _indentSpace + offsetX + 3;

            EditorGUI.indentLevel += 1;

            return r;
        }
        private void EndtHorizontalButtonedLayout()
        {
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}