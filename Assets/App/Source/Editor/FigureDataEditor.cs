using UnityEditor;
using UnityEngine;
using App.Infrastructure.Config.ScriptableObjects;

namespace App.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(FigureDataSO))]
    public sealed class FigureDataEditor : Editor
    {
        private const float CellButtonSize = 36F;
        private static Color ActiveColor => new(0.3F, 0.85F, 0.4F);
        private static Color InactiveColor => new(0.25F, 0.25F, 0.25F);

        public override void OnInspectorGUI()
        {
            if (target is not FigureDataSO figureData) return;

            serializedObject.Update();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Figure Configuration", EditorStyles.boldLabel);

            var nameProp = serializedObject.FindProperty("_figureName");
            var colorProp = serializedObject.FindProperty("_colorIndex");

            EditorGUILayout.PropertyField(nameProp);
            EditorGUILayout.PropertyField(colorProp);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("5x5 Shape Matrix", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click any cell to toggle it on/off. Active cells form the figure shape.",
                MessageType.Info);

            EditorGUILayout.Space(6);

            var defaultColor = GUI.backgroundColor;
            var cellsProp = serializedObject.FindProperty("_cells");
            for (var y = 0; y < FigureDataSO.Dimension; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (var x = 0; x < FigureDataSO.Dimension; x++)
                {
                    var index = y * FigureDataSO.Dimension + x;
                    var isActive = figureData.GetCell(x, y);
                    GUI.backgroundColor = isActive ? ActiveColor : InactiveColor;

                    if (!GUILayout.Button(isActive ? "■" : " ", GUILayout.Width(CellButtonSize),
                            GUILayout.Height(CellButtonSize))) continue;
                    if (cellsProp is { isArray: true } && index < cellsProp.arraySize)
                        cellsProp.GetArrayElementAtIndex(index).boolValue = !isActive;
                    else
                    {
                        Undo.RecordObject(figureData, "Toggle Figure Cell");
                        figureData.SetCell(x, y, !isActive);
                        EditorUtility.SetDirty(figureData);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = defaultColor;

            EditorGUILayout.Space(12);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear All", GUILayout.Width(100), GUILayout.Height(26)))
            {
                if (cellsProp is { isArray: true })
                {
                    for (var i = 0; i < cellsProp.arraySize; i++)
                        cellsProp.GetArrayElementAtIndex(i).boolValue = false;
                }
                else
                {
                    Undo.RecordObject(figureData, "Clear Figure Cells");
                    figureData.ClearAll();
                    EditorUtility.SetDirty(figureData);
                }
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Invert All", GUILayout.Width(100), GUILayout.Height(26)))
            {
                if (cellsProp is { isArray: true })
                {
                    for (var i = 0; i < cellsProp.arraySize; i++)
                    {
                        var elem = cellsProp.GetArrayElementAtIndex(i);
                        elem.boolValue = !elem.boolValue;
                    }
                }
                else
                {
                    Undo.RecordObject(figureData, "Invert Figure Cells");
                    figureData.InvertAll();
                    EditorUtility.SetDirty(figureData);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            serializedObject.ApplyModifiedProperties();

            var domainFigure = figureData.ToDomainFigure();
            EditorGUILayout.HelpBox(
                $"Active Cells: {domainFigure.Shape.CellCount} | Bounding Box: {domainFigure.Shape.BoundingWidth}x{domainFigure.Shape.BoundingHeight}",
                MessageType.None);
        }
    }
}