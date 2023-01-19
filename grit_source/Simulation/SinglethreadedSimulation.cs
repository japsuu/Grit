using System;
using Grit.Simulation.Elements;
using Grit.Simulation.Elements.ElementDefinitions;

namespace Grit.Simulation;

public class SinglethreadedSimulation : Simulation
{
    /// <summary>
    /// Flattened 2D-array of the world.
    /// Get the element at [x, y] by [x + y * Width].
    /// Flattened arrays are used here instead of 2D-arrays for performance reasons.
    /// </summary>
    private Element[] elementMatrix;
    
    /// <summary>
    /// All cells that have been stepped already this frame.
    /// </summary>
    private readonly bool[] steppedCells;
    
    /// <summary>
    /// Shuffled array of access indexes for the element matrix.
    /// Prevents visually consistent updating of the world.
    /// </summary>
    private int[] shuffledIndexes;

    private bool skipStepNextFrame;
    private readonly bool isInitialized;


    public override Element GetElementAt(int index) => elementMatrix[index];


    protected override void StepRandomTicks(double dt, double a)
    {
        if(!isInitialized) return;

        for (int i = 0; i < Settings.RANDOM_TICKS_PER_FRAME; i++)
        {
            (int x, int y) = RandomFactory.RandomPosInWorld();
            
            if (steppedCells[x + y * Settings.WORLD_WIDTH])
                continue;
            
            // No need to manually set the random cell dirty, since we don't want consequent updates to happen.
            // If HandleStep creates a new cell/causes movement, dirtying will be handled internally.
            (int newX, int newY) = elementMatrix[x + y * Settings.WORLD_WIDTH].RandomStep(this, x, y, dt);
            
            steppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
        }
    }

    public override void ForceStepAll()
    {
        skipStepNextFrame = false;
    }

    public override void ForceSkipAll()
    {
        skipStepNextFrame = true;
    }


    public SinglethreadedSimulation(int targetTps) : base(targetTps)
    {
        steppedCells = new bool[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];

        InitializeShuffledIndexes();
        ShuffleIndexArray();
        InitializeWorld();

        isInitialized = true;
    }

    
    public override void SetElementAt(int setX, int setY, Element newElement)
    {
        int index = setX + setY * Settings.WORLD_WIDTH;
        
        // Skip if we are trying to replace an element with itself.
        if(newElement.Id == elementMatrix[index].Id)
            return;
        
        elementMatrix[index] = newElement;
        FrontBuffer[index] = newElement.GetColor();    //BUG: WARN: NOTE: TODO: Move this so that dirty pixels get redrawn/recolored! Move to Element.cs.Draw()?
    }
    
    
    public override void SwapElementsAt(int x1, int y1, int x2, int y2)
    {
        // Do not allow an cell to swap itself.
        if (x1 == x2 && y1 == y2)
        {
            throw new Exception("Tried to swap an element with itself.");
        }
        
        // Swap operations
        int index1 = x1 + y1 * Settings.WORLD_WIDTH;
        int index2 = x2 + y2 * Settings.WORLD_WIDTH;
        (elementMatrix[index2], elementMatrix[index1]) = (elementMatrix[index1], elementMatrix[index2]);
        (FrontBuffer[index2], FrontBuffer[index1]) = (FrontBuffer[index1], FrontBuffer[index2]);
    }


    protected override void FixedUpdate(double dt, double a)
    {
        if(!isInitialized) return;
        
        // Reset the stepped cells.
        for (int i = 0; i < Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT; i++)
        {
            steppedCells[i] = false;
        }
        
        if(skipStepNextFrame)
        {
            skipStepNextFrame = false;
            return;
        }
        
        // Loop the world (random X access, bottom to top)
        for (int y = Settings.WORLD_HEIGHT - 1; y >= 0; y--)
        {
            foreach (int x in shuffledIndexes)
            {
                if (steppedCells[x + y * Settings.WORLD_WIDTH])
                    continue;

                (int newX, int newY) = elementMatrix[x + y * Settings.WORLD_WIDTH].Step(this, x, y, dt);

                steppedCells[newX + newY * Settings.WORLD_WIDTH] = true;
            }

            ShuffleIndexArray();
        }
    }
    
    
    private void InitializeShuffledIndexes()
    {
        shuffledIndexes = new int[Settings.WORLD_WIDTH];
        
        for (int i = 0; i < shuffledIndexes.Length; i++)
        {
            shuffledIndexes[i] = i;
        }
    }
    
    
    private void InitializeWorld()
    {
        elementMatrix = new Element[Settings.WORLD_WIDTH * Settings.WORLD_HEIGHT];
        
        for (int x = 0; x < Settings.WORLD_WIDTH; x++)
        {
            for (int y = 0; y < Settings.WORLD_HEIGHT; y++)
            {
                int index = x + y * Settings.WORLD_WIDTH;
        
                elementMatrix[index] = new AirElement(x, y);
                FrontBuffer[index] = elementMatrix[index].GetColor();

                // Generate some stone at the bottom of the world
                if (y >= Settings.WORLD_HEIGHT - 50)
                {
                    elementMatrix[index] = new StoneElement(x, y);
                    FrontBuffer[index] = elementMatrix[index].GetColor();
                }
            }
        }
    }
    
    
    /// <summary>
    /// Shuffles the x-axis access indexes using the Fisher-Yates algorithm.
    /// </summary>
    private void ShuffleIndexArray()
    {
        int n = shuffledIndexes.Length;
        while (n > 1)
        {
            int k = RandomFactory.SeedlessRandom.Next(n--);
            (shuffledIndexes[n], shuffledIndexes[k]) = (shuffledIndexes[k], shuffledIndexes[n]);
        }
    }
}