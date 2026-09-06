---
title: K-Pop Dance Animation — Roadmap
type: roadmap
parent: "[[kpop-dance-skeletal-animation]]"
tags: [roadmap, unreal-engine, skeletal-animation, cloth-sim, webgpu]
created: 2026-09-06
---

# K-Pop Dance Animation — Roadmap

> [!info] Rule
> Every phase ends in something **recordable**. If it produces nothing filmable, it is structured wrong.

**Progress:** P0 ☐ · P1 ☐ · P2 ☐ · P3 ☐ · P4 ☐ · P5 ☐ · P6 ☐

---

## Phase 0 — Prove the Pipeline
`~8-12 hrs` · **Output:** 10-sec MP4, generic and ugly

**Purpose:** Validate every link in the chain before your own work depends on it.
**Context:** Zero original work. This is a control — if something breaks later, you know it was your change.
**Focus:** Getting to a rendered file. Not quality.
**How:** Mixamo character + Mixamo dance → UE5 → Sequencer → Movie Render Queue.

### Setup
- [ ] Install UE5 (5.4+ for Chaos Cloth Asset editor)
- [ ] Install Blender
- [ ] Install `mmd_tools` addon
- [ ] Create UE project, set folder structure
- [ ] Set up version control ignore rules

### Pipeline test
- [ ] Download Mixamo character + any dance animation
- [ ] Import FBX into UE5
- [ ] Play animation on character in level
- [ ] Add camera in Sequencer
- [ ] Add audio track to Sequencer
- [ ] Render to MP4 via Movie Render Queue
- [ ] Confirm file plays outside the engine

---

## Phase 1 — Retarget Real Choreo
`1-2 weeks` · **Output:** 30 sec of blockout mannequin dancing on beat

**Purpose:** Get correct motion onto a rig. This is the real milestone — everything after is craft on top.
**Context:** Blockout mannequin only. No costume, no lighting, no character.
**Focus:** Foot sliding and root drift. Nothing else matters yet.
**How:** VMD via Blender → FBX → UE5 IK Retargeter → UE5 Mannequin. Fall back to video-to-motion if no VMD.

### Pick the target
- [ ] Choose song + routine
- [ ] Confirm clean frontal practice-video footage exists
- [ ] Search for usable VMD motion file
- [ ] Check VMD redistribution terms + note author for credit
- [ ] Decide route: VMD retarget or video-to-motion

### Learn the timing
- [ ] Scrub reference video frame by frame
- [ ] Mark the 8-counts
- [ ] Mark accent beats / hits
- [ ] Pick the 30-sec section to build

### Get motion in
- [ ] Import PMX model + VMD into Blender via `mmd_tools`
- [ ] Bake animation
- [ ] Export FBX (check scale + units)
- [ ] Import FBX into UE5
- [ ] Build IK Rig for source skeleton
- [ ] Build IK Rig for UE5 Mannequin
- [ ] Configure IK Retargeter, chain mapping
- [ ] Export retargeted Animation Sequence
- [ ] Save as `Dance_v01_raw`

### First cleanup pass
- [ ] Add beat-marker track in Sequencer
- [ ] Align animation to audio
- [ ] Fix root trajectory drift
- [ ] IK foot planting — kill foot sliding
- [ ] Fix limb interpenetration
- [ ] Save as `Dance_v02_footfix`
- [ ] Render 30-sec clip

---

## Phase 2 — Hand-Animate an 8-Count
`1-2 weeks` · **Output:** split-screen comparison clip

**Purpose:** Prove you can animate, not just move files between tools. Highest interview value in the project.
**Context:** ~4 seconds only. Deliberate learning exercise.
**Focus:** Snap and accents — the sharpness retargeting smooths away.
**How:** Delete retargeted motion for the range, key it from scratch in Control Rig.

### Learn the tools
- [ ] Control Rig basics on the Mannequin
- [ ] Animation Sequence editing in-engine
- [ ] Curve editor — tangents, ease, holds
- [ ] Additive animation / layers

### Animate
- [ ] Pick the 4-sec 8-count
- [ ] Block key poses (extremes only)
- [ ] Add breakdowns
- [ ] Pass on timing — push accents early/late
- [ ] Polish arcs and spacing
- [ ] Save as `Dance_8count_handkeyed`

### Compare
- [ ] Set up side-by-side scene (yours vs retargeted)
- [ ] Render split-screen clip

---

## Phase 3 — Costume + Cloth Simulation
`2-3 weeks` · **Output:** slow-mo clip of fabric during a spin

**Purpose:** The money shot. Cloth is what sells the movement.
**Context:** Hardest tuning work in the project. Fast leg movement is worst-case for cloth solvers.
**Focus:** Stability first, beauty second. Start over-constrained, loosen gradually.
**How:** Chaos Cloth asset + painted constraint maps + body collision.

### Get an outfit on
- [ ] Source or model outfit mesh (skirt + loose sleeves)
- [ ] Skin outfit to skeleton
- [ ] Paint skinning weights (hips, shoulders, knees)
- [ ] Verify no clipping in bind pose

### Cloth setup
- [ ] Learn Chaos Cloth asset workflow
- [ ] Create cloth asset from skirt
- [ ] Pin waistband hard
- [ ] Paint max-distance map (small values to start)
- [ ] Paint stiffness map
- [ ] Set up body collision primitives
- [ ] Run sim — confirm no explosion

### Tune
- [ ] Loosen max-distance incrementally
- [ ] Tune damping
- [ ] Tune stiffness / bend
- [ ] Fix leg-clipping during high kicks
- [ ] Repeat on sleeves
- [ ] Record parameter values that worked

### Capture
- [ ] Pick the best spin / direction change
- [ ] Render slow-motion clip

---

## Phase 4 — Polish the Solo
`1-2 weeks` · **Output:** finished solo performance video

> [!success] Ship gate
> This alone is a valid portfolio piece. Stopping here is a win, not a failure.

**Purpose:** Turn a tech demo into a performance.
**Context:** First point where the project is presentable to an employer.
**Focus:** Camera and lighting do more for perceived quality here than more sim tuning.
**How:** Stage environment, materials, real camera cuts, second cleanup pass.

### Look dev
- [ ] Build simple stage environment
- [ ] Lighting pass
- [ ] Materials on character + outfit
- [ ] Hair physics (Groom) — only if scope allows

### Second cleanup pass
- [ ] Re-watch with fresh eyes, list every visible flaw
- [ ] Fix worst 5
- [ ] Save as `Dance_v03_sharpened`

### Camera
- [ ] Block camera shots against beat markers
- [ ] Cut on the beat
- [ ] Add the slow-mo fabric moment

### Deliverables
- [ ] Render performance video (60-90 sec) — original audio
- [ ] Re-export with royalty-free tempo-matched track for public posting
- [ ] Cut breakdown video: blockout → retargeted → cleaned → costumed → lit
- [ ] Breakdown: hand-keyed 8-count vs retargeted
- [ ] Breakdown: cloth on/off toggle
- [ ] Breakdown: before/after foot-slide fix
- [ ] Breakdown: screen capture of IK Retargeter, Control Rig, cloth painting
- [ ] Write credits (group, song, choreographer, VMD author)
- [ ] Post

---

## Phase 5 — Web Viewer (glTF)
`1 weekend` · **Output:** shareable link

**Purpose:** Get a clickable link, and prove the export path before betting a month on it.
**Context:** Stock viewer, no custom engine. Honest scope — not claiming to have written a renderer.
**Focus:** The export working. Not visual parity with UE.
**How:** Export glTF from UE → Three.js viewer with orbit controls → static host.

### Export
- [ ] Export character + baked animation as glTF/GLB
- [ ] Verify animation plays in a glTF validator/viewer
- [ ] Optimize mesh + texture size for web

### Viewer
- [ ] Three.js scene: load GLB, play animation clip
- [ ] Orbit controls
- [ ] Basic lighting + ground
- [ ] Play/pause + scrub
- [ ] Mobile check

### Ship
- [ ] Deploy to static host
- [ ] Embed the Phase 4 video on the same page as fallback
- [ ] Add link to portfolio + LinkedIn post

---

## Phase 6 — Branch: Pick One
`3-4 weeks` · **Decide after Phase 5, not before**

> [!warning] Decision gate
> **6A if targeting games studios. 6B if targeting graphics / creative-dev roles.**
> Do not start both.

### 6A — Group + Formations (UE)
**Purpose:** The full group performance — the original vision.
**Focus:** Per-dancer timing offsets so they read as people, not clones.
**How:** Duplicate the animation, offset it, choreograph formations in Sequencer.

- [ ] Spawn multiple dancers
- [ ] Block formation positions
- [ ] Animate formation transitions
- [ ] Add per-dancer timing offsets
- [ ] Vary outfits slightly
- [ ] Camera work covering a group
- [ ] Check perf — accept offline render if realtime chokes
- [ ] Render full group performance video

### 6B — Custom Web Engine + Cloth (from scratch)
**Purpose:** Prove you can write the systems, not just configure them.
**Focus:** Position-based dynamics cloth solver. Reuses GPU compute skills from [[gpu-fluid-smoke-sim]].
**How:** WebGPU compute shaders, skinned-mesh collision, hand-rolled solver.

- [ ] Check current WebGPU browser coverage
- [ ] Own skinned-mesh renderer (no engine helper)
- [ ] GPU skinning in compute/vertex shader
- [ ] PBD cloth solver — constraint solve on GPU
- [ ] Ping-pong buffers for cloth state
- [ ] Collision against skinned body
- [ ] Stability tuning under fast movement
- [ ] Frame budget pass — target 60fps mid-range laptop
- [ ] WebGPU support gate + video fallback
- [ ] Deploy
- [ ] Write technical breakdown post (the actual portfolio artifact)

---

## Parking Lot
- [ ] Same 4 sec through all 3 motion routes (hand / VMD / video-to-motion), side by side
- [ ] Secondary motion on hair
- [ ] Second song / second routine
