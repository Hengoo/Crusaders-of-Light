using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consideration : ScriptableObject {

    public enum CurveType
    {
        NONE = 0,
        LINE = 1,
        LOGISTIC = 2,
        POLYNOMIAL = 3
    }

    public struct Context
    {
        public Character target;
        public Character user;
        public ItemSkill skill;
    }

    [Header("Consideration: ")]
    public CurveType TypeOfCurve = CurveType.NONE;
    public float Steepness = 0.0f;
    public float yShift = 0.0f;
    public float xShift = 0.0f;



    public virtual float CalculateScore(Context SkillContext)
    {
        return 0.0f;
    }

    protected float CalculateConsideration(CurveType CurveType, float InputValue, float Steepness, float yShift, float xShift)
    {
        if (CurveType == CurveType.LINE)
        {
            return CalculateConsiderationLine(InputValue, Steepness, yShift);
        }
        else if (CurveType == CurveType.LOGISTIC)
        {
            return CalculateConsiderationLogistic(InputValue, Steepness, yShift, xShift);
        }
        else if (CurveType == CurveType.POLYNOMIAL)
        {
            return CalculateConsiderationPolynomial(InputValue, Steepness, yShift, xShift);
        }

        return 0.0f;
    }

    // If m Negativ: Inverse
    private float CalculateConsiderationLine(float InputValue, float M, float N)
    {
        float outputValue = 0.0f;

        outputValue = M * InputValue + N;

        return Mathf.Clamp01(outputValue);
    }

    // k = "Steepness" IF Negativ: Inverse
    // yShift = Minimum y Value
    // xShift = x Value of the middle
    // (1+(1-yShift)/(1+e^(-k*(x-xShift))))-(1-yShift)
    private float CalculateConsiderationLogistic(float InputValue, float K, float yShift, float xShift)
    {
        float outputValue = 0.0f;

        outputValue = ((1 + (1 - yShift)) / (1 + Mathf.Exp(-K * (InputValue - xShift)))) - (1 - yShift);

        return Mathf.Clamp01(outputValue);
    }

    // yShift = Minimum y Value
    // yShift Negative: flip vertically
    // xShift = -1 = flip horizontally
    private float CalculateConsiderationPolynomial(float InputValue, float Exponent, float yShift, float xShift)
    {
        float outputValue = 0.0f;

        if (yShift >= 0)
        {
            // ((x + xShift)^exponent)*(1-yShift)+yShift
            outputValue = Mathf.Pow((InputValue + xShift), Exponent) * (1 - yShift) + yShift;
        }
        if (yShift < 0)
        {
            // ((x + xShift)^exponent)*-(1-Abs(yShift))+1
            outputValue = Mathf.Pow((InputValue + xShift), Exponent) * -(2 - Mathf.Abs(yShift)) + 1;
        }

        return Mathf.Clamp01(outputValue);
    }

    protected float ClampInputValue(float Value, float LowerEnd, float HigherEnd)
    {
        return Mathf.Clamp01((Value - LowerEnd) / (HigherEnd - LowerEnd));
    }

    public float GetFunctionAtXValue(float xValue)
    {
        return CalculateConsideration(TypeOfCurve, xValue, Steepness, yShift, xShift);
    }
}
