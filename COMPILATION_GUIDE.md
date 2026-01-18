# AdvancedAI - Compilation & Setup Guide

## System Requirements

### Minimum Specifications:
- **Operating System:** Windows 10/11, macOS 10.12+, or Linux
- **RAM:** 8 GB minimum (16 GB recommended for training)
- **Storage:** 50 GB free space (for Unity, dependencies, and training data)
- **GPU:** NVIDIA GPU with CUDA capability (recommended for training)

### Software Prerequisites:
| Software | Version | Purpose |
|----------|---------|---------|
| Unity Hub | Latest | Project management |
| Unity | 6000.0.49f1 | Game engine (exact version) |
| Python | 3.10 | Training and utilities |
| Visual Studio | 2022+ | C# development (optional) |
| Git | Latest | Version control |

---

## Prerequisites Installation

### 1. Install Python

#### Windows:
```powershell
# Download from python.org or use Windows Store
# Ensure "Add Python to PATH" is checked during installation

# Verify installation
python --version
pip --version
```

#### macOS:
```bash
# Using Homebrew
brew install python@3.10

# Verify installation
python3 --version
pip3 --version
```

#### Linux (Ubuntu/Debian):
```bash
sudo apt update
sudo apt install python3.10 python3.10-venv python3.10-dev pip

# Verify installation
python3.10 --version
pip3 --version
```

### 2. Install Unity Hub

1. Download from [unity.com/hub](https://unity.com/hub)
2. Run installer and follow setup wizard
3. Accept license agreements
4. Create/login to Unity account

### 3. Install Unity 6000.0.49f1

```powershell
# Using Unity Hub CLI (if available)
# OR manually through Unity Hub GUI:
# 1. Open Unity Hub
# 2. Click "Installs" → "Install Editor"
# 3. Search for version 6000.0.49f1
# 4. Click Install
# 5. Select modules:
#    - Windows: Visual Studio 2022 support
#    - macOS: iOS Build Support (if building for iOS)
#    - Linux: Linux IL2CPP support (if building for Linux)
```

### 4. Clone or Extract Project

```powershell
# Clone from repository
git clone https://github.com/giuliaRaffaellaVitale/MultiAgentProject.git
cd AdvancedAI

# OR extract zip file
# Extract to desired location
```

---

## Project Setup

### Step 1: Open Project in Unity

```powershell
# Option A: Command line (Windows)
cd 
"C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Unity.exe" -projectPath . -logFile -

# Option B: Through Unity Hub GUI
# 1. Open Unity Hub
# 2. Click "Open"
# 3. Navigate to project folder
# 4. Select AdvancedAI folder
# 5. Click "Open"
```

**Expected:** Unity loads the project and compiles scripts (5-10 minutes on first load)

### Step 2: Verify Package Dependencies

Unity should automatically resolve packages from `Packages/manifest.json`:

```powershell
# In Unity Editor console, verify no missing dependency errors
# Key packages should load:
# - ML-Agents 0.30.0
# - Input System 1.14.0
# - Universal Render Pipeline 17.0.4
```

**If packages don't load:**
1. Go to `Window` → `TextAsset`
2. Clear package cache: `Library/PackageCache`
3. Restart Unity
4. Wait for package re-download

### Step 3: Set Up Python Environment

Navigate to project root and create virtual environment:

```powershell
# Windows
python -m venv ml_agents_env
.\ml_agents_env\Scripts\Activate.ps1

# macOS/Linux
python3 -m venv ml_agents_env
source ml_agents_env/bin/activate
```

**Output:** Console shows `(ml_agents_env)` prefix

### Step 4: Install Python Dependencies

```powershell
# Ensure virtual environment is activated
pip install --upgrade pip

# Install from requirements.txt
pip install -r requirements.txt

# Verify key packages
pip show mlagents
pip show torch
```

**Expected installations:** ~30 packages, ~2-3 GB disk space

### Step 5: Verify ML-Agents Installation

```powershell
# Test mlagents CLI
mlagents-learn --help

# Expected: Shows help menu with training command options
```

If command not found:
```powershell
# Reinstall ML-Agents
pip uninstall mlagents -y
pip install mlagents==0.30.0
```

---

## Compilation (Building from Source)

### C# Script Compilation

**Automatic:** Unity automatically compiles all C# scripts when:
- Project opens
- Scripts are saved
- Project settings change

**Manual trigger:**
1. `Assets` → `Reimport All`
2. Or: `Ctrl+Shift+R` (Windows) / `Cmd+Shift+R` (macOS)

**Check for compilation errors:**
```
Window → General → Console
```
Look for red error messages.

### Common Compilation Issues:

#### Issue: "Cannot find Assembly-CSharp"
**Solution:**
```powershell
# Delete cache and reimport
rm -r Library/ScriptAssemblies
# Then restart Unity
```

#### Issue: ML-Agents namespace not found
**Solution:**
1. Verify package installed: `Packages/manifest.json` contains `"com.unity.ml-agents": "4.0.0"`
2. If missing, add dependency through Package Manager:
   - `Window` → `Package Manager`
   - `+` → `Add package by name...`
   - Enter: `com.unity.ml-agents`
   - Version: `0.30.0`

#### Issue: Input System conflicts
**Solution:**
1. `Edit` → `Project Settings`
2. Search for "Active Input Handling"
3. Set to `Both` (to support both old and new input systems)

---

## Running Training

### Prerequisites Check:
```powershell
# Ensure Python environment activated
.\ml_agents_env\Scripts\Activate.ps1

# Verify Unity build settings are correct
# File → Build Settings
# - Scenes: All required scenes included
# - Target Platform: Windows/Mac/Linux as desired
```

### Training Phase 1 (Basic Paddle Training)

```powershell
# Terminal 1: Keep virtual environment activated
.\ml_agents_env\Scripts\Activate.ps1

# Navigate to project folder
cd C:\Users\giuli\AdvancedAI

# Start training
mlagents-learn config.yaml --run-id=phase1_run --seed=42
# click play on Unity scene

# Expected output:
# - Agent initialized
# - Training starts
# - Progress logged every 10,000 steps
# - Checkpoints saved every 50,000 steps

```

**Output Location:** `results/phase1_run/`

### Training Phase 2 (1v1 Competitive)

Once Phase 1 completes or sufficiently trained:

```powershell
# Activate environment if needed
.\ml_agents_env\Scripts\Activate.ps1

# Start Phase 2 training with self-play
mlagents-learn config.yaml --run-id=phase2_run --seed=42 --torch-device=cuda

# Parameters:
# --run-id: Unique identifier for this run
# --seed: Random seed for reproducibility
# --torch-device: Use GPU (cuda) or CPU (cpu)
```
---

## Using Trained Models in Inference

### Step 1: Locate Trained Model

```powershell
# After training completes
ls results/phase1_run/

# Look for: *.onnx file
# Example: Player.onnx (from training output)
```

### Step 2: Import Model to Unity

```
Project window:
- Assets/
  - Right-click → Import New Asset
  - Select Player.onnx from results/
  - Click Import
```

### Step 3: Configure Agent for Inference

In Unity Editor:
1. Select agent GameObject (e.g., Red Player)
2. In Inspector, find `Behavior Parameters` component
3. Set:
   - Behavior Type: Inference Only
   - Model: Drag Player.onnx here

### Step 4: Run Inference in Game

```powershell

# Click play in the Unity scene
# Game plays with trained agent (no network connection needed)
```

