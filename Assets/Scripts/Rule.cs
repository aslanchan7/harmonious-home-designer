using System;

[System.Serializable]
public class Rule
{
    public string name;

    // public delegate bool RuleCheck();
    private Func<bool> ruleCheck;
    public bool ruleState;
    public int points;
    public FSEnergyType energyType;
    public string[] furnitureDependencies;

    public enum StateChange
    {
        TRUE_TO_FALSE = -1,
        UNCHANGED = 0,
        FALSE_TO_TRUE = 1,
    }

    public Rule(
        string name,
        Func<bool> ruleCheck,
        int points,
        FSEnergyType energyType,
        string[] furnitureDependencies
    )
    {
        this.name = name;
        this.ruleCheck = ruleCheck;
        this.points = points;
        this.energyType = energyType;
        this.furnitureDependencies = furnitureDependencies;
        ruleState = false;
    }

    public StateChange Check()
    {
        bool oldState = ruleState;
        ruleState = ruleCheck();
        if (ruleState == oldState)
            return StateChange.UNCHANGED;
        return ruleState
            ? StateChange.FALSE_TO_TRUE
            : StateChange.TRUE_TO_FALSE;
    }
}
