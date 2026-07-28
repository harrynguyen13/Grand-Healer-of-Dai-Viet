# Lương Y Đại Việt – Truyền Nhân Y Đạo

<p align="center">
  <strong>Grand Healer of Dai Viet</strong><br>
  A 2D Simulation RPG inspired by traditional Vietnamese medicine.
</p>

<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-6000.4.0f1-black?logo=unity">
  <img alt="C Sharp" src="https://img.shields.io/badge/C%23-Gameplay-blueviolet?logo=csharp">
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows">
  <img alt="Status" src="https://img.shields.io/badge/Status-In%20Development-orange">
</p>

---

## About the Project

**Grand Healer of Dai Viet** is a Unity 2D simulation role-playing game in which the player takes the role of a young traditional healer in Đại Việt.

The player examines patients, identifies diseases, prepares herbal prescriptions, manages medicinal herbs, completes quests, earns money and reputation, and gradually develops their career as a physician.

> **Medical disclaimer:** The medical content in this game is simplified and adapted for gameplay. It is not intended to provide real medical diagnosis or treatment advice.

---

## Gameplay Overview

The main gameplay loop is:

1. Patients arrive and enter the clinic queue.
2. The player begins an examination.
3. The patient describes their symptoms.
4. The player reviews the examination information.
5. The player selects a diagnosis.
6. The player prepares a herbal prescription.
7. The treatment result is evaluated.
8. The patient leaves the clinic.
9. Treatment results and rewards are delivered through the mailbox.
10. Currency, reputation, quests, and progression are updated.

```text
Patient Arrives
      |
      v
Examination
      |
      v
Diagnosis
      |
      v
Prescription
      |
      v
Treatment Evaluation
      |
      v
Mailbox Result
      |
      v
Money + Reputation
      |
      v
Unlock New Content
```

---

## Main Features

### Patient and Clinic System

- Patient spawning and movement
- Clinic waiting queue
- Examination stages and patient flow
- Diagnosis interface
- Prescription interface
- Treatment evaluation
- Patient session suspension and restoration
- Patient exit handling
- Automatic patient spawning and queue management

### Traditional Medicine System

- Medical book interface (Y Thư)
- Disease information
- Herbal medicine database
- Prescription records
- Herb quantity selection
- Tracking the number of Y Thư openings during treatment
- Y Thư-related treatment rewards and penalties

### Herbal Medicine and Garden System

- Herb inventory
- Herb shop
- Selected-herb management
- Herb descriptions and role tooltips
- Prescription quantities
- Herb purchase and economy integration
- Herb planting and growth
- Harvesting and harvest-all support
- Floating harvest text
- Animated harvest-ready icon
- Garden plot save data
- Unlockable garden areas

### Mailbox System

- Treatment-result mail
- Quest-related mail
- Mail list interface
- Unread mail badge
- Money and reputation results
- Diagnosis and prescription feedback

### Quest and Special Case System

- Quest definitions
- Active quest pool
- Quest progress tracking
- Quest rewards
- Runtime quest management
- Quest panel interface
- Separate special examination flow in the `Government` scene
- Special diagnosis and prescription interfaces
- Special NPC, result evaluation, mailbox, quest, and Y Thư integration

### Player Progression and World Systems

- Player currency
- Reputation and rank display
- Persistent HUD
- Save and Continue flow
- Player and NPC movement
- Custom 2D pathfinding
- Scene transitions
- Persistent scene objects
- Interaction prompts
- Camera control
- Minimap
- Audio and volume settings

---

## Scenes

The current build contains five enabled scenes:

| Order | Scene | Main Purpose |
|---:|---|---|
| 0 | `LoginScene` | Main menu, New Game, Continue, and Exit |
| 1 | `IntroScene` | Opening story sequence |
| 2 | `SampleScene` | Main outdoor village and gameplay area |
| 3 | `ClinicInterior` | Patient examination and prescription gameplay |
| 4 | `Government` | Special government examination and quest content |

---

## Technology

| Category | Technology |
|---|---|
| Engine | Unity `6000.4.0f1` |
| Language | C# |
| Render Pipeline | Universal Render Pipeline `17.4.0` |
| Input | Unity Input System `1.19.0` |
| Camera | Cinemachine `3.1.6` |
| UI | Unity UI and TextMesh Pro |
| 2D Tools | Sprite, Tilemap, 2D Animation, Pixel Perfect, SpriteShape |
| Import Tools | Aseprite Importer and PSD Importer |
| Version Control | Git and GitHub |

---

## Main Project Structure

```text
Grand-Healer-of-Dai-Viet/
|-- Assets/
|   |-- Audio/
|   |-- Character/
|   |-- Data/
|   |-- Map/
|   |-- Prefabs/
|   |-- Resources/
|   |-- Scenes/
|   |-- Scripts/
|   |   |-- Audio/
|   |   |-- Camera/
|   |   |-- Core/
|   |   |-- Economy/
|   |   |-- HerbGardenPlot/
|   |   |-- Mailbox/
|   |   |-- MedicalData/
|   |   |-- Npc/
|   |   |-- Pathfinding/
|   |   |-- Patient/
|   |   |-- Player/
|   |   |-- Quest/
|   |   |-- Scene/
|   |   |-- Special/
|   |   |-- System/
|   |   `-- UI/
|   |-- Settings/
|   `-- UI/
|-- Packages/
|-- ProjectSettings/
`-- README.md
```

> The structure above only shows the main folders to keep the README clean and readable.

---

## Requirements

- Unity Hub
- Unity Editor `6000.4.0f1`
- Git
- Visual Studio, Visual Studio Code, or JetBrains Rider with C# support

Using the same Unity Editor version is recommended to avoid package, scene, prefab, and serialization conflicts.

---

## Installation

Clone the repository:

```bash
git clone https://github.com/harrynguyen13/Grand-Healer-of-Dai-Viet.git
```

Then:

1. Open Unity Hub.
2. Select **Add project from disk**.
3. Choose the cloned project folder.
4. Open the project with Unity `6000.4.0f1`.
5. Wait for Unity to restore packages and finish importing assets.

---

## Running the Game

1. Open `Assets/Scenes/LoginScene.unity`.
2. Confirm that all five scenes are enabled in **Build Profiles**.
3. Enter Play Mode.
4. Select **New Game** or **Continue**.

---

## Build Scene Order

```text
0. Assets/Scenes/LoginScene.unity
1. Assets/Scenes/IntroScene.unity
2. Assets/Scenes/SampleScene.unity
3. Assets/Scenes/ClinicInterior.unity
4. Assets/Scenes/Government.unity
```

---

## Controls

| Key | Action |
|---|---|
| `W`, `A`, `S`, `D` | Move |
| `Left Shift` | Run |
| `E` | Interact or enter a door |
| `F` | Start a patient examination |
| Mouse | Navigate menus and interfaces |

---

## Save Data

The project contains persistent systems for:

- Player progression
- Economy
- Patient visits
- Quests
- Herb garden plots
- Mailbox state
- Scene transitions

Save files created during testing are stored locally through Unity and are not included in the Git repository.

---

## Development Notes

- Scene and prefab references are assigned through the Unity Inspector.
- Move or rename Unity assets inside Unity to preserve `.meta` references.
- Do not delete `.meta` files separately from their related assets.
- Do not commit generated folders such as `Library`, `Temp`, `Logs`, `Obj`, or local build output.
- Older save files may become incompatible after save-data structures are changed.

---

## Git Workflow

Before starting work:

```bash
git pull
```

Stage only files related to the current feature or fix:

```bash
git add <files>
git commit -m "type(scope): description"
git push
```

Example commit messages:

```text
feat(patient): add clinic examination flow
fix(patient): improve NPC clinic exit behavior
fix(ythu): preserve book usage across clinic sessions
feat(garden): add herb planting and harvesting
docs: update project README
```

---

## Screenshots

Gameplay screenshots and GIF previews can be added here later, for example:

- Main menu
- Village map
- Clinic interior
- Diagnosis interface
- Prescription interface
- Herb garden
- Government special case

---

## Project Information

- **Project:** Lương Y Đại Việt – Truyền Nhân Y Đạo
- **English title:** Grand Healer of Dai Viet
- **Developer:** Nguyễn Anh Hào
- **Repository:** `harrynguyen13/Grand-Healer-of-Dai-Viet`
- **Platform target:** Windows desktop
- **Genre:** 2D Simulation RPG

---

## License

This repository currently does not include an open-source license.

The source code and project assets should not be assumed to be available for reuse, redistribution, or commercial use unless permission is granted by the project owner.
