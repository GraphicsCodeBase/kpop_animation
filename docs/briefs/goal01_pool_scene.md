# Brief: Goal 1, Pool scene recreation

> Phase: Proof of concept · Status: **approved, except Intent** (✏️ director to rewrite in their own words) · **1A grey box signed off 2026-10-10** · **1C props: detail benchmark accepted 2026-10-10** · **1C mosaic tiles signed off 2026-10-18** (colour left for 1D)

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
| 2026-10-10 | 1A v1–v3: stairs built climbing away from the corner ("wrong direction") | ✅ Flipped in v4: they now climb toward the booth corner |
| 2026-10-10 | 1A v4–v5: stairs "not aligned to the edge"; compare with the ref and match it better | ✅ v6: layout and camera matched to `ref01_pool_01` with a 50% overlay (see Build notes) |
| 2026-10-10 | 1A v6 signed off: "looks good" | ✅ |
| 2026-10-10 | 1C props v1: props are primitive shapes; need more detail and texture | ✅ v2: kiosk and lounger remodelled in Blender as the detail benchmark |
| 2026-10-10 | 1C props v2 accepted: "good for now". Scene build proven; textures next session | ✅ |
| 2026-10-18 | 1C tiles v1 (crew check): random speckle read as noise, and at the ref's size the tiles blurred to flat colour | ✅ v2: dark tiles on an every-other-tile lattice, as in the ref; distance fade starts later |
| 2026-10-18 | 1C tiles v2 signed off: "the tiles read as that pool". Floor is brighter and bluer than the ref; leave the colour for 1D | ✅ (colour → 1D) |

## Build notes (crew)
- **Scene:** `unity/Assets/_Project/Scenes/Goal01_Pool.unity`. **Builder:** `KPop → Goal 1 → Build Pool Grey Box` (`Scripts/Editor/PoolGreyBoxBuilder.cs`); rebuilding replaces `Pool_GreyBox`.
- **Matching tool:** `KPop → Goal 1 → Capture Ref Overlay` renders the main camera at the ref's size (685×532) and blends it 50/50 with `ref01_pool_01` into `captures/_overlay/` (git-ignored). Use it again for the 1C and 1D checks.
- **1A screenshots:** `captures/goal01_pool/goal01_1A_greybox_v1…v6`.
- **Measured from the refs (estimates):** basin 17.3 × 12 m, 2.0 m deep. Lanes run parallel to the back wall, 2.5 m apart, the first 2.3 m out from it, and end in a T 1.8 m short of the right wall. Stairs: a 0.9 m-wide flight against the back wall, 8 steps (0.45 m run, 0.25 m rise), top step 0.2 m from the right wall. The booth stands about 1 m out from the back wall and hides the corner.
- **Camera `CAM_Wide_HighAngle`:** long lens (27° vertical FOV), about 5 m above the pool floor, tilted 32° down, from the front-left into the back-right corner.
- **Stand-ins:** two 1.65 m capsules for the dancers, until 1B. The grey prop blocks used to line up the shot in 1A were replaced by the Blender props in 1C.
- **1C props come from Blender** (pipeline decision 2026-10-10). Start from the props in `blender/pool/build_pool.py`, export them as FBX and place them on the stand-ins. Only the props come over; the Blender layout, camera, tiles and lighting don't.
- **1C props pass (v1–v2):** `blender -b blender/pool/pool.blend --python blender/pool/export_props.py` writes one FBX per prop (pivot at bottom centre) to `unity/Assets/_Project/Sets/Pool/Props/`. The builder places them; they import facing +Z, so they get a 180° turn. Kiosk scaled to 0.72 (≈1.3 m) and balloons to ≈0.6 m in Blender to match the ref. Not carried over yet: the parasol stripes (grey for now), balloon translucency, bunting, mosaic tiles.
- **1C mosaic tiles (v1–v2):** procedural shader `Shaders/Mosaic.shader` (`KPop/Mosaic`), laid out in world space so floor, walls and lanes share one grid with no UVs. The builder writes the materials `Sets/Pool/Materials/M_PoolMosaic` and `M_LaneMosaic` from the palettes in `PoolGreyBoxBuilder.cs`, so the script stays the source of truth. Measured from `ref01_pool_01`: 2.5 cm tiles; a pale aqua field (`#CCE6E3`, `#BEDDDA`) with dark teal tiles (`#5FA3AA`, `#4A93A0`) only on every other tile in both directions (a 5 cm lattice, 90% filled), so they never touch; lanes are a navy mosaic, 10 tiles wide, on the same grid. Matte, soft lighting (wrapped diffuse, softened shadows). Tiles fade to their average colour below ~1 px to stop shimmer. Screenshots: `captures/goal01_pool/goal01_1C_tiles_v1_*`, `…_v2_*`.
- **Batch captures:** `Unity.exe -batchmode -projectPath unity -executeMethod BatchCapture.Goal01Pool -quit` (editor closed) rebuilds the pool and writes the overlay, a 1080p wide and two close-ups to `captures/_overlay/`.
- **For 1D:** the floor renders brighter and bluer than the ref (ours ≈ `#CBEDF8` against the ref's `#AFCECC` on the near floor). Fix it with light and grade, not by repainting the tiles.
- **Next session (1C, rest):** parasol stripes and fringe, see-through balloons, bunting. Then bring the seats, side table, parasol and stair handrail (add posts) up to the kiosk and lounger detail level. Still untiled: the stair block's sides, which are tiled in the ref.
