# Grit

Single buffered complex cellular automata; Minecraft in 2D, but everything is simulated.

Pixels are simulated based on their physical properties.

Built in MonoGame.

---

## Currently implemented:
- Solids, liquids, and gases,
- Infinite world with chunking,
- Fixed timestep,
- Dirty recting,
- Minecraft-like random ticking system,
- Plethora of debugging tools (with ImGUI support!).

---

Most of the interesting logic can be found [here](https://github.com/japsuu/Grit/blob/main/grit_source/Simulation/Simulation.cs) (Simulation.cs) and [here](https://github.com/japsuu/Grit/blob/main/grit_source/Simulation/World/Regions/Chunks/Chunk.cs) (Chunk.cs)
