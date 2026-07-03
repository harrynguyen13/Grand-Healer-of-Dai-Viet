using System;
using UnityEngine;

public class QuestDefinition
{
    public string Id { get; private set; }
    public string Title { get; private set; }
    public int Target { get; private set; }

    private readonly Func<int> getCurrentValue;

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
    }

    public int Current
    {
        get
        {
            if (getCurrentValue == null)
                return 0;

            return Mathf.Clamp(getCurrentValue.Invoke(), 0, Target);
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
}