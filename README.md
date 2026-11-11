# CS464 Assignment 02 · Omer Abdullah · BSCS25135


- **Video:** https://drive.google.com/file/d/1yFtpo9TjgCE4adgG89BibpnSmfA4AMDA/view?usp=drive_link
- **Input:** Legacy Input (Horizontal/Vertical, Space), with Active Input Handling set to Both
- **Ground check:** Physics.CheckSphere at a Feet child, mask = Ground layer only, with a gizmo (green grounded, red airborne)
- **Camera:** SmoothDamp (smoothTime 0.25 s) with look-ahead, in LateUpdate
- **Juice:** Camera dip on landing, Ease.OutBack curve, started by the player's Landed event after more than 0.1 s in the air

## Tuning
| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | because its a sweet spot between walking and not sprinting while being fast enough, easier to steer around abstacles etc |
| Jump height | 1.8 m | i set my jump obstacles for this hence this number |
| Time to apex | 0.61 s | √(2h/g) = √(2·1.8/9.81) |
| Fall multiplier | 2.2× | Fall takes 0.41 s instead of 0.61 s, so the jump feels snappy; the apex stays the same |
| Coyote time | 0.1 s | At 5 m/s that is 0.5 m past the edge of a floor |
| Jump buffer | 0.12 s | At 5 m/s that is 0.6 m before landing, so a slightly early press still jumps |
| Camera smoothing | 0.25 s | sweet spot again doesnt lag behind  |
| Look-ahead | 0.35 s | At 5 m/s the camera aims 1.75 m ahead, so I see where I'm going |

## Four answers
1. **The jump, in numbers.** v = √(2gh) = √(2·9.81·1.8) = 5.94 m/s, so on a 1 kg body jumpForce is 5.94 (m·v). It takes 0.61 s to reach the apex and about 0.41 s to fall back (2.2× gravity falling), so about 1.02 s in the air. At 5 m/s one jump carries me about 5.1 m. and it easily covers my widest gaps.
2. **Update and FixedUpdate.** in Update i read the keys, build the camera-relative move vector, run the ground check, stamp the coyote and buffer timestamps, and queue the jump. in FixedUpdate i set rb.linearVelocity from the move vector, apply the queued jump once, and add extra fall gravity. if I swapped them, GetKeyDown would be missed on frames where FixedUpdate doesn't run (about two in three at 144 fps), so some jumps would be lost, and physics changes would run at the framrate.
3. **Vector3.Dot.** My script doesnt call it yet, but it could in PlayerController's Update, right after building move from the camera's axes: float away = Vector3.Dot(move, forward);, where forward is the cameras flattened forward direction. The sign say which way im walking relative to the camera: positive means I'm walking away from the camera (W), negative means toward it (S), and zero mean sideways (A/D only). It's the same idea as the forward * v term in my movement line, which scale the camera's forward by how much W/S is presed.
4. **The juice.** the camera dip uses Ease.OutBack over 0.35 s, which is about 21 frames at 60 fps. OutBack make the camera spring back past its rest position and settle. 0.35 s is long enough to see the overshoot but short enough to finish before my nxt jump.

## Levels

| Level | What the controller must do there | What I changed for this jump |
| --- | --- | --- |
| Level01 | Dodge the obstacles and follow the tall pillar to the goal; jump over the 1.1 m low covers where the path is blocked | Nothing. The 1.8 m jump clear the 1.1 m covers and there are no gaps |
| Level02 | Find a route through the maze past small and large obstacles to the goal; hop the 1.1 m low covers, walk around the 3 m walls | Nothing. The 3 m walls are ment to be walked around, not jumped |
| Level03 | Navigate the more complicated maze, following the pillar and the coins to reveal and reach the goal | Nothing. Camera-relative movement and the follow camera was enough here |
| Level04 | Weave through the obstacle field using only the coins as guidance, jumping the low covers | Nothing. The jump height already fit the obstacles |
| Level05 | Same obstacle route as Level01 with a changed layou | Nothing. The 1.8 m jump clears everything it ask for |