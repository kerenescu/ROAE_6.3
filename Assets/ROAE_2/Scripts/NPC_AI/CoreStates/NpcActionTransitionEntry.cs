using System;

[Serializable]
public class NpcActionTransitionEntry
{
    public NpcActionType action;
    public float worsenProbability;
    public float keepProbability = 1f;
    public float improveProbability;

    public void Normalize()
    {
        float total = worsenProbability + keepProbability + improveProbability;
        if (total <= 0f)
        {
            keepProbability = 1f;
            worsenProbability = 0f;
            improveProbability = 0f;
            return;
        }

        worsenProbability /= total;
        keepProbability /= total;
        improveProbability /= total;
    }
}
