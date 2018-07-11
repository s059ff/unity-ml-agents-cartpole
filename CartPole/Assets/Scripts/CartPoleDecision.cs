using System.Collections.Generic;
using UnityEngine;

public class CartPoleDecision : MonoBehaviour, Decision
{
    public float[] Decide(
        List<float> vectorObs, 
        List<Texture2D> visualObs, 
        float reward, 
        bool done, 
        List<float> memory)
    {
        var position = vectorObs[0];
        if (Mathf.Abs(position) < 0.5f)
            return new float[] { 0f };
        else if (0f < position)
            return new float[] { -1f };
        else
            return new float[] { 1f };
    }

    public List<float> MakeMemory(
        List<float> vectorObs, 
        List<Texture2D> visualObs, 
        float reward, 
        bool done, 
        List<float> memory)
    {
        return new List<float>();
    }
}
