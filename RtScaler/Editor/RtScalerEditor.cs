using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(RtScaler))]
public class RtScalerEditor : Editor
{
    #region Private Fields

    private RectTransform[] _rts;
    private TextMeshProUGUI[] _textMeshes;
    private Text[] _texts;
    private LayoutElement[] _layoutElements;
    private Image[] _images;
    private Dictionary<RectTransform, Vector2> _anchoredPositions;
    private Dictionary<RectTransform, Vector2> _sizeDeltas;
    private Dictionary<TextMeshProUGUI, float> _textMeshFontSizes;
    private Dictionary<TextMeshProUGUI, float> _textMeshFontSizesMax;
    private Dictionary<Text, float> _textFontSizes;
    private Dictionary<Text, float> _textFontSizesMax;
    private bool _dataRemembered;
    private Dictionary<LayoutElement, LayoutElementProperties> _layoutElementsProperties;
    private Dictionary<Image, float> _imagesPpuMultipliers;

    #endregion

    #region Public Methods

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying) return;

        var scale = serializedObject.FindProperty("_scale");
        var value = serializedObject.FindProperty("_value");
        var lastValue = serializedObject.FindProperty("_lastValue");

        serializedObject.Update();
        EditorGUILayout.PropertyField(scale);

        if (scale.boolValue)
        {
            EditorGUILayout.PropertyField(value);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(EditorGUIUtility.FindTexture("console.infoicon.inactive.sml"), GUILayout.MaxWidth(20f));
        EditorGUILayout.LabelField("Dont forget to remove component after scaling, to prevent problems");
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Normalize RT"))
        {
            RegisterUndo("Normalize RT");
            var rt = ((RtScaler)target).GetComponent<RectTransform>();
            var wasScale = rt.localScale.x;
            rt.localScale = Vector3.one;
            Scale(wasScale, 1, "Normalize RT");
        }

        if (scale.boolValue)
        {
            Scale(value.floatValue, lastValue.floatValue, "Scale RT");
            lastValue.floatValue = value.floatValue;
        } else
        {
            ForgetData();
            value.floatValue = 1;
            lastValue.floatValue = 1;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void RegisterUndo(string undoName)
    {
        var objects = new List<Object>();

        if (target != null)
            objects.Add(target);

        if (_rts != null)
            objects.AddRange(_rts);

        if (_textMeshes != null)
            objects.AddRange(_textMeshes);

        if (_texts != null)
            objects.AddRange(_texts);

        if (_layoutElements != null)
            objects.AddRange(_layoutElements);

        if (_images != null)
            objects.AddRange(_images);

        Undo.RegisterCompleteObjectUndo(objects.ToArray(), undoName);

        foreach (var obj in objects)
        {
            EditorUtility.SetDirty(obj);
        }
    }

    private void Scale(float value, float lastValue, string undoName)
    {
        RememberData();
        if (!Mathf.Approximately(value, lastValue))
        {
            RegisterUndo(undoName);
            if (_rts != null && _rts.Length > 0)
            {
                foreach (var rt in _rts)
                {
                    rt.anchoredPosition = _anchoredPositions[rt] * value;
                    rt.sizeDelta = _sizeDeltas[rt] * value;
                }
            }

            if (_textMeshes != null && _textMeshes.Length > 0)
            {
                foreach (var textMesh in _textMeshes)
                {
                    textMesh.fontSize = _textMeshFontSizes[textMesh] * value;
                    textMesh.fontSizeMax = _textMeshFontSizesMax[textMesh] * value;
                }
            }

            if (_texts != null && _texts.Length > 0)
            {
                foreach (var text in _texts)
                {
                    text.fontSize = (int) (_textFontSizes[text] * value);
                    text.resizeTextMaxSize = (int) (_textFontSizesMax[text] * value);
                }
            }

            if (_layoutElements != null && _layoutElements.Length > 0)
            {
                foreach (var layoutElement in _layoutElements)
                {
                    layoutElement.minWidth = _layoutElementsProperties[layoutElement].MinWidth * value;
                    layoutElement.minHeight = _layoutElementsProperties[layoutElement].MinHeight * value;
                    layoutElement.preferredWidth = _layoutElementsProperties[layoutElement].PreferredWidth * value;
                    layoutElement.preferredHeight = _layoutElementsProperties[layoutElement].PreferredHeight * value;
                    layoutElement.flexibleWidth = _layoutElementsProperties[layoutElement].FlexibleWidth * value;
                    layoutElement.flexibleHeight = _layoutElementsProperties[layoutElement].FlexibleHeight * value;
                }
            }

            if (_images != null && _images.Length > 0)
            {
                foreach (var image in _images)
                {
                    if (image.type == Image.Type.Sliced)
                    {
                        image.pixelsPerUnitMultiplier = _imagesPpuMultipliers[image] / value;
                    }
                }
            }
        }
    }

    private void RememberData()
    {
        if (!_dataRemembered)
        {
            _dataRemembered = true;
            var t = target as RtScaler;
            if (t != null)
            {
                _rts = t.GetComponentsInChildren<RectTransform>();
                _textMeshes = t.GetComponentsInChildren<TextMeshProUGUI>();
                _texts = t.GetComponentsInChildren<Text>();
                _layoutElements = t.GetComponentsInChildren<LayoutElement>();
                _images = t.GetComponentsInChildren<Image>();

                _anchoredPositions = new Dictionary<RectTransform, Vector2>();
                _sizeDeltas = new Dictionary<RectTransform, Vector2>();
                _textMeshFontSizes = new Dictionary<TextMeshProUGUI, float>();
                _textMeshFontSizesMax = new Dictionary<TextMeshProUGUI, float>();
                _textFontSizes = new Dictionary<Text, float>();
                _textFontSizesMax = new Dictionary<Text, float>();
                _layoutElementsProperties = new Dictionary<LayoutElement, LayoutElementProperties>();
                _imagesPpuMultipliers = new Dictionary<Image, float>();

                if (_rts != null && _rts.Length > 0)
                {
                    foreach (var rt in _rts)
                    {
                        _anchoredPositions[rt] = rt.anchoredPosition;
                        _sizeDeltas[rt] = rt.sizeDelta;
                    }
                }

                if (_textMeshes != null && _textMeshes.Length > 0)
                {
                    foreach (var textMesh in _textMeshes)
                    {
                        _textMeshFontSizes[textMesh] = textMesh.fontSize;
                        _textMeshFontSizesMax[textMesh] = textMesh.fontSizeMax;
                    }
                }

                if (_texts != null && _texts.Length > 0)
                {
                    foreach (var text in _texts)
                    {
                        _textFontSizes[text] = text.fontSize;
                        _textFontSizesMax[text] = text.resizeTextMaxSize;
                    }
                }

                if (_layoutElements != null && _layoutElements.Length > 0)
                {
                    foreach (var layoutElement in _layoutElements)
                    {
                        _layoutElementsProperties[layoutElement] = new LayoutElementProperties
                        {
                            MinWidth = layoutElement.minWidth,
                            MinHeight = layoutElement.minHeight,
                            PreferredWidth = layoutElement.preferredWidth,
                            PreferredHeight = layoutElement.preferredHeight,
                            FlexibleWidth = layoutElement.flexibleWidth,
                            FlexibleHeight = layoutElement.flexibleHeight
                        };
                    }
                }

                if (_images != null && _images.Length > 0)
                {
                    foreach (var image in _images)
                    {
                        if (image.type == Image.Type.Sliced)
                        {
                            _imagesPpuMultipliers[image] = image.pixelsPerUnitMultiplier;
                        }
                    }
                }
            }
        }
    }

    private void ForgetData()
    {
        if (_dataRemembered)
        {
            _dataRemembered = false;
            _anchoredPositions.Clear();
            _anchoredPositions = null;
            _sizeDeltas.Clear();
            _sizeDeltas = null;
            _textMeshFontSizes.Clear();
            _textMeshFontSizes = null;
            _textMeshFontSizesMax.Clear();
            _textMeshFontSizesMax = null;
            _textFontSizes.Clear();
            _textFontSizes = null;
            _imagesPpuMultipliers.Clear();
            _imagesPpuMultipliers = null;
            _textFontSizesMax.Clear();
            _textFontSizesMax = null;
            _layoutElementsProperties.Clear();
            _layoutElementsProperties = null;
        }
    }

    #endregion
}