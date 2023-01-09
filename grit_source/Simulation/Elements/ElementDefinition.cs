using Grit.Simulation.World;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements;

/// <summary>
/// Defines the PROPERTIES of an element.
/// NOT the functionality.
/// </summary>
public abstract class ElementDefinition
{
    //TODO: Get rid of this, and change it to ElementForm.
    public abstract ElementType Type { get; }

    protected abstract Color InitialColor { get; }
    protected abstract ElementForm InitialForm { get; }

    protected readonly int StartX;
    protected readonly int StartY;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">Start x-position</param>
    /// <param name="y">Start y-position</param>
    protected ElementDefinition(int x, int y)
    {
        StartX = x;
        StartY = y;
    }

    /// <summary>
    /// Returns the current color of this object.
    /// Allows for example heat to change the color of the cell.
    /// </summary>
    public virtual Color GetColor()
    {
        return InitialColor;
    }

    /// <summary>
    /// Returns the current element form of this object.
    /// Allows for example heat to change the form of the cell (liquid -> gas).
    /// </summary>
    public virtual ElementForm GetForm()
    {
        return InitialForm;
    }
}