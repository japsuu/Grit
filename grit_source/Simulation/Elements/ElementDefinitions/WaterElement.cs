﻿using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class WaterElement : Element
{
    public WaterElement(int x, int y) : base(x, y)
    {
    }

    protected override Color InitialColor => Color.Aqua;
    public override ushort Id => 3;
    protected override ElementForm InitialForm => ElementForm.Liquid;

    public override (int newX, int newY) Step(Simulation simulation, int x, int y, double deltaTime)
    {
        int newX = x;
        int newY = y;
        int belowY = y + 1;
        int leftX = x - 1;
        int rightX = x + 1;
        
        // If at the bottom of the world, replace cell with air.
        if (belowY >= Settings.WORLD_HEIGHT)
        {
            simulation.SetElementAt(x, y, new AirElement(x, y));
            return (newX, newY);
        }

        // Below cell.
        if (simulation.GetElementAt(x + belowY * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
        {
            simulation.SwapElementsAt(x, y, x, belowY);
            newY = belowY;
            return (newX, newY);
        }

        // Randomly choose whether to prioritize left or right update
        bool prioritizeLeft = RandomFactory.RandomBool();
        if (prioritizeLeft)
        {
            // Left bottom cell.
            if (leftX > -1 && simulation.GetElementAt(leftX + belowY * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }
                    
            // Right bottom cell.
            if (rightX < Settings.WORLD_WIDTH && simulation.GetElementAt(rightX + belowY * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }
        }
        else
        {
            // Right bottom cell.
            if (rightX < Settings.WORLD_WIDTH && simulation.GetElementAt(rightX + belowY * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }
                    
            // Left bottom cell.
            if (leftX > -1 && simulation.GetElementAt(leftX + belowY * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }
        }

        if (prioritizeLeft)
        {
            // Left cell.
            if (leftX > -1 && simulation.GetElementAt(leftX + y * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, leftX, y);
                newX = leftX;
                return (newX, newY);
            }
                    
            // Right cell.
            if (rightX < Settings.WORLD_WIDTH && simulation.GetElementAt(rightX + y * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, rightX, y);
                newX = rightX;
                return (newX, newY);
            }
        }
        else
        {
            // Right cell.
            if (rightX < Settings.WORLD_WIDTH && simulation.GetElementAt(rightX + y * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, rightX, y);
                newX = rightX;
                return (newX, newY);
            }
                    
            // Left cell.
            if (leftX > -1 && simulation.GetElementAt(leftX + y * Settings.WORLD_WIDTH).GetForm() == ElementForm.Gas)
            {
                simulation.SwapElementsAt(x, y, leftX, y);
                newX = leftX;
                return (newX, newY);
            }
        }
        
        return (newX, newY);
    }
}