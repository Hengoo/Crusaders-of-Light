using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Consideration), true)]
public class CustomInspector_Consideration : Editor {


    public override void OnInspectorGUI()
    {
        Consideration MyTarget = (Consideration)target;

        base.OnInspectorGUI();

        AnimationCurve Anim = AnimationCurve.Linear(0, MyTarget.GetFunctionAtXValue(0f), 1, MyTarget.GetFunctionAtXValue(1f));

        Anim.AddKey(0.01f, MyTarget.GetFunctionAtXValue(0.01f));
        Anim.AddKey(-0.0001f, 0);
        Anim.AddKey(0.99f, MyTarget.GetFunctionAtXValue(0.99f));
        Anim.AddKey(1.0001f, 1);

        float Temp = 0;
        for (int i = 0; i < 20; i++)
        {
            Temp = (i / 2f) * 0.1f;

            Anim.AddKey(Temp, MyTarget.GetFunctionAtXValue(Temp));
        }

        for (int i = 1; i < Anim.keys.Length - 1; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(Anim, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(Anim, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        Anim = EditorGUILayout.CurveField(Anim, GUILayout.Width(300), GUILayout.Height(300));

        EditorGUILayout.LabelField(" Curve at 0: " + MyTarget.GetFunctionAtXValue(0f));
        EditorGUILayout.LabelField(" Curve at 1: " + MyTarget.GetFunctionAtXValue(1f));

        GUILayout.BeginVertical("box");
        GUILayout.Space(5);
        if (GUILayout.Button("Line - Standard"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.LINE, 1, 0, 0);
        }
        if (GUILayout.Button("Line - Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.LINE, -1, 1, 0);
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Space(5);
        if (GUILayout.Button("Logistic - Standard"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.LOGISTIC, 10, 1, 0.5f);
        }
        if (GUILayout.Button("Logistic - Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.LOGISTIC, -10, 1, 0.5f);
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Space(5);
        GUILayout.BeginVertical("box");
        GUILayout.Space(5);
        if (GUILayout.Button("Polynomial - 2 - Standard"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 2, 0, 0);
        }
        if (GUILayout.Button("Polynomial - 2 - x Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 2, 0, -1);
        }
        if (GUILayout.Button("Polynomial - 2 - y Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 2, -1, 0);
        }
        if (GUILayout.Button("Polynomial - 2 - x & y Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 2, -1, -1);
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.BeginVertical("box");
        GUILayout.Space(5);
        if (GUILayout.Button("Polynomial - 6 - Standard"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 6, 0, 0);
        }
        if (GUILayout.Button("Polynomial - 6 - x Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 6, 0, -1);
        }
        if (GUILayout.Button("Polynomial - 6 - y Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 6, -1, 0);
        }
        if (GUILayout.Button("Polynomial - 6 - x & y Inverse"))
        {
            SetCurveValues(MyTarget, Consideration.CurveType.POLYNOMIAL, 6, -1, -1);
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();
    }

    public void SetCurveValues(Consideration MyTarget, Consideration.CurveType CurveType, float Steepness, float yShift, float xShift)
    {
        MyTarget.TypeOfCurve = CurveType;
        MyTarget.Steepness = Steepness;
        MyTarget.yShift = yShift;
        MyTarget.xShift = xShift;
    }
}
