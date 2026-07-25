Lương Y Đại Việt – Truyền Nhân Y Đạo

English title: Grand Healer of Dai Viet

A 2D simulation role-playing game developed with Unity. The player takes the role of a young traditional healer in Đại Việt, examines patients, identifies diseases, prepares herbal prescriptions, manages medicinal herbs, completes quests, and develops their reputation as a physician.

Medical disclaimer: The medical content in this game is simplified and adapted for gameplay. It is not intended to provide real medical diagnosis or treatment advice.

Project overview

The project focuses on a traditional Vietnamese medicine-themed gameplay loop:

Patients arrive and enter the clinic queue.

The player begins an examination.

The patient describes symptoms.

The player reviews examination information and selects a diagnosis.

The player prepares a herbal prescription.

The treatment is evaluated.

The patient leaves the clinic.

Treatment results and rewards can be delivered through the mailbox.

Reputation, currency, quests, and progression are updated.

Implemented systems

Patient and clinic system

Patient spawning and movement

Clinic waiting queue

Examination stages and patient flow

Diagnosis interface

Prescription interface

Treatment evaluation

Patient session suspension and restoration

Patient exit handling

Medicine counter display

Automatic clinic queue spawning

Medical data system

DiseaseData

HerbData

MedicalDatabase

PatientCase

TreatmentEvaluator

Medical content is stored through Unity data assets and used by the examination and prescription systems.

Herbal medicine system

Herb inventory

Herb shop

Selected-herb management

Herb descriptions and role tooltips

Prescription quantities

Herb purchase and economy integration

Herb garden system

Plant selection

Cursor preview before planting

Garden plots

Plant growth data

Harvesting

Harvest-all support

Floating harvest text

Animated harvest-ready icon

Garden plot save data

Unlockable garden areas

Y Thư system

Medical book interface

Disease information

Prescription records

Tracking the number of times the book is opened during a treatment

Y Thư-related treatment rewards

Special-disease integration

Mailbox system

Mail data

Mail list items

Mailbox panel

Unread mail badge

Mailbox manager

Treatment-result and quest-related mail integration

Quest system

Quest definitions

Active quest pool

Quest progress tracking

Quest rewards

Runtime quest management

Quest panel interface

Special government case

The Government scene contains a separate special examination flow, including:

Special diagnosis interface

Special prescription interface

Special examination conditions

Special NPC handling

Special result evaluation

Special disease case data

Quest and mailbox integration

Y Thư integration for the special case

Player progression and economy

Player currency

Reputation and rank display

Player progression services

Persistent HUD

Save and continue flow

World and interaction systems

Player movement

NPC movement

Custom 2D pathfinding

Scene transitions

Persistent scene objects

Interaction prompts

Camera control

Minimap-related systems

Audio and volume settings

Scenes

The current build contains five enabled scenes:

Order

Scene

Main purpose

0

LoginScene

Main menu, new game, continue, and exit

1

IntroScene

Opening story sequence

2

SampleScene

Main outdoor village and gameplay area

3

ClinicInterior

Patient examination and prescription gameplay

4

Government

Special government examination and quest content

Technology

Engine: Unity 6000.4.0f1

Language: C#

Render pipeline: Universal Render Pipeline 17.4.0

Input: Unity Input System 1.19.0

Camera: Cinemachine 3.1.6

UI: Unity UI / TextMesh Pro

2D tools: Sprite, Tilemap, Aseprite importer, PSD importer, 2D Animation, Pixel Perfect, SpriteShape

Version control: Git and GitHub

Main project structure

Grand-Healer-of-Dai-Viet/
├── Assets/
│   ├── Animals/
│   ├── Audio/
│   ├── Character/
│   ├── Data/
│   ├── Editor/
│   ├── Goi_Thuoc/
│   ├── Icon_Thuoc/
│   ├── Map/
│   ├── Prefabs/
│   ├── Resources/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Audio/
│   │   ├── Camera/
│   │   ├── Core/
│   │   ├── Economy/
│   │   ├── HerbGardenPlot/
│   │   ├── Mailbox/
│   │   ├── MedicalData/
│   │   ├── Npc/
│   │   ├── Pathfinding/
│   │   ├── Patient/
│   │   ├── Player/
│   │   ├── Quest/
│   │   ├── Scene/
│   │   ├── Special/
│   │   ├── System/
│   │   └── UI/
│   ├── Settings/
│   ├── Shaders/
│   ├── TextMesh Pro/
│   └── UI/
├── Packages/
├── ProjectSettings/
└── README.md

Requirements

Install the following before opening the project:

Unity Hub

Unity Editor 6000.4.0f1

Git

Visual Studio, Visual Studio Code, or JetBrains Rider with C# support

Using the same Unity editor version is recommended to avoid scene, prefab, package, and serialization conflicts.

Installation

Clone the repository:

git clone https://github.com/harrynguyen13/Grand-Healer-of-Dai-Viet.git

Then:

Open Unity Hub.

Select Add project from disk.

Choose the cloned project folder.

Open the project with Unity 6000.4.0f1.

Wait for Unity to restore packages and finish importing assets.

Running the game

Open Assets/Scenes/LoginScene.unity.

Confirm that all five scenes are enabled in Build Profiles.

Enter Play Mode.

Start a new game or continue an existing save.

Build scene order

0. Assets/Scenes/LoginScene.unity
1. Assets/Scenes/IntroScene.unity
2. Assets/Scenes/SampleScene.unity
3. Assets/Scenes/ClinicInterior.unity
4. Assets/Scenes/Government.unity

Save data

The project contains persistent gameplay systems for player progress, economy, patient visits, quests, garden plots, mailbox state, and scene transitions.

Save files created during testing are stored locally by the Unity application and are not part of the Git repository.

Development notes

Scene and prefab references are assigned through the Unity Inspector.

Moving or renaming scripts, prefabs, scenes, or data assets should be done inside Unity to preserve .meta references.

Do not delete .meta files independently from their related Unity assets.

Avoid committing generated Unity folders such as Library, Temp, Logs, Obj, and local build output.

Older save files may become incompatible after save-data structures are changed.

Git workflow

Before starting work:

git pull

Stage only the related files for each feature or fix:

git add <files>
git commit -m "type(scope): description"
git push

Examples:

feat(patient): add clinic examination flow
fix(ythu): preserve book usage across clinic sessions
fix(patient): improve NPC clinic exit behavior
feat(garden): add herb planting and harvesting
docs: update project README

Project information

Project: Lương Y Đại Việt – Truyền Nhân Y Đạo

English title: Grand Healer of Dai Viet

Developer: Nguyễn Anh Hào

Repository: harrynguyen13/Grand-Healer-of-Dai-Viet

Platform target: Windows desktop

Genre: 2D Simulation RPG

Licence

This repository currently does not include an open-source licence. Source code and assets should not be assumed to be available for reuse, redistribution, or commercial use unless permission is granted by the project owner.