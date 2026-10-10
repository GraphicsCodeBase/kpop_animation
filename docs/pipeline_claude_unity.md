# Claude ↔ Unity Pipeline

> Status: 🟡 Draft, awaiting director approval · How the AI crew builds inside Unity, and how the director stays in control.

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

## MCP options (researched 2026-10-10)

| Option | Cost | Notes |
|---|---|---|
| **MCP for Unity** (CoplayDev, open source) | Free | Mature community plugin; Unity package with a built-in "register with Claude Code" button |
| **Unity's official MCP server** (Unity AI, open beta) | Needs a Unity AI trial/subscription; pricing unclear | Unity 6+, `com.unity.ai.assistant`, project must be connected to Unity Cloud; beta, may change |
| Others (IvanMurzak, CoderGamester, UniMCP4CC…) | Free | Smaller; check Unity 6.3 support |

**Recommendation:** start with **MCP for Unity (CoplayDev)**. It's free, built for exactly this, and nothing locks us in. Re-evaluate the official server once it leaves beta.

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
  captures/        screenshots and test renders (large renders ignored by git)
```

## Director's controls
- **Every build task starts from a brief.** No brief, no build.
- **Claude never makes a creative call silently.** If the brief doesn't cover it, Claude asks.
- **Nothing is committed without the director seeing it** (screenshots or render).
- **Unity Editor is the director's to open and close.** Claude says when it needs the editor open (MCP) or closed (batch mode).

## Open questions
- Approve the MCP choice (CoplayDev vs. official).
- Do large renders go in git (with Git LFS) or stay local only?

## Changelog
- 2026-10-10: Draft created.
