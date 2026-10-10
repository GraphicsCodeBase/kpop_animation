# MASTER PROMPT

> Status: 🔴 Not ready · Target: complete at **Gate A** (pre-production sign-off)
>
> This is the document Claude Code executes from to build the MV. When it's ready, the build starts with:
> **"Read `docs/MASTER_PROMPT.md` and begin Phase 2."**
>
> It is a **summary and contract**: each section states the essentials and links to the detailed doc. Detail lives in the linked docs, not here.

## Readiness checklist
A section is ready when it is specific enough that a crew could build it **without asking the director a creative question**.

| # | Section | Source doc | Status |
|---|---|---|---|
| 1 | Mission & roles | this file | 🟢 |
| 2 | Creative north star | [00_vision.md](00_vision.md) | 🔴 |
| 3 | Cast | [02_artist.md](02_artist.md) | 🔴 |
| 4 | Song & structure | [04_song_structure.md](04_song_structure.md) | 🔴 |
| 5 | Sets | [03_sets.md](03_sets.md) | 🟡 original mapped, ours undecided |
| 6 | Shot list | 05_storyboard.md | 🔴 |
| 7 | Look & motion rules | [01_moodboard.md](01_moodboard.md) + above | 🔴 |
| 8 | Technical stack | this file | 🟢 |
| 9 | Build order & briefs | [roadmap.md](roadmap.md), [briefs/](briefs/_template.md) | 🔴 |
| 10 | Review protocol | this file | 🟢 |
| 11 | Definition of done | this file | 🟡 |

---

## 1. Mission & roles
Build a director-led, anime-style cover MV of "Switch to me" (나로 바꾸자, DAHYUN & CHAEYOUNG) with two original characters, entirely in Unity.
- **Director (user):** owns every creative decision. When a creative choice isn't covered by this document, **ask; don't invent**.
- **Claude Code (crew):** does all technical work. It may make technical choices freely, as long as they serve the creative rules below.

## 2. Creative north star
_From 00_vision.md: logline, concept, the feeling (opening / peak / last frame), 3 pillars, anti-goals._

TBD

## 3. Cast
_From 02_artist.md: both characters (identity, look, movement), their contrast and unity, styling per set, interaction moments, model sources._

TBD

## 4. Song & structure
_From 04_song_structure.md: sections with timestamps and energy, killing part, signature moments._

TBD

## 5. Sets
_From 03_sets.md: our set list (keep / reinvent / cut), purpose, palette and lighting mood for each, hero set._

TBD

## 6. Shot list
_From 05_storyboard.md: every shot with time, set, outfit, shot size, camera move, who's in frame, action. Killing part and interaction shots flagged._

TBD

## 7. Look & motion rules
_Rules that apply everywhere: shading style, palette rules, lighting principles, how hair and cloth should move, cutting rhythm, music reactions. Each rule is checkable._

TBD

## 8. Technical stack (fixed)
- Unity 6.3 (`6000.3.0f1`), URP, anime toon shading (lilToon or MToon via UniVRM)
- Timeline (sequencing and edit), Cinemachine (cameras), URP Volumes (grade per set), VFX Graph (particles), Unity Recorder (final output)
- Spring-bone physics for hair and cloth; mocap clips retargeted and synced to the beat map
- External inputs only: song audio, character models, mocap clips

## 9. Build order & briefs
_From roadmap.md: Phase 2 foundation → animatic (Gate B) → Phase 3 per set (Gate C) → Phase 4 post. One brief per task in docs/briefs/._

TBD

## 10. Review protocol
- Nothing passes a gate without director sign-off.
- Crew delivers renders or screenshots; the director gives notes; notes are logged in the brief's review table.
- Every creative decision made during the build goes into [direction_log.md](direction_log.md).

## 11. Definition of done
- A full-length MV rendered from Unity Recorder, matching the approved shot list.
- _Director to add: quality bar (from the fidelity section of 00_vision.md), output format, resolution, frame rate._
