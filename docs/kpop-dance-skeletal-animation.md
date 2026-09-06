---
title: K-Pop Dance Performance — Skeletal Animation in Unreal Engine
status: idea
plausibility: high (only if phased — see Feasibility)
tags: [project-idea, unreal-engine, skeletal-animation, mocap, cloth-sim, technical-art, portfolio]
created: 2026-08-16
---

# K-Pop Dance Performance — Skeletal Animation in Unreal Engine

## Origin
Been deep in K-pop dance practice/performance videos — ILLIT, TWICE, ITZY. What's striking isn't any one
dancer, it's the *group* precision: synchronized timing, formation changes, and the way costumes move
(skirts, sleeves, hair) selling the impact of every hit.

Can't dance. Want to produce that same feeling through animation instead. Also saw the MMD scene — people
already recreate K-pop choreo with distributed motion files and community models. That proves the motion
data problem is solvable; the interesting work is everything downstream of it.

Real goal underneath the cool video: **learn skeletal animation properly** — rigging, skinning, retargeting,
IK/FK, animation blending, and cloth simulation — in Unreal Engine.

## Goal
1. Start with untextured/blockout mannequins so the focus is purely on motion
2. Get one character dancing a real K-pop routine, correctly, on beat
3. Build outfits on top with **real cloth simulation** — flowing skirt, loose sleeves, hair with physics
4. Expand to a group with formation changes
5. Cut a camera-choreographed video that reads as a performance, not a tech test

## Feasibility

**Verdict: highly plausible, with one condition — do not start with the group.**

Every part of this has a well-trodden path. Nothing here needs research-grade work. The risk is entirely
scope: a full 3-minute group number is 10-20x the work of a 30-second solo, and the *learning* is
front-loaded into that first 30 seconds. Solo-first, then multiply.

**Genuinely easy (well-documented, days not weeks):**
- Getting a rigged humanoid into UE5 and playing an animation on it — Mixamo/MetaHuman/UE Mannequin
- Retargeting animation between skeletons — UE5's IK Rig + IK Retargeter is built for exactly this
- Sequencer for camera work and audio sync — this is what it's for
- Duplicating one dancer into five (the *animation* is trivially copyable; formations are the work)

**Genuinely hard (where the weeks go):**
- **Motion cleanup.** This is the real project. Whatever your motion source, you get foot sliding, jitter,
  limb interpenetration, and a root trajectory that drifts. K-pop choreo is fast, sharp, and full-body —
  it exposes bad motion data brutally. Budget more time here than on everything else combined.
- **Cloth simulation tuning.** Getting a skirt to *not* explode, clip through legs, or look like wet
  cardboard is fiddly, iterative, and hard to shortcut. A dancer's fast leg movement is close to the worst
  case for cloth solvers.
- **Skinning weights.** If you build your own character, hips/shoulders/knees deform badly by default.
  Painting weights well is a craft skill that takes real repetition.
- **Sharpness / snap.** Mocap and video-derived motion smooth out exactly the accents that make K-pop read
  as K-pop. Recovering that punch means hand-editing curves — which is, conveniently, the skill you said
  you want to learn.

**What could kill it:**
- Scoping to a full group number first and burning out at 20% done — **the single most likely failure**
- Machine can't handle multiple cloth-simmed characters in realtime (mitigation below — it's survivable)
- Getting stuck chasing photoreal characters instead of shipping motion. Stylized/blockout is fine and
  arguably reads better for an animation reel
- Music copyright blocking public posting (see Legal & Practical — has an easy workaround)

**Prerequisite check:** you don't need to be able to dance. You do need to be able to *see* timing — count
8s, spot when a hit lands early or late. That's learnable in an afternoon by scrubbing a practice video
frame by frame, and it's worth doing before any of the rest.

## The Core Decision: Where Does the Motion Come From?

Everything else hangs off this. Three routes:

### Route 1 — Hand keyframe from scratch
Watch reference, pose the rig, key it out.
- **Learning value: highest.** This is animation, actual animation.
- **Cost: brutal.** A 3-min K-pop routine at performance quality is months of full-time work for a trained
  animator. Not viable for the whole piece.
- **Correct use: an 8-count.** ~4 seconds. Do this once, deliberately, as a learning exercise.

### Route 2 — MMD motion files (.VMD) retargeted into UE
The community route. Motion already exists for a huge number of K-pop songs.
- Path: Blender + `mmd_tools` addon → import `.pmx` model + `.vmd` motion → bake → export FBX → UE5 IK
  Retargeter → UE5 skeleton.
- **Pro:** motion is already hand-polished by the MMD community, often sharper than raw mocap because a
  human tuned it. Fastest route to something that actually looks good.
- **Con:** MMD skeletons use Japanese bone names and a different hierarchy — retargeting takes real setup.
  Also check redistribution terms on any motion file before publishing; many carry author restrictions.
- **Learning value: solid** — retargeting is a genuinely employable skill, and cleanup is still on you.

### Route 3 — Video-to-motion (AI pose extraction)
Feed a dance practice video in, get skeletal animation out.
- Tools worth trialing: Rokoko Video, DeepMotion, Move One, Plask (all have free/trial tiers). Open-source
  monocular pose research also exists if you want to go deeper.
- **Pro:** works for *any* video, including ones with no MMD motion. Feels like magic the first time.
- **Con:** output quality is the weakest of the three. Foot sliding, depth ambiguity, and lost sharpness
  are the norm. Multi-person video is significantly worse than single-dancer footage.
- **Learning value: high but indirect** — you'll learn cleanup, hard, because you'll have no choice.

### Decision
**Hybrid.** Route 2 as the production spine, Route 1 as the deliberate learning exercise, Route 3 as a
fallback and as a comparison experiment.

Concretely: hand-keyframe one 8-count from scratch (Phase 2) so you've genuinely animated. Use VMD-retarget
for the full-length piece. If your target song has no usable VMD, fall back to Route 3 + heavy cleanup.

Bonus: running the same 4 seconds through all three routes and comparing them side by side is a *strong*
portfolio artifact on its own — it demonstrates pipeline judgment, not just tool use.

## Division of Labor

Same principle as the fluid sim project: **tooling and plumbing to Claude Code, craft to me.** If the
animation curves and cloth tuning get handed over pre-done, the learning is lost and there's nothing to
talk about in an interview.

**Claude Code writes:**
- UE project setup, folder structure, source control ignore rules
- Blender Python scripts for the MMD → FBX batch pipeline (bone renaming, unit/scale fixes, bake settings)
- IK Rig / IK Retargeter config scaffolding and any repetitive asset setup
- Animation Blueprint boilerplate (state machine skeleton, slot setup)
- Sequencer setup scripts — audio track, beat-marker track, camera cut scaffolding
- A "known-good" reference: a Mixamo dance on the UE Mannequin, working end to end, before any of my work
  enters — so a broken result is provably *my* change and not a broken project
- Formation/blocking helper: spawn N dancers at marked positions, drive them from one animation asset
- Render/export automation (Movie Render Queue config, frame ranges, output settings)

**I do, hands on:**
- All motion cleanup — foot planting, root trajectory, jitter, interpenetration
- The hand-keyframed 8-count, start to finish
- Skinning weight painting on anything I build myself
- Cloth asset setup and every round of parameter tuning
- Beat alignment and timing accents — making it hit
- Camera choreography and shot timing
- Look dev: lighting, materials, staging

## Components Needed

**Have / free — no blocker:**
- [ ] Unreal Engine 5 (free) — use 5.4 or newer for the Chaos Cloth Asset editor; verify current version
- [ ] Blender (free) + `mmd_tools` addon — required for the VMD route
- [ ] UE5 Mannequin (ships with engine) — the blockout model you described, already there
- [ ] Mixamo (free) — rigged characters and test animations for pipeline validation
- [ ] MetaHuman (free) — if/when you want a real character instead of blockout
- [ ] Sequencer + Movie Render Queue (in-engine) — camera work and final render

**Need to source:**
- [ ] Target song + routine — pick one where reference footage is clean and frontal
- [ ] Motion data — VMD file, or a mocap trial account, or a good practice video for Route 3
- [ ] Royalty-free audio for the publishable cut (see Legal & Practical)
- [ ] Outfit mesh — Blender-modeled yourself, or a marketplace/CC0 asset. Modeling it yourself is a
      separate skill tree; not required for the animation goal

**Knowledge to acquire (roughly in order):**
- [ ] Skeletal mesh vs. skeleton vs. animation asset — UE's data model, non-negotiable foundation
- [ ] IK Rig + IK Retargeter workflow
- [ ] Animation Sequence editing in-engine, and Control Rig for pose adjustment
- [ ] Additive animation and animation layers — how to fix motion without destroying the base
- [ ] Curve editing — where sharpness gets recovered
- [ ] Chaos Cloth: cloth asset creation, painting max-distance/stiffness maps, collision setup
- [ ] Groom/hair basics — only if hair physics makes the final scope
- [ ] Sequencer camera work, and Movie Render Queue for final output

**Hardware reality check:**
Cloth sim on multiple characters at once is the load-bearing cost. If realtime chokes with 5 dancers,
you're still fine — Movie Render Queue renders offline frame by frame, so a slideshow in the viewport can
still export a smooth 60fps video. Note this early so a slow viewport doesn't read as project failure.

## Roadmap

Each phase ends in something **recordable**. If a phase has no visible output, it's structured wrong.

### Phase 0 — Prove the pipeline (weekend, ~8-12 hrs)
Mixamo character + Mixamo dance animation, playing in UE5, camera orbiting, rendered to an MP4. Zero
original work. Purpose is to validate that every link in the chain works before anything of yours depends
on it.
> **Output:** 10-second clip. Ugly and generic — and that's fine, it's a control.

### Phase 1 — Retarget real choreo onto the mannequin (1-2 weeks)
Pick the song. Get motion (VMD via Blender, or video-to-motion). Retarget to the UE5 Mannequin. Sync to
audio in Sequencer. Do a first cleanup pass on foot sliding and root drift.
> **Output:** 30 seconds of blockout mannequin dancing a recognizable routine, on beat. *This is the real
> milestone.* Everything after it is craft on top of a working spine.

### Phase 2 — Hand-animate an 8-count (1-2 weeks)
Pick 4 seconds of the routine. Delete the retargeted motion for that range. Key it yourself from reference,
using Control Rig. Compare yours against the retargeted version side by side.
> **Output:** split-screen comparison clip. This is the single most interview-valuable artifact in the
> project — it proves you can actually animate, not just move files between tools.

### Phase 3 — Costume + cloth simulation (2-3 weeks)
Attach an outfit to the character. Set up Chaos Cloth on the skirt/sleeves. Paint constraint maps.
Configure collision against the body. Then tune, and tune again.
> **Output:** slow-motion clip focused on fabric during a spin or fast direction change. The money shot.

### Phase 4 — Polish the solo (1-2 weeks)
Lighting and stage environment. Materials. Camera choreography with real cuts. Second full motion cleanup
pass now that you know what to look for. Hair physics if it's still in scope.
> **Output:** complete, finished solo performance video. **A valid stopping point** — this alone is a
> portfolio piece.

### Phase 5 — Group + formations (3-4 weeks)
Multiple dancers. Formation changes and position transitions. Per-dancer timing offsets so they don't read
as clones. Camera work that covers a group instead of one body.
> **Output:** the full group performance video.

**Total: roughly 2-3 months of consistent evening/weekend work to Phase 4. Phase 5 pushes it to 3-4.**
Treat Phase 4 as the real finish line and Phase 5 as an expansion, not a requirement.

## Technical Notes

- **Foot sliding is the #1 tell of amateur work.** It's what separates "moved a file into Unreal" from
  "animated." Learn IK foot planting early and apply it aggressively — this single fix does more for
  perceived quality than anything else on this list.
- **Retarget onto the UE5 Mannequin first, always.** It's the best-documented skeleton in the ecosystem
  and every tutorial assumes it. Move to MetaHuman or a custom character only after the motion is already
  correct on the Mannequin. Debugging a retarget and a custom rig simultaneously is a bad time.
- **Version your animation assets.** `Dance_v01_raw`, `_v02_footfix`, `_v03_sharpened`. Cleanup is
  destructive and you *will* want to roll back a pass.
- **Cloth: start over-constrained.** Pin the waistband hard, keep max-distance small, then loosen
  gradually. Starting loose gives you an exploding skirt and no idea which parameter caused it.
- **Add a beat-marker track in Sequencer** before doing any timing work. Aligning to visible beat markers
  is enormously easier than aligning by ear, and it's the same idea as the velocity visualizer in the
  fluid sim project — build the diagnostic view before you need it.
- **Character faces: don't sculpt a real idol.** Beyond the likeness concerns, face modeling is a
  completely separate skill tree that has nothing to do with your animation goal. Original or stylized
  characters, or MetaHuman presets.
- **Render offline, not realtime.** Movie Render Queue with high sample counts. Realtime viewport quality
  is not what you show anyone.

## Legal & Practical (matters — this gets posted publicly)

- **Music:** the recording is copyrighted. Posting to YouTube/LinkedIn with the original track risks
  Content ID claims, muting, or takedown — which would break the portfolio piece you built this for.
  Options: (a) publish with a royalty-free track cut to the same tempo, (b) keep the
  original-audio version as a local file you send directly to employers, (c) both. **Recommend both** —
  the choreo timing needs the real song while you work, so animate against the original and swap audio at
  export.
- **Choreography** itself sits in murkier territory than the recording, but you're not reproducing the
  video, you're producing an original animation. Credit the original group, song, and choreographer in the
  description. That's both correct and professional.
- **Likeness:** original characters, not modeled versions of real idols. Avoids the issue entirely and
  saves you weeks of face work you don't need.
- **Motion files:** check redistribution terms on any VMD you use. Many MMD-community motions carry
  explicit author conditions. Credit the motion author.

## Demo Deliverable

**Primary — the performance video (60-90 sec):**
Full routine (or the strongest section of it), multiple camera angles, cuts on the beat, costume with
visible cloth sim. Aim for one deliberate slow-motion moment where the fabric sells the movement. Should
read as a *performance*, not a tech demo. This is the version that goes on LinkedIn.

**Secondary — the breakdown video (60-90 sec):** *this is the one that gets you hired.*
- Blockout mannequin → retargeted → cleaned → costumed → lit, as a progression on the same 4 seconds
- The hand-keyframed 8-count next to the retargeted version
- Cloth sim on/off toggle
- A moment of before/after foot-slide cleanup
- Screen-captured pipeline: IK Retargeter, Control Rig, cloth constraint painting

The performance video shows taste. The breakdown shows competence. Studios hire on the second one.

**What it proves to a hiring manager:**
Skeletal animation fundamentals, retargeting across skeleton hierarchies, physics/cloth setup, multi-tool
pipeline (Blender ↔ Unreal), Sequencer and cinematography, and — via the breakdown — an actual understanding
of *why* each step happens rather than tutorial-following. That maps directly to technical artist, animation
programmer, and gameplay-animation roles.

## Status / Next Steps
- [ ] Pick the target song and routine — prioritize clean frontal practice-video footage
- [ ] Check whether a usable VMD motion exists for it (decides Route 2 vs Route 3)
- [ ] Install Blender + `mmd_tools`, install UE5, confirm versions
- [ ] Phase 0: Mixamo character + animation → UE5 → rendered MP4 (prove the chain end to end)
- [ ] Watch the reference video frame by frame; mark the 8-counts and the accent beats
- [ ] Phase 1: first retarget onto the UE5 Mannequin
- [ ] Decide after Phase 4 whether the group expansion (Phase 5) is worth it

## Reference
- Unreal Engine docs — IK Rig and IK Retargeter (the core retargeting workflow)
- Unreal Engine docs — Chaos Cloth / Cloth Asset editor
- Unreal Engine docs — Control Rig, and Movie Render Queue
- Blender `mmd_tools` addon — PMX/VMD import, the MMD → FBX bridge
- Mixamo — free rigged characters and animations, for pipeline validation
- Related in this vault: [[gpu-fluid-smoke-sim]] — same division-of-labor principle (tooling delegated,
  craft kept), and the same "build the diagnostic view early" lesson
