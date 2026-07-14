using System;
using UnityEngine;

public class QuestDefinition
{
    public string Id { get; private set; }
    public string Title { get; private set; }
    public int Target { get; private set; }

    private readonly Func<int> getCurrentValue;

    private int startValue;

    public QuestDefinition(
        string id,
        string title,
        Func<int> getCurrentValue,
        int target
    )
    {
        Id = id;
        Title = title;
        this.getCurrentValue = getCurrentValue;
        Target = Mathf.Max(1, target);
        startValue = 0;
    }

    public int RawCurrent
    {
        get
        {
            if (getCurrentValue == null)
                return 0;

            return Mathf.Max(0, getCurrentValue.Invoke());
        }
    }

    public int StartValue
    {
        get { return startValue; }
    }

    public void SetStartValue(int value)
    {
        startValue = Mathf.Max(0, value);
    }

    public bool UseAbsoluteProgress
    {
        get { return IsRankQuest(); }
    }

    public int Current
    {
        get
        {
            if (UseAbsoluteProgress)
            {
                return Mathf.Clamp(RawCurrent, 0, Target);
            }

            int valueFromQuestStart = RawCurrent - startValue;
            return Mathf.Clamp(valueFromQuestStart, 0, Target);
        }
    }

    public bool IsCompleted
    {
        get { return Current >= Target; }
    }

    public string GetProgressText()
    {
        return Current + " / " + Target;
    }

    private bool IsRankQuest()
    {
        if (string.IsNullOrEmpty(Id))
            return false;

        return Id.Contains("_Rank_");
    }
}