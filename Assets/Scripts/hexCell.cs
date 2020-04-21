using UnityEngine;

internal class hexCell
{
    internal string type = string.Empty, tObject = string.Empty, landType = string.Empty;
    internal GameObject obj = null;
    internal int x = 0, y = 0;
    internal float innerRadius = 1;
    internal float outerRadius()
    {
        return innerRadius + (innerRadius * 0.1547f);
    }
    internal int height = 0;
}