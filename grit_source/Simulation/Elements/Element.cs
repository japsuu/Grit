using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements;


public abstract class Element
{
    public enum ElementForm : ushort
    {
        Solid,
        Liquid,
        Gas
    }
    
    public abstract ushort Id { get; }
    
    protected abstract ElementForm InitialForm { get; }
    
    protected abstract Color InitialColor { get; }

    protected readonly int StartX;
    protected readonly int StartY;


    #region PRIVATE METHODS

    protected Element(int x, int y)
    {
        StartX = x;
        StartY = y;
    }

    #endregion

    
    #region PUBLIC METHODS

    public virtual (int newX, int newY) Step(Element[] matrix, int x, int y, float deltaTime)
    {
        return (x, y);
    }

    public virtual (int newX, int newY) RandomStep(Element[] matrix, int x, int y, float deltaTime)
    {
        return (x, y);
    }

    public virtual Color GetColor()
    {
        return InitialColor;
    }
    
    public virtual ElementForm GetForm()
    {
        return InitialForm;
    }

    #endregion
}