﻿using Grit.Simulation.World;
using Microsoft.Xna.Framework;

namespace Grit.Simulation.Elements.ElementDefinitions;

public class SandElement : Element
{
    protected override Color InitialColor => RandomFactory.RandomColor(new Color(189, 148, 66, 255), new Color(255, 204, 102, 255));
    public override ushort Id => 1;
    protected override ElementForm InitialForm => ElementForm.Solid;

    public SandElement(int x, int y) : base(x, y)
    {
    }

    public override (int newX, int newY) Step(Element[] matrix, int x, int y, float deltaTime)
    {
        int newX = x;
        int newY = y;
        int belowY = y + 1;

        // If at the bottom of the world, replace cell with air.
        if (belowY >= Settings.WORLD_HEIGHT)
        {
            WorldMatrix.SetElementAt(x, y, new AirElement(x, y));
            return (newX, newY);
        }

        // Below cell.
        if (matrix[x + belowY * Settings.WORLD_WIDTH].GetForm() != ElementForm.Solid)
        {
            WorldMatrix.SwapElementsAt(x, y, x, belowY);
            newY = belowY;
            return (newX, newY);
        }

        // Randomly choose whether to prioritize left or right update
        bool prioritizeLeft = RandomFactory.RandomBool();
        if (prioritizeLeft)
        {
            // Left bottom cell.
            int leftX = x - 1;
            if (leftX > -1 && matrix[leftX + belowY * Settings.WORLD_WIDTH].GetForm() != ElementForm.Solid)
            {
                WorldMatrix.SwapElementsAt(x, y, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }

            // Right bottom cell.
            int rightX = x + 1;
            if (rightX < Settings.WORLD_WIDTH &&
                matrix[rightX + belowY * Settings.WORLD_WIDTH].GetForm() != ElementForm.Solid)
            {
                WorldMatrix.SwapElementsAt(x, y, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }
        }
        else
        {
            // Right bottom cell.
            int rightX = x + 1;
            if (rightX < Settings.WORLD_WIDTH &&
                matrix[rightX + belowY * Settings.WORLD_WIDTH].GetForm() != ElementForm.Solid)
            {
                WorldMatrix.SwapElementsAt(x, y, rightX, belowY);
                newX = rightX;
                newY = belowY;
                return (newX, newY);
            }

            // Left bottom cell.
            int leftX = x - 1;
            if (leftX > -1 && matrix[leftX + belowY * Settings.WORLD_WIDTH].GetForm() != ElementForm.Solid)
            {
                WorldMatrix.SwapElementsAt(x, y, leftX, belowY);
                newX = leftX;
                newY = belowY;
                return (newX, newY);
            }
        }

        return (newX, newY);
    }
}