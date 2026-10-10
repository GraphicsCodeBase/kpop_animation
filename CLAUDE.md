# CLAUDE.md

Director-led anime-style K-pop cover MV of "Switch to me" (DAHYUN & CHAEYOUNG), a duo, built entirely in Unity.
The user is the **director** and makes every creative decision. Claude is the **crew and creative mentor**: it does all technical work and helps the director build design sense.

## Source of truth
- `docs/` is the production bible. Start at `docs/README.md` and `docs/roadmap.md`.
- `docs/archive/` is a superseded UE5 plan. Never treat it as current direction.
- The director also thinks in a Claude Project (claude.ai). Its decisions only become real once they're written into `docs/` here.

## Working with the director
- Don't make creative decisions for them. Ask, offer 2–3 options, and get their *because*.
- Push back on vague direction until it's specific enough to build from.
- Log every decision in `docs/direction_log.md` with its reason. Leave the *because* blank for the director if they didn't give one.
- Each doc has an **Open questions** section and a **Changelog**. Keep both current.

## "Process the inbox"
The Claude Project pushes handoffs to `docs/inbox/` directly, so run `git pull` first.
For each file in `docs/inbox/` (except `_handoff_template.md` and `done/`):
1. Read it, and apply the decisions and proposed changes to the right docs.
2. Ask the director about anything ambiguous or that contradicts an existing doc. Don't guess.
3. Add the decisions to `docs/direction_log.md`, and open items to the relevant Open questions.
4. Move the file to `docs/inbox/done/`.
5. Summarise what changed, then commit and push when the director confirms.

## Git
- Repo: `GraphicsCodeBase/kpop_animation`, branch `main`. The director wants commits pushed to `main`.
- The Claude Project also pushes (to `docs/inbox/` only), so always `git pull` before starting work.
- Show what's going in before committing. Use clear messages, e.g. "Add shot breakdown 0:00–0:30".
- If a push fails with an authentication error, ask the director to run `git push` in their terminal to sign in again. Never enter credentials.

## Build phase (after Gate A)
- Unity 6.3 (`6000.3.0f1`) with URP. Timeline for edit, Cinemachine for camera, URP Volumes for grade, VFX Graph, Unity Recorder for output.
- Each build task starts from a brief in `docs/briefs/` (template: `docs/briefs/_template.md`).
