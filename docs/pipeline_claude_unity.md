# Claude ↔ Unity Pipeline

> Status: 🟢 Approved 2026-10-10 · How the AI crew builds inside Unity, and how the director stays in control.

## The three ways Claude works in Unity

| Channel | What it is | Editor open? | Used for |
|---|---|---|---|
| **1. MCP bridge** | A plugin in the Unity Editor that lets Claude Code act on it live: read the scene, create and modify objects, edit components, read the console | ✅ Must be open | Day-to-day building, tweaking, inspecting, fixing errors |
| **2. Scripts in the repo** | C# files Claude writes into `Assets/`: runtime systems (beat sync, spring bones…) and **builder scripts** (editor menu commands that construct a set, lighting rig or Timeline from scratch) | Either | Anything that must be repeatable or reviewable: grey-box sets, lighting rigs, render setup |
| **3. Batch mode** | Unity launched from the command line, with no editor window, running a builder or capture script | ❌ Must be closed | Automated renders and test captures, e.g. overnight |

**Rule:** if it matters, it's in a script or a saved scene/prefab committed to git. MCP edits are saved into scenes and prefabs, never left only in the open editor.

## The production loop

```
Director: brief in docs/briefs/        (intent, must-have, must-avoid, done-when)
   │
   ▼
Claude Code: reads brief + MASTER_PROMPT
   │   builds via MCP (live) and/or builder scripts (repeatable)
   │   reads the Unity console, fixes errors
   ▼
Claude Code: captures screenshots / test renders → looks at them
   │   checks against the brief's "done when" → iterates
   ▼
Director: reviews the images/renders → notes in the brief's review table
   │
   ├── notes → back to Claude Code
   └── approved → Claude Code commits + pushes
```

## Visual feedback (how Claude "sees")
- **Screenshots:** a small editor tool that renders a chosen camera to PNG in `captures/`. Claude reads the images and checks composition, colour and lighting.
- **Test renders:** Unity Recorder → MP4 for the director (motion, timing, music). Claude can't judge motion or audio; the director does.
- **Ref overlay:** `KPop → Goal 1 → Capture Ref Overlay` renders the main camera at a reference frame's exact size and blends it 50/50 with the ref (`captures/_overlay/`, git-ignored). It's the most reliable way to line up a set and camera with a reference.

## MCP options (researched 2026-10-10)

| Option | Cost | Notes |
|---|---|---|
| **MCP for Unity** (CoplayDev, open source) | Free | Mature community plugin; Unity package with a built-in "register with Claude Code" button |
| **Unity's official MCP server** (Unity AI, open beta) | Needs a Unity AI trial/subscription; pricing unclear | Unity 6+, `com.unity.ai.assistant`, project must be connected to Unity Cloud; beta, may change |
| Others (IvanMurzak, CoderGamester, UniMCP4CC…) | Free | Smaller; check Unity 6.3 support |

**Decision (2026-10-10): MCP for Unity (CoplayDev).** Free, built for exactly this, and nothing locks us in. Re-evaluate the official server once it leaves beta.

## Repo layout
```
kpop_animation/
  docs/            production bible
  unity/           the Unity project (Assets/, Packages/, ProjectSettings/ committed;
                   Library/, Temp/, Logs/, UserSettings/ ignored)
    Assets/_Project/
      Scripts/Runtime/   beat sync, spring bones, etc.
      Scripts/Editor/    builder scripts, capture tool
      Scenes/  Sets/  Characters/  Timeline/  Audio/
  blender/         prop modelling: one folder per set, with a build script and .blend; props exported as FBX into unity/
  captures/        screenshots (committed) and test videos (local only, ignored by git)
```

## Director's controls
- **Every build task starts from a brief.** No brief, no build.
- **Claude never makes a creative call silently.** If the brief doesn't cover it, Claude asks.
- **Nothing is committed without the director seeing it** (screenshots or render).
- **Unity Editor is the director's to open and close.** Claude says when it needs the editor open (MCP) or closed (batch mode).

## Decisions
- MCP: **CoplayDev MCP for Unity**.
- Test videos stay **local only** (git-ignored); screenshots are committed.
- **Blender = prop workshop.** Props are modelled in Blender and exported as FBX into `unity/Assets/_Project/Sets/<set>/Props/`. Layout, camera, lighting and render stay in Unity, the single source of truth for the scene.

## Open questions
- MCP `execute_code` fails on any code in this project (CodeDom compile error "Line 1", Roslyn not installed). Until fixed, Claude builds through builder scripts and menu items, which is the repeatable route anyway. Fix (install Roslyn or update the package past v10.0.0), or leave it?

## Changelog
- 2026-10-10: Draft created.
- 2026-10-10: Approved. CoplayDev MCP; videos local only.
- 2026-10-10: Added the ref overlay tool; noted the `execute_code` failure.
- 2026-10-10: Blender's role decided: prop modelling only, FBX into Unity.
