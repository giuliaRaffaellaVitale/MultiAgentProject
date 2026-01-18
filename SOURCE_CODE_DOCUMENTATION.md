# AdvancedAI - Source Code Documentation

## Project Overview

**AdvancedAI** is a Unity-based machine learning project that implements a paddle simulation environment using Unity ML-Agents. The project trains AI agents to play a 1v1 paddle game through reinforcement learning with curriculum-based learning phases.

**Technology Stack:**
- **Engine:** Unity 6000.0.49f1
- **ML Framework:** Unity ML-Agents 0.30.0
- **Training:** Python with PyTorch (via mlagents package)
- **Physics:** Unity Built-in Physics Engine

---

## Architecture Overview

The project follows a modular architecture with three main components:

```
┌─────────────────────────────────────────┐
│         Game Management Layer            │
│  (GM1vs1.cs) - Game State & Coordination │
└──────────────┬──────────────────────────┘
               │
     ┌─────────┴──────────┐
     │                    │
┌────▼─────┐      ┌──────▼────┐
│ Agents   │      │  Physics   │
│(P1vs1.cs)│      │(Ball, etc.)│
└──────────┘      └────────────┘
```

---

## Source Files Documentation

### 1. **GM1vs1.cs** - Game Manager (1v1 Mode)
**Location:** `Assets/Scripts/GM1vs1.cs`  
**Type:** MonoBehaviour (Singleton Pattern)  
**Purpose:** Central game controller managing game state, turn coordination, and reward distribution.

#### Key Responsibilities:
- **Game State Management:** Controls serve/rally phases
- **Team Management:** Manages red and blue team players
- **Score Tracking:** Tracks bounce counts and rally outcomes
- **Reward Distribution:** Assigns rewards/penalties to agents
- **Episode Reset:** Manages episode begins and ends

#### Methods:
- `SetServe(Team1vs1 team)` - Initiates serve phase
- `SetRally()` - Transitions to rally phase
- `AssignReward(Team1vs1 team, float reward)` - Distributes rewards to agents
- `EndRally()` - Terminates current rally
- `GetPlayerSpawnTransform(P1vs1 agent)` - Returns spawn position for player
- `GetBallSpawnTransform()` - Returns ball spawn position based on serving team
- `ValidBounce(Team1vs1 team)` - handles valid ball bounces
- `InvalidBounce(Team1vs1 team)` - handles invalid ball bounces
- `OnBounceExceed(CourtSide side)` - handles ball bounces exceeded w.r.t expected ones

#### Public Attributes:
- `BallController ball` - Reference to ball physics controller
- `P1vs1 redTeam` - Red team agent
- `P1vs1 blueTeam` - Blue team agent
- `Rigidbody ballRb` - Ball rigidbody for physics
- `Transform redServePosition` - Red team serve position
- `Transform blueServePosition` - Blue team serve position
- `Transform ballServeBlue` - Blue team ball serve position
- `Transform ballServeRed` - Red team ball serve position

#### Reward Constants:
- **Winning Reward:** 5.0f
- **Valid Bounce Reward:** 0.5f
- **Invalid Bounce Penalty:** -0.7f
- **Loser Penalty:** -3.0f

---

### 2. **P1vs1.cs** - Game Agent (1v1 Mode)
**Location:** `Assets/Scripts/P1vs1.cs`  
**Type:** Unity ML-Agents Agent  
**Purpose:** ML-Agent implementation for 1v1 gameplay with observation collection and action processing.

#### Inheritance:
- Extends `Unity.MLAgents.Agent`
- Implements agent learning loop

#### Key Methods:

**`Initialize()`**
- Called once when agent is created
- Initializes rigidbody component reference

**`OnEpisodeBegin()`**
- Reset agent state at episode start
- Clears velocity and angular velocity
- Spawns agent at designated position
- Resets distance tracking for movement reward

**`CollectObservations(VectorSensor sensor)`**
- **Observation Space:** 21 floats
- **Collected Data:**
  - Self position (3 values)
  - Self velocity (3 values)
  - Ball relative position (3 values)
  - Ball velocity (3 values)
  - Serving context (1 value)
  - Opponent position relative (3 values)
  - Opponent velocity (3 values)

**`OnActionReceived(ActionBuffers actions)`**
- **Action Space:** 4 continuous values
  - Index 0: Move X-axis
  - Index 1: Move Z-axis
  - Index 3: Swing/Rotation
- Implements movement physics and swing handling
- Computes movement rewards towards ball during reception
- Penalizes inactivity

**`HandleSwing(float swingInput)`**
- Handles the swing of the racket 

**`OnRacketHit(Collision collision)`**
- Handles the reward assignment after hitting the ball with the racket

#### Key Attributes:
- `teamId` - Team identifier (0=Red, 1=Blue)
- `moveSpeed` - Movement force multiplier (0.2)
- `isPlayerServing` - Current serving status

#### Reward Structure:

| Action | Reward |
|--------|--------|
| Racket hit | 2.0f |
| Good timing | 0.3f |
| Movement towards ball | 0.1f (movement × delta distance) |
| Too close (net/grid) | -0.1f |
| Inactivity | -0.05f |

---

### 3. **BallController.cs** - Ball Physics Controller
**Location:** `Assets/Scripts/BallController.cs`  
**Type:** MonoBehaviour  
**Purpose:** Manages ball physics, collision detection, and reception/bounce tracking.

#### Key Attributes:
- `bounceCount` - Tracks bounces per court side (max 2)
- `isServeBall` - Flag for serve state
- `isReceivable` - Flag for reception eligibility
- `lastBounceSide` - Tracks which court side last touched ball (Red/Blue/None)
- `lastTeamTouched` - Tracks which team last hit ball
- `expectedReceiver` - Identifies which team should receive next

#### Key Methods:

**`OnTriggerEnter(Collider collision)`**
- Handles ground contact (Red/Blue court)
- Manages out-of-field events
- Triggers game end conditions

**`OnCollisionEnter(Collision collision)`**
- Detects racket hits
- Updates team tracking
- Assigns reception rewards
- Resets bounce count on hit

#### Bounce Rules:
- **Maximum bounces:** 2 per court side
- **Reception requirement:** Ball must be marked receivable by expected team
- **Reception bonus:** 2.0f × reception count × 0.05

#### Penalty/Reward Logic:
- Opponent penalty for interception: -0.01f
- Body collision penalty: -0.3f
- Reception reward: 2.0f

---

### 4. **Racket.cs** - Racket Collision Handler (1v1)
**Location:** `Assets/Scripts/Racket.cs`  
**Type:** MonoBehaviour  
**Purpose:** Minimal collision handler attached to racket GameObject.

#### Functionality:
- Detects ball contact
- Triggers `SetRally()` state change
- Forwards collision data to agent via `OnRacketHit()`

#### Dependencies:
- References `P1vs1` agent
- References `GM1vs1` game manager

---

### 5. **Player1step.cs** - Phase 1 Curriculum Agent
**Location:** `Assets/Scripts/Player1step.cs`  
**Type:** Unity ML-Agents Agent  
**Purpose:** Simplified learning agent for Phase 1 (single-agent paddle learning).

#### Phase 1 Characteristics:
- **Single Agent:** Red team only
- **Simplified Environment:** No opponent
- **Goal:** Learn basic paddle mechanics (hit ball to blue field)
- **Duration:** Curriculum training phase 1

#### Key Methods:

**`CollectObservations(VectorSensor sensor)`**
- **Observation Space:** 18 floats
- **Includes:** Self position, velocity, ball relative position, velocity
- **Padding:** 7 neutral observations (for Phase 2 compatibility)

**`OnActionReceived(ActionBuffers actions)`**
- Similar action space to P1vs1
- Simpler reward structure focused on basic mechanics
- Movement reward toward ball

**`OnRacketHit(Collision collision)`**
- Ball physics calculation
- Height-based good timing reward (between -0.63f and 1.0f)
- Direction alignment reward toward blue field
- Racket hit base reward: 1.0f

**`HandleSwing(float swingInput)`**
- Handles the swing of the racket 

#### Reward Constants:
- Racket hit: 1.0f
- Good timing: 0.1f
- Movement: 0.01f
- Too close penalty: -0.05f

---

### 6. **BallCurriculum1step.cs** - Phase 1 Ball Event Handler
**Location:** `Assets/Scripts/BallCurriculum1step.cs`  
**Type:** MonoBehaviour  
**Purpose:** Manages ball events and episode termination in Phase 1.

#### Event Handlers:

**`OnTriggerEnter(Collider other)`**
- **Out Field:** Penalty -0.5f, end episode
- **Red Ground:** Penalty -0.3f, end episode (failure)
- **Blue Ground:** Reward 3.0f, end episode (success)

**`OnCollisionEnter(Collision col)`**
- **Net/Grid:** Penalty -0.5f, end episode
- **Body:** Penalty -0.3f (no episode end)

#### Learning Objective:
Successfully hit the ball from red court to blue court over the net.

---

### 7. **Racket1Step.cs** - Phase 1 Racket Handler
**Location:** `Assets/Scripts/Racket1Step.cs`  
**Type:** MonoBehaviour  
**Purpose:** Ball hit detection for Phase 1 agent.

#### Functionality:
- Detects ball collision with racket
- Forwards to `Player1step.OnRacketHit()` for physics and reward calculation

---

## Enumerations

### Team1vs1 (GM1vs1.cs)
```csharp
public enum Team1vs1 { None, Red, Blue }
```
Represents team identification in 1v1 matches.

### CourtSide (BallController.cs)
```csharp
public enum CourtSide { None, Red, Blue }
```
Identifies which court section was affected.

### GameState1vs1 (GM1vs1.cs)
```csharp
public enum GameState1vs1 { Serve, Rally }
```
Game phase state machine.

---

## Training Configuration

**File:** `config.yaml`

### Model Trainer: POCA (Proximal Policy Optimization with Centralized Agent)
- **Behavior:** Player (multi-agent 1v1)
- **Algorithm:** POCA
- **Max Training Steps:** 5,000,000

### Hyperparameters:
| Parameter | Value |
|-----------|-------|
| Batch Size | 1024 |
| Buffer Size | 40,960 |
| Learning Rate | 0.0003 |
| Beta (entropy) | 0.005 |
| Epsilon (clip ratio) | 0.2 |
| Lambda (GAE) | 0.95 |
| Epochs per batch | 4 |

### Network Architecture:
- **Hidden Layers:** 2
- **Hidden Units:** 256
- **Normalization:** Enabled
- **Encoder Type:** Simple
- **Discount Factor (Gamma):** 0.99

### Self-Play Configuration:
- **Save Checkpoint Every:** 50,000 steps
- **Team Change Every:** 200,000 steps
- **Swap Steps:** 10,000
- **Window Size:** 10
- **Play Against Latest Ratio:** 0.5
- **Initial ELO Rating:** 1200.0

### Episode Settings:
- **Time Horizon:** 256 steps
- **Checkpoints Retained:** 5
- **Summary Frequency:** 10,000 steps

---

## Learning Curriculum

### Phase 1 (`phase1.yaml`)
**Objective:** Basic paddle mechanics
- Single agent learns to hit ball from red court to blue court
- Simplified environment without opponent
- Focuses on swing timing and direction
- Progresses to Phase 2 when agent reaches proficiency

### Phase 2 (`phase2.yaml`)
**Objective:** 1v1 competitive gameplay
- Introduces opponent agent
- Escalates to full game rules (serve, rally, scoring)
- Self-play mechanism enables continuous improvement
- Agents learn counter-strategies

---

## Data Flow Diagram

```
┌──────────────────┐
│  Environment     │
│  (Scene Objects) │
└────────┬─────────┘
         │
    ┌────▼─────────────────────────────────────┐
    │  Physics Update (FixedUpdate)            │
    │  - Ball movement                         │
    │  - Racket swing                          │
    │  - Collision detection                   │
    └────┬─────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────┐
    │  Agent Observation Collection                 │
    │  - Collect positions, velocities              │
    │  - Collect game context                       │
    └────┬──────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────┐
    │  ML-Agents Training Engine                    │
    │  - Policy evaluation                          │
    │  - Action generation                          │
    └────┬──────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────┐
    │  Action Application                           │
    │  - Apply forces (movement)                    │
    │  - Apply rotations (swing)                    │
    └────┬──────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────┐
    │  Reward Calculation                           │
    │  - Game events trigger rewards/penalties      │
    │  - Agent learning signal                      │
    └────┬──────────────────────────────────────────┘
         │
    ┌────▼─────────────────────────────────────────┐
    │  Episode Reset (When Episode Ends)           │
    │  - Reset positions                           │
    │  - Clear velocities                          │
    │  - Prepare for next episode                  │
    └──────────────────────────────────────────────┘
```

---

## Class Dependency Graph

```
GM1vs1 (Game Manager)
├── P1vs1 (Agent) × 2
│   ├── Rigidbody (player physics)
│   └── Reference to GM1vs1
├── BallController
│   ├── Rigidbody (ball physics)
│   └── Reference to GM1vs1
└── Racket × 2
    └── References to P1vs1 and GM1vs1

Player1step (Curriculum Phase 1 Agent)
├── Rigidbody
└── Transforms (serving position, field markers)

BallCurriculum1step (Phase 1 Ball Handler)
└── Player1step reference

Racket1Step (Phase 1 Racket Handler)
└── Player1step reference
```

---

## Physics Configuration

### Rigidbody Settings (Agents & Ball):
- **Gravity:** Enabled
- **Drag:** Standard
- **Angular Drag:** Standard
- **Collision Detection:** Continuous or Continuous Dynamic
- **Constraints:** Freeze Y-rotation (agents stay upright)

### Collision Layers:
- **Ball** - Interacts with rackets, ground, grid
- **Rackets** - Detect ball collisions
- **Ground** - Trigger detection for serve/out court
- **Grid/Net** - Penalties on contact

---

## Key Constants Summary

### Rewards (1v1 Mode):
| Event | Reward |
|-------|--------|
| Winning rally | 5.0 |
| Valid bounce | 0.5 |
| Reception hit | 2.0 base |
| Racket contact | 2.0 |
| Good timing | 0.3 |
| Movement (per step) | 0.1 × delta distance |

### Penalties (1v1 Mode):
| Event | Penalty |
|-------|---------|
| Lost rally | -3.0 |
| Invalid bounce | -0.7 |
| Interception | -0.01 per step |
| Body collision | -0.3 |
| Inactivity | -0.05 |
| Out of field | -0.3 |

### Rewards (Phase 1):
| Event | Reward |
|-------|--------|
| Hit to blue court | 3.0 |
| Racket contact | 1.0 |
| Good timing | 0.1 |
| Movement (per step) | 0.01 × delta distance |

### Penalties (Phase 1):
| Event | Penalty |
|-------|---------|
| Out of field | -0.5 |
| Red court bounce | -0.3 |
| Net/Grid contact | -0.5 |
| Body collision | -0.3 |
| Too close | -0.05 |

---

## Training Output

**Model Format:** ONNX  
**Location:** `Assets/Player.onnx`  
**Framework:** Trained with PyTorch via ML-Agents

Model can be:
1. Used within Unity for inference
2. Exported for external deployment
3. Converted to other formats (TensorFlow, etc.)


