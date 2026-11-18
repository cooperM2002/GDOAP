using UnityEngine;

/// <summary>
/// base class for primary use of a given item, any equipped item that is "usable" bases itself off of this.
/// </summary>
public abstract class PrimaryUseBehaviour : SneedSpace
{

    /// <summary>
    /// base PrimaryUse, LMB
    /// </summary>
    public abstract void PrimaryUse();

}
