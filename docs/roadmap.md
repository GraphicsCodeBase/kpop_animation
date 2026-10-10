# Roadmap: Who Does What

**Director (you):** decisions, references, notes, sign-off. **Crew (Claude):** research help, all building, rendering.
Each phase ends with a **gate**: nothing moves forward until the director signs off.

## Phase 1: Pre-production (paper only, no Unity yet)

| # | Task | Director | Crew | Output |
|---|---|---|---|---|
| 1.1 | Shot breakdown of original | Watch and fill the full shot list (time, section, set, outfit, shot size, camera, who's in frame) | Rough set map ✅, frame grabs on request | `04_song_structure.md` |
| 1.2 | Adaptation strategy | Decide faithful vs. reinterpreted, set by set | Lay out the trade-offs and costs | `00_vision.md` |
| 1.3 | Vision | Logline, concept, pillars, anti-goals | Ask questions, flag anything vague | `00_vision.md` |
| 1.4 | Moodboard | Collect references per set and per character; write "what to steal" | Grab frames, organise, analyse | `01_moodboard.md` |
| 1.5 | Duo design | Personalities, contrast and unity, looks, styling per set | Find candidate models (VRoid/BOOTH), check licences | `02_artist.md` |
| 1.6 | Set list | Pick which sets to keep, merge, reinvent; palette and mood per set | Sourcing plan (build vs. buy) per set | `03_sets.md` |
| 1.7 | Storyboard | Shot list for our version, with killing part and interaction moments | Turn it into a structured shot table | `05_storyboard.md` |
| **Gate A** | **Pre-production sign-off** | Approve all docs | Write build briefs from them | `briefs/` |

## Phase 2: Foundation (Crew builds, Director reviews)

| # | Task | Director | Crew | Output |
|---|---|---|---|---|
| 2.1 | Unity project | — | Unity 6.3 + URP, packages, folder layout | Project |
| 2.2 | Song timing | Confirm sections match the music | Beat/BPM/section analysis from audio | Beat map |
| 2.3 | Characters in engine | Approve look: shader, proportions | Import models, toon shader, spring bones | Test renders |
| 2.4 | Grey-box sets | Approve layout and camera angles | Blocked-out sets from the set list | Screenshots |
| 2.5 | Animatic | Approve timing, cuts, shot order | Timeline with grey-box sets, placeholder motion, Cinemachine shots | **Animatic render** |
| **Gate B** | **Animatic sign-off** | The whole MV works in rough form | | |

## Phase 3: Production (one set at a time)

| # | Task | Director | Crew | Output |
|---|---|---|---|---|
| 3.1 | Choreography | Choose and direct dance per section; note timing and feel | Source mocap, retarget, clean up, sync to beat | Animated shots |
| 3.2 | Set dressing and lighting | Notes on mood, colour, light | Final sets, lighting, practicals | Dailies |
| 3.3 | Hair, cloth, face | Notes on weight, expression | Physics tuning, expressions, blinks | Dailies |
| **Gate C** | Each set approved | | | |

## Phase 4: Post-production

| # | Task | Director | Crew | Output |
|---|---|---|---|---|
| 4.1 | Edit | Final cut decisions | Timeline cuts | Locked cut |
| 4.2 | Grade and effects | Look per set | URP Volumes, VFX Graph | Graded cut |
| 4.3 | Release | Thumbnail, title, teaser | Recorder exports | Final MV |
