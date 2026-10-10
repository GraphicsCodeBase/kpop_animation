# K-pop MV — Production Bible

A solo-artist K-pop music video, made in Unity.
**Director (you):** vision, every creative decision, sign-off on every stage.
**AI crew:** all technical work — building sets, rigging, physics, choreography integration, lighting, camera, rendering.

**Everything happens in Unity:** sets, animation, camera, editing, grading, rendering.
- Sequencing & edit: **Timeline** (the MV is one Timeline; shots are Cinemachine clips, cuts happen in Timeline)
- Camera: **Cinemachine**
- Grade & post FX: **URP Volumes** (one look profile per set)
- Particles: **VFX Graph**
- Final output: **Unity Recorder** renders the finished MV straight out of Unity
- Only inputs from outside: the song, the character model, and mocap clips

Every file has **Open questions** and a **Changelog**. Nothing is final until the director signs off.

## Production pipeline

| Stage | Director's deliverable | What the AI crew builds from it | Status |
|---|---|---|---|
| **Pre-production** | | | |
| 1. Concept | [00_vision.md](00_vision.md): logline, artist concept/era, pillars, anti-goals | — | 🟡 |
| 2. References | [01_moodboard.md](01_moodboard.md): annotated references | — | ⚪ |
| 3. Artist | [02_artist.md](02_artist.md): identity, look, styling per set | Character model setup, outfits, physics | ⚪ |
| 4. Song | [04_song_structure.md](04_song_structure.md): section map, killing part, dance break | Beat map, timing grid | ⚪ |
| 5. Sets | [03_sets.md](03_sets.md): each set's world, palette, lighting mood | Set builds in Unity | ⚪ |
| 6. Storyboard | 05_storyboard.md: shot list, framing, cuts per section | Camera rigs, shot setups | ⚪ |
| 7. Animatic | Review and approve the rough timed cut | Grey-box animatic render | ⚪ |
| **Production** | | | |
| 8. Performance | Choose and direct choreography per section | Mocap sourcing, cleanup, retargeting | ⚪ |
| 9. Shoot | Review dailies (renders) and give notes | Lit, animated shots rendered | ⚪ |
| **Post-production** | | | |
| 10. Edit | Final cut decisions | Assembly, cuts on beat | ⚪ |
| 11. Grade & VFX | Look per set, finishing touches | Colour grade, particles, post FX | ⚪ |
| 12. Release | Thumbnail, title card, teaser cut | Exports | ⚪ |

## Roadmap & briefs
- [roadmap.md](roadmap.md): who does what, phase by phase, with sign-off gates
- [briefs/](briefs/_template.md): one work order per build task; this is how the crew is prompted

## Director's log
[direction_log.md](direction_log.md): every decision with its *because*.

## Archive
[archive/](archive/README.md): earlier UE5 skeletal-animation version of the project. Superseded; history only.
