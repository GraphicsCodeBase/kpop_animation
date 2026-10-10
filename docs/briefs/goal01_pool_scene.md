# Brief: Goal 1, Pool scene recreation

> Phase: Proof of concept · Status: **approved, except Intent** (✏️ director to rewrite in their own words)

## Intent (Director) ✏️
_Your words: why this scene first, and what it should feel like._

Draft for you to rewrite: prove that we can build a set and have two characters dance in it, by recreating the S2 pool scene from REF-01 as closely as we reasonably can.

## Read first
- [03_sets.md](../03_sets.md): **S2 Pool: detailed breakdown** (zones, props, light, camera, styling)
- [refs/pool/](../refs/pool/): 8 reference frames
- [pipeline_claude_unity.md](../pipeline_claude_unity.md): how the build works

## Deliverable
A Unity scene of the pool, with two characters dancing, and a **10-second test render** from the high-angle wide shot (matching `ref01_pool_01`).

## Stages
| Stage | What | Director checks |
|---|---|---|
| 1A Grey box | Pool basin with correct proportions, stairs, camera matching the high-angle wide | Screenshot next to ref: does the space and angle read right? |
| 1B Dancers | Two placeholder characters dancing (VRM + Mixamo) | Test render: do they dance believably in the space? |
| 1C Dressing | Mosaic tile material, lane lines, props (simple shapes first), bunting, balloons | Screenshot next to ref: does it read as *that* pool? |
| 1D Light & grade | Soft high-key light, warm accents, cool airy grade | Screenshot next to ref: does the mood match? |

## Must have
- Zone A (the pool basin) with the lane lines, corner stairs and the main props.
- The high-angle wide shot.
- Two characters on screen at once.

## Must avoid
- Harsh shadows or a dark, contrasty look. This set is soft and airy.
- Plain flat-colour floor. The mosaic texture is what makes the set recognisable.

## Done when (checkable)
- Side-by-side with `ref01_pool_01`, someone who knows the MV recognises the location.
- Both characters dance for 10 s without sliding feet or passing through each other.
- The render comes out of Unity Recorder.

## Decisions (2026-10-10)
1. **Faithful recreation.** Copy the references as closely as possible; no reinterpretation in this test.
2. **Zone A only.** Zone B (curved stairs) comes in a later goal.
3. **Placeholder characters** (any two VRMs). The designed duo comes later.

## Freedom
- Crew may choose all technical methods (shaders, lighting setup, prop construction).
- Crew must ask before changing layout, colours or camera angle away from the references.

## Review notes
| Date | Note | Resolved |
|---|---|---|
