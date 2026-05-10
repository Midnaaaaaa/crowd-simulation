# Crowd Simulation & Procedural Navigation in Unity

Real-time procedural navigation system in Unity featuring dynamic map generation, multi-agent movement, and advanced bidirectional A* with custom heuristics.

> Tested on RTX 4060 — real-time simulation performance

---

## Preview

### Simulation Demo

[![Watch the video](https://img.youtube.com/vi/6vgtR8yfXT0/maxresdefault.jpg)](https://www.youtube.com/watch?v=6vgtR8yfXT0)

---

## Screenshots

<p align="center">
  <img src="Images/image.png" width="390"/>
</p>

---

## Features

- Procedural map generation with randomized obstacle placement
- Dual random point generation with bidirectional A* pathfinding
- Custom heuristic modification for optimized back-to-front search behavior
- Collision detection system:
  - Agent ↔ Agent
  - Agent ↔ Environment
- Steering-based path following system
- Physics-inspired movement system based on Reynolds-style steering behaviors
- Scalable multi-agent simulation framework
- Real-time crowd navigation and avoidance

---

## Technologies

- Unity
- C#
- A* Pathfinding
- Bidirectional Search
- Steering Behaviors
- Procedural Generation
- Physics-based Simulation

---

## Navigation System

The project uses a custom implementation of **Bidirectional A\*** where the search is performed simultaneously from both the start and target nodes.

Additional heuristic modifications were introduced to:
- Improve convergence speed
- Reduce unnecessary node exploration
- Optimize back-to-front search behavior in dense environments

This allows large numbers of agents to navigate procedurally generated maps efficiently in real time.

---

## Crowd Simulation

Agents follow paths using steering-based movement inspired by Craig Reynolds' classical behaviors:

- Seek
- Arrival
- Separation
- Collision Avoidance

The combination of local steering and global pathfinding creates emergent crowd-like behavior while maintaining stable navigation performance.

---

## Procedural Environment

Maps are generated dynamically with:
- Random obstacle placement
- Walkable area generation

Each simulation run produces different crowd movement patterns and navigation challenges.

---

## Performance

### Tested Hardware

- GPU: RTX 4060

The simulation supports multiple simultaneous agents while maintaining real-time responsiveness.

---

## Installation

1. Clone the repository

```bash
git clone https://github.com/yourusername/your-repository.git
```

2. Open the project in Unity

3. Load the main scene

4. Press Play

---

## Future Work

- GPU-based crowd simulation
- Flow field navigation
- Dynamic obstacle avoidance
- Hierarchical pathfinding

---

## Inspiration

This project was developed as an exploration of:
- AI navigation systems
- Pathfinding optimization
- Emergent multi-agent behavior
- Real-time procedural simulation

---

## License

MIT License
