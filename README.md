# Grit

Single buffered complex cellular automata; Minecraft in 2D, but everything is simulated.

Pixels are simulated based on their physical properties.

Built in MonoGame.

---

## Currently implemented:
- Solids, liquids, and gases,
- Chunking,
- Dirty recting,
- Minecraft-like random ticking system,
- Plethora of debugging tools.

---

Most of the interesting logic can be found [here](https://github.com/japsuu/Grit/blob/main/grit_source/Simulation/World/WorldMatrix.cs)(WorldMatrix.cs) and [here](https://github.com/japsuu/Grit/blob/main/grit_source/Simulation/World/DirtyChunk.cs)(DirtyChunk.cs)
