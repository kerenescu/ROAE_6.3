using System;
using System.Collections.Generic;

public static class NpcStateSpaceGenerator
{
    public static List<NpcDecisionState> GenerateAllStates()
    {
        List<NpcDecisionState> states = new List<NpcDecisionState>();

        foreach (EmpathyBucket empathy in Enum.GetValues(typeof(EmpathyBucket)))
        {
            foreach (CreativityBucket creativity in Enum.GetValues(typeof(CreativityBucket)))
            {
                foreach (CorruptionBucket corruption in Enum.GetValues(typeof(CorruptionBucket)))
                {
                    foreach (RelationshipBucket relationship in Enum.GetValues(typeof(RelationshipBucket)))
                    {
                        NpcDecisionState state = new NpcDecisionState
                        {
                            empathy = empathy,
                            creativity = creativity,
                            corruption = corruption,
                            relationship = relationship
                        };

                        states.Add(state);
                    }
                }
            }
        }

        return states;
    }
}
