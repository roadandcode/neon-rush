# Performance

What Neon Rush costs to download, start and run today, and how each number was taken.

Nothing here has been tuned yet. These are first measurements, written down so that the size and rendering work to come has something to be compared against, and so that the claims in the README have numbers behind them.

## Test set-up

- The browser build: WebGL 2, IL2CPP, a 1280 × 720 canvas at device pixel ratio 1.
- Chromium 152 on Windows 11. Intel Core Ultra 9 285K, GeForce RTX 5070 Ti, 32 GB RAM.
- Served by a local web server on the same machine.

That is a fast desktop. The game is meant to hold 60 fps on an ordinary laptop or a phone, and nothing below shows that it does. It shows how much work a frame is. Slower hardware is the first thing under [Not measured yet](#not-measured-yet).

## Download

| File | Size |
| --- | ---: |
| `WebGL.wasm`, engine and game code | 11,526 KB |
| `WebGL.data`, scenes, shaders, audio, engine resources | 5,887 KB |
| `WebGL.framework.js` | 74 KB |
| `WebGL.loader.js` | 47 KB |
| Addressables: settings, catalog, track bundle, script bundle | 29 KB |
| **Total** | **17.2 MB** |

These are the sizes that go over the wire: the build gzips the three large files.

The build report shows what the data file is made of before compression. Of 9.0 MB of assets:

- 2.7 MB is the engine's splash screen logo.
- About 3.3 MB is textures URP brings along for effects the game doesn't use: ten film grain textures, the SMAA lookup tables and blue noise.
- 0.9 MB is sound. The two music loops are 0.38 MB each.
- 0.9 MB is shaders, most of them URP's.
- The game's own art barely registers. The bundle holding every hazard and pickup prefab is 23 KB.

So two thirds of the data is a logo and textures for effects that are switched off, and that is where a size pass starts. The code file is the bigger half and will take stripping and build settings to move.

## Start-up

| | |
| --- | ---: |
| First frame on the canvas | 0.8 to 1.1 s |
| Title screen up | 3.0 to 3.5 s (seven loads) |

Timed from navigation, in the page, by sampling the canvas each frame. The two seconds in between are spent behind the engine's splash screen.

One thing makes this better than a player's first visit and one makes it worse. The files came from the same machine, so there is no download in it: the 17 MB arrives in a quarter of a second. At 20 Mbit/s the download alone would add about seven seconds (arithmetic, not a measurement). Against that, my local server sends the gzipped files without a `Content-Encoding` header, so the loader unpacks them in JavaScript, which a host that sets the header leaves to the browser.

## Frame cost

Main-thread time inside the browser's frame callback: everything the engine does in a frame on the CPU, including game logic, UI and issuing the draw commands. Draw calls and triangles were counted by wrapping the WebGL draw functions.

| | Title screen | In a run |
| --- | ---: | ---: |
| Frame time, median | 0.7 to 1.0 ms | 0.9 to 1.1 ms |
| 95th percentile | 1.0 to 1.3 ms | 1.2 to 1.3 ms |
| 99th percentile | 1.1 to 1.5 ms | 1.4 to 1.6 ms |
| Worst frame | 1.6 ms | 2.2 ms once warm, 18.5 ms in a first run |
| Draw calls | 26 | 35 to 38 typical, 47 at most |
| Triangles | 1,661 | about 1,200, 1,313 at most |

The title screen is four samples of three to four seconds. The run column is five whole runs, from the start of the run to the crash, between 376 and 730 frames each. A range is the spread between samples.

On this CPU a frame uses about a millisecond of a 16.7 ms budget.

The worst frames split cleanly in two. The first run after the page loads has one or two long frames: the worst were 17.4, 5.6 and 18.5 ms in the three first runs I measured (I was also saving screenshots off the canvas during the 17.4 ms one). The second and third runs of a session had none, with worst frames of 2.2 and 2.0 ms. So something is paid for the first time it is used in a run, and on a slower machine that one frame would be a visible hitch. I have not found out what it is; pools growing and effects or shaders being used for the first time are the candidates. Doing that work behind the loading screen is on the list below.

What this does not give is a frame rate. The test page drove frames from a timer, so the pacing was the timer's, not the display's.

The draw call count includes the post-processing chain (bloom, tonemapping, vignette), which I have not separated from the scene itself.

## Garbage

The rule for this project is that a frame of a run allocates nothing. For the game's own code that has been a test result from the start: the message bus, the state machine, and a warmed-up player, track and scoring loop each run under `Is.Not.AllocatingGCMemory()`.

Measuring whole frames in play mode showed that was not the whole story.

### The HUD was allocating

The score display was written to avoid garbage: one label per digit, each set to one of ten constant strings, so no string is ever built. Its allocation test passed. In a real run it allocated every time the score moved anyway, about fourteen times a second.

The cause is UI Toolkit, not the string. Once a label is on a live panel, changing its text allocates, whatever the text is. A unit test builds the elements without a panel, so it never saw it. I measured a few ways of changing one element per frame on a live panel, as bytes on top of an idle frame:

| Change | Bytes per frame |
| --- | ---: |
| `label.text`, even to a cached string | 364 |
| Toggling a class that changes `display` | 742 |
| `style.display` | 546 |
| `style.visibility` | about 90 on average |
| `style.translate` | 0 |
| `style.opacity` | 0 |

The sizes depend on the element. A plainer label cost half as much per text change, so the figures are a guide. The split between what costs something and what costs nothing is the part I rely on.

So the display is now a row of reels. Each digit is a cell that clips a column of the ten digits, and showing a number slides the columns with `style.translate`. Label text is set once, when the strip is built. Cells are still shown and hidden when the number gains or loses a digit, which happens a handful of times in a run.

### Before and after

Play mode in the editor, frames compared against an idle frame of the same session:

| | Frames that allocated extra | Extra bytes |
| --- | ---: | ---: |
| Text labels, 4.5 s of a first run | 91 | 40 KB |
| Reels, the same 4.5 s | 31 | 7.6 KB |
| Reels, a whole second run (9.4 s, 47,033 frames) | 50 | 3.6 KB |
| Reels, a whole third run (8.5 s, 42,206 frames) | 42 | 3.1 KB |

Most of what is left is not the game. The test harness allocates 72 bytes about five times a second by itself: it shows up at that rate in an empty scene with none of the game loaded, and on the title screen with nothing happening. Take that out and the second and third runs have six and two frames that allocated anything, 456 and 208 bytes in total across the whole run. I have not traced those.

The first run of a session also has a few one-off allocations of 2.6 to 12.5 KB that later runs do not, which is what pools growing as each kind of obstacle first appears would look like. I have not confirmed that is all of it.

Pickups got their own check, since one fires a sound, a particle burst and a multiplier change at once. I sent 145 of them at the live game in a second and a half. 143 of the frames carrying one allocated exactly what an idle frame does. The other two coincided with one of the first-run allocations above, which turn up on frames without a pickup as well.

### What guards it now

- The EditMode allocation tests, as before, for the game's own code.
- A PlayMode test that boots the real game, counts the score up on the real HUD for 240 frames and compares them with 240 idle frames. A typical frame must match an idle one exactly, and the average cost per change must stay far below what a label text change costs. This is the test that would have caught the original problem.

All of these numbers are from the editor. UI Toolkit is managed code, so I expect a build to behave the same way, but I have not measured garbage in the browser.

## Not measured yet

- **Slower hardware.** A mid-range laptop with integrated graphics, and a phone. This is the measurement that matters most and I don't have it.
- **GPU time.** Everything above is main-thread time.
- **Frame pacing** against a real display refresh, and dropped frames.
- **Memory** in the browser: heap size and peak.
- **Garbage in the browser build**, as opposed to the editor.
- **Load time over a network**, first visit against a cached one.
- **Windows and Android.** Both build from the same code and are parked until the game is settled.

## Known, and not done yet

- **Size.** The splash logo, the unused URP textures and shader variants, managed code stripping, Brotli in place of gzip, and audio import settings have all been left as they came.
- **Rendering settings are the URP template's.** The browser gets the template's Mobile tier: 0.8 render scale and no anti-aliasing, which is the stair-stepping on the runner's outline in the screenshots. Each platform profile needs settings chosen for it.
- **A console warning.** The browser build logs that URP's FSR upscaling shader is not supported. Bloom, tonemapping and the vignette are visibly applied, and I have not checked what the warning changes.
- **First-run costs are paid during the run.** One long frame and a few allocations in the first run after loading, none in later runs. Whatever they turn out to be, the fix is to do that work behind the loading screen.
