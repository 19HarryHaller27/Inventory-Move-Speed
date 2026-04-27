# Inventory move speed

**Light pack, light feet. Heavy pack, heavy feet. Vintage Story 1.22+ (server-only code mod).**

---

## In plain English (for players)

**You don’t need to know how mods work to use this.** Here is the whole idea in normal words.

### What does this mod change?

It makes you **walk and run a bit faster** when your **main pack** is **pretty empty**, and **a bit slower** when that same pack is **pretty full**. That is it. There is no new button, no extra bar on the screen, and no new item. The game already has a **Character** screen (usually the **C** key) that shows how fast you move; this mod just nudges that number up or down based on your **backpack**—the **big** storage area when you open **Inventory and Crafting** (the **E** key).

### Which part of my stuff counts?

Only the **big grid** in that inventory window—the place where you stash loot, food, and spare tools when you are not using them. The **bottom row of quick slots** (the hotbar) is **not** part of that. If something is only on the hotbar, this mod does **not** count it as “on your back.” That is so the mod is about “how loaded is my **rucksack**,” not “am I holding a pickaxe in my hand.”

### How does it decide “empty” or “full”?

It does **not** care how *heavy* an item is in real life, and it does **not** add up how many **pieces** of wood or rock you have in a stack.

It only looks at **slots**: each box in your backpack is either **empty** or **has something in it**. If a slot has **one feather** in it, that slot counts as “used” the same as if you crammed **64 rocks** into that same slot. So the question is really: **of all your backpack boxes, how many have *anything* in them?**  
That is turned into a **percentage**: if **half** of the boxes have something and **half** are empty, you are at **fifty-fifty**—and that is the **middle** where this mod does **not** help or hurt your speed.

- **Lighter** than that (more **empty** boxes) → a **bonus** to move speed, up to a cap when **every** box is empty.  
- **Heavier** than that (more **used** boxes) → a **penalty** to move speed, up to a cap when **every** box has something in it.  
- **Right in the middle** (about half and half) → this mod is **neutral**; it is not trying to help or slow you for pack weight.

### How strong is the effect?

Roughly: for each **1%** your pack is **emptier** than that half-and-half point, you get a **small** speed **buff**; for each **1%** **fuller** than that point, you get a **small** speed **penalty**. In the **math** section below, the **biggest** buff and the **biggest** penalty are each in the **25%** ballpark in how the game **displays** movement (like other simple mods that tweak the same number). You do not have to think about the math while playing—just “**empty = zippier, jam-packed = slower**.”

### Where do I *see* it?

Open your **Character** window. The game already lists **walk** and **sprint** and can show different **sources** of bonuses and penalties. This mod adds one of those sources. There is no separate “encumbrance” window added by this mod.

### I opened E and the left side has no boxes at all

That is **normal for brand-new vanilla** until you get a **basket** or **bag** and put it in the **bag** slot. The **game** has to give you a backpack area first. This mod only runs when the game has that area; it does not create the boxes for you. Once you have them, the mod can start doing its job.

### Anything else I should know?

- **Multiplayer:** everyone who joins should have the mod if your group wants the same rules.  
- **Other mods** that also change run speed (hunger, health, other buffs) can **stack** with this one—the game **adds** the effects. If everything together feels too fast or too slow, that is a matter of **which mods** you use and **settings**, not a bug in “the mod forgot to work.”

---

## What it is

### The short version

**Inventory move speed** is a **server-side** mod that ties **how full your main backpack storage is** to **how fast you walk and sprint**. It does not add items, blocks, or a new window. The **Character (C)** screen already shows **stat layers** for `walkspeed` and `sprintSpeed`; this mod only adds or removes **one** named layer so the effect is **inspectable** in the same place the game already uses for “why am I this fast or slow.”

### Backpack-only, not “everything you own”

Vintage Story splits the player’s storage into **several** `IInventory` objects: hotbar, backpack grid, hand crafting, mouse cursor, and more. This mod **intentionally** uses **only** the inventory class the engine calls **“backpack”**—`GlobalConstants.backpackInvClassName` (the string `backpack` resolved per player). That is the main **E**-screen storage grid (plus whatever **extra** rows the game gives when you have **bags** equipped in the correct bag slots). The **hotbar** is a **different** `IInventory`; the mod **does not** look at it. If an item is only in the hotbar, it does **not** make your pack “heavier” in this mod’s terms.

**Why** “backpack” only, not every inventory at once? The design goal was **one** clear mental image: the **big** grid in the **Inventory and Crafting** window—the place that already reads as *“loose things I am hauling, not the bar I’m actively using to switch tools.”* Merging every inventory would complicate the rule, drag in the crafting output slot, the mouse stack, and other special cases. **Backpack-only** keeps the rule one sentence: *if it lives in the backpack grid, it encumbers; if it doesn’t, it doesn’t.*

### Slot-%, not stack mass

The mod does **not** sum item weights, `StackSize`, or `maxstacksize`. It only asks: **is this slot empty, or not?**  
If **any** `ItemStack` is present, the slot is **one** unit of “used.”

- A single **feather** in a slot counts the same as a **64-stack** of cobble.  
- That is **deliberately simple**—one boolean per slot, not a table of “grams per item.”

From that, it computes **fill percentage**:

- **used** = number of **non-empty** `ItemSlot`s.  
- **total** = `IInventory.Count` (how many **slots** exist in that backpack inventory *right now*).  
- **% full** = `100 × used / total`.

So you get a value from **0** (completely clear grid) to **100** (every single slot has at least one item, even if not full stacks). **Half and half**—half the slots with something, half **completely** empty—lands at **50%** in this system, which is the **neutral** pivot for the mod’s speed bonus or penalty (see *Numbers* below).

### Self-normalizing when your grid changes

When you equip a better **bag** in a bag slot, vanilla can **increase** the number of **backpack** rows. The same `IInventory` object’s **`Count`** can grow. The mod always divides **used** by **the current** `Count`, so:

- **Adding empty slots** (bigger pack) can **lower** your % full if you do not add more items.  
- You are never “stuck” at a wrong % because the UI grew—**one ratio**, always relative to *today’s* total slots.

### What happens every second, on the server

Once per **real** second (game tick listener, **1000** ms), the mod loops **online** players. For each, it fetches the **backpack** `IInventory`. It counts **used** and computes **% full**, then a single **float** to add to movement stats (or it **removes** the layer). **No** watched attributes, **no** custom packets, **no** client `ModSystem` in the stock design—**authority lies on the server**, and the **Character** sheet is driven by the same `Entity.Stats` the world already replicates. That is why there is **no** separate HUD: the “GUI” is **vanilla** documentation of stats.

**Edge case:** if there is **no** backpack inventory or **`Count` < 1**, the mod **clears** its stat layer and does not apply a bonus. That is normal when the game has not given you a grid yet (see *Purpose*, vanilla note).

**Multiplayer**  
All logic runs in **`EnumAppSide.Server`**. Clients that need to load the same `ModSystem` assembly in MP follow your usual `code` mod practice; the **server** is what applies movement, so a client without the mod will not *double* the rule, but the player experience is consistent when everyone has the mod installed the way the game expects for net code mods.

---

## Numbers

### The pivot: 50% full = neutral (for this mod)

- **Lighter** than 50% **slot fill** → the mod applies a **bonus** to walk and sprint.  
- **Heavier** than 50% **slot fill** → the mod applies a **penalty** of the same *shape* on the other side.  
- **Exactly** 50% (in ideal math) → the mod’s layer is **not** left as a “zero buff”; the implementation **removes** the layer so nothing from this mod applies.

**Linear ramp away from 50%:**  
for each **one percentage point** of fill **away** from **50%**, the movement add changes by **0.5%** in the **additive stat sense** the Character sheet and other mods in this project use. The constant in code is **`0.005f` per one percent** distance from 50%.

- **When % full is *below* 50:**  
  `moveAdd = (50 - percentFull) × 0.005`  
  Example: **0%** full (empty) → `50 × 0.005 = 0.25` → about **+25%** in that additive readout.

- **When % full is *above* 50:**  
  `moveAdd = -(percentFull - 50) × 0.005`  
  Example: **100%** full (every slot has something) → `-(50) × 0.005 = -0.25` → about **−25%** in the same readout.

- **Halfway examples** in between:

| Fill % (slot-based) | Distance from 50% | Resulting additive (approx) | In words |
|---------------------|------------------|-----------------------------|----------|
| 0% | 50% below | **+0.25** | **Maximum** light-pack bonus. |
| 10% | 40% below | +0.20 | Strong buff. |
| 25% | 25% below | +0.125 | Middle buff. |
| 40% | 10% below | +0.05 | Small buff. |
| 50% | 0 | **(layer removed)** | **Neutral** for this mod. |
| 60% | 10% above | −0.05 | Small penalty. |
| 75% | 25% above | −0.125 | Middle penalty. |
| 100% | 50% above | **−0.25** | **Maximum** “loaded” penalty. |

### Stacking and reading “%” in the UI

The values go through **`Entity.Stats.Set("walkspeed", ...)`** and the same for **`sprintSpeed`**, on a **dedicated layer name** (see *For modders*). The **Character** window breaks out layers so you can see *this* mod’s contribution. **Other** mods (hunger, health, kill streak, custom buffs) that add **different** layers **add**; they do not automatically overwrite one another. **Balance** in a modpack (total movement *too* fast or slow) is a **tuning** question, not a “hard conflict.”

**Floating point:** `percentFull` is computed as `100f * used / count`. A half-full grid in **integers** might be **50.0**; odd slot counts (e.g. 7 total slots) can give **repeating** percentages. If you need **no flicker** right at 49.5 / 50.5, a **dead band** in policy is a small, targeted change (see *For modders*).

---

## Purpose

### What we were going for

The goal was a **portable, readable** rule: **travelling light** feels **faster**; **hauling a packed grid** feels **slower**—all without inventing a new resource (“stamina to carry,” “pounds of ore,” database of item mass). The **game already** represents “where my stuff is” in **inventories**; this mod is *only* a **policy** on top of the **one** that best matches the fantasy of a **hiking pack** (the **backpack** `IInventory`).

### Why not weight, why not the hotbar

- **No universal weight in vanilla** as a first-class, player-facing number you can trust across every item. Slot occupancy is the **one** thing you can read everywhere with the same code path. **Feather = ingot** in a slot is a **gameplay** choice: it is easy to **predict**; it is **harsh** on the player who fills with light junk; a fork can replace `PackLoadPolicy` with a weighted model later without throwing away the project layout.

- The **hotbar** is the **tactical** strip—**tools in hand, quick blocks**. Mixing that into the same “encumbrance” can feel wrong (a sword in slot 1 does not need to be “in the rucksack”). The mod **punts** the question: **out of scope** = **not in backpack**.

### Pacing and decision-making in play

- Return trips, **chests** near the mine, and **sorting** become slightly more *meaningful* if you *feel* the run home when the grid is full.  
- **Early** in a run, a player who has **very few** backpack slots (small basket) hits **50%** *faster* in terms of *absolute* number of item stacks than someone with a huge endgame pack—**that is a consequence of** slot-% **design**, and it matches “early gear = tight” without special-case code for game stage.

### What it is *not*

- **Not** a **hunger** or **temporal stability** sim. It does not read the hunger tree.  
- **Not** a **combat** system (no damage, no armor in the rule).  
- **Not** a “realism” or **physics** mod.  
- **Not** a client-only “fake” speed: **server** applies the stat.  
- **Not** a replacement for **vanilla** design decisions (bags, stack sizes, how many reeds a basket costs).

### Vanilla: “I have no backpack slots at all”

If the **left** side of “Inventory and Crafting” is **completely** empty, **vanilla** has often **not** given you a **backpack** `IInventory` with a positive slot count—typically you need a **hand basket** (or other bag) **in a bag slot**. **This mod does not create the grid;** the engine does. With a **null** backpack inventory, or a backpack with **no** slots (`Count` is 0), the mod **clears** its layer. That is **intentional**; it is not the mod “turning off” the UI. **Fix** the empty panel in **play** (get a container equipped), not in *this* mod’s math.

**After** you have slots, **empty** rows mean **% full** can be **0%**—that is the **largest** movement **bonus** from this mod, which matches the fantasy of a **freed** pack.

---

## For modders

### File layout and responsibilities

| File | Role |
|------|------|
| `InventoryMoveSpeedServerSystem.cs` | Entry point: **`ModSystem`**, `ShouldLoad(Server)`, registers a **1 s** `OnServerTick` loop, resolves backpack via **`GetOwnInventory`**, iterates `ItemSlot`, applies **`Entity.Stats`** or **clears** layer. |
| `PackLoadPolicy.cs` | **Pure** math: `ComputeMoveAddForPercentFull(percentFull)` from **0–100** float. No game API, easy to **unit test** in isolation if you extract to a test project. |
| `PackLoadConstants.cs` | **`StatLayer` string** and **`MoveAddPerOnePercentFromHalf`** (default **0.005f**). Change the curve’s **slope** here, or the layer name, in one place. |
| `modinfo.json` | `type: "code"`, `modid`, **game** dependency, short description. |
| `InventoryMoveSpeed.csproj` + `Directory.Build.props` | Build, API reference, optional post-build **copy** next to `modinfo`, optional **deploy** to the user’s Mods directory. |
| `build.cmd` | Calls **`dotnet build -c Release`** from the mod root. |

There is **no** `ClientSystem`, **no** assets, **no** `assets/` folder in the default layout—**unless** you fork in a custom HUD; the design explicitly uses **vanilla** Character UI.

### Exact math (formulas in one place)

Let `used` and `count` be non-negative integers, `count > 0`:

```
percentFull = 100.0f * (float)used / (float)count
```

Then, with constant `K = MoveAddPerOnePercentFromHalf` (0.005 in stock):

- If `percentFull < 50` →  `add = (50f - percentFull) * K`  
- If `percentFull > 50` →  `add = -(percentFull - 50f) * K`  
- If `add == 0` (and in practice when **exact** neutral), **remove** the `walkspeed` and `sprintSpeed` **layers** for this mod. If `add != 0`, **`Set` both stats** to `add` with `persistent: false` on the named layer.

**Tuning the severity** without touching the server file’s structure: **only** `K` (e.g. **0.003** for gentler) or a **replaced** function in `PackLoadPolicy` (e.g. **exponential** falloff, **capped** max bonus).

### Dead band / hysteresis (flicker at 50%)

With **7** total slots, “half” is not an integer; `percentFull` can **oscillate** a few **tenths** around 50% when you move one stack. If that causes **visible** stat flapping, **clamp** the policy:

- e.g. treat `47–53` as **0**; or  
- require **N** seconds above/below before **committing** a new band; or  
- **floor** or **int**-ify the percent the way the hunger sample does for display (see `HungerEffectCurves` in the Hungereffects mod).

The stock code is **intentionally minimal**; a dead band is a **modder fork** concern, not a one-line fix without playtesting.

### Stacking with other mods on `Entity.Stats`

Vintage Story’s stats system uses **names** and **stackable** `StatLayer` entries. This mod’s layer is a **string constant**; **other** features should use **other** layer strings. **Summation** is the normal composition model—**HealthEffects**, **HungerEffects**, **Kill Streak**, and **Inventory move speed** can all be active; **net** `walkspeed` is the **sum of contributions** from each. If your pack is **over-tuned** for speed, the fix is **numeric**, not a merge conflict.

### Adding a second `IInventory` in a fork

If you also want the **offhand** or a **modded** inventory, **define** a clear rule: e.g. **separate** ratios, or a **single** `usedA + usedB` / `countA + countB`. Keep **all** `Stats.Set` on the **server** and one **place** that computes the final `add` so the client and server can’t disagree.

### Build, deploy, and API surface

- **Build:** from the mod directory, `dotnet build -c Release` or `build.cmd`. **`VintageStoryPath`** must point at a folder that contains `VintagestoryAPI.dll` (see `Directory.Build.props` or an override on the **build line**).  
- **Run folder:** the `.csproj` can copy the built **`InventoryMoveSpeed.dll`** to sit next to **`modinfo.json`**.  
- **User install:** `%AppData%\Roaming\VintagestoryData\Mods\InventoryMoveSpeed\` with at least the **DLL** + **modinfo**. **Disable** automatic deploy to AppData for CI by setting the environment or MSBuild property the project uses for `InventoryMoveSpeedNoDeploy`.  
- **PDB** optional, for your own **breakpoints** in a dev world.

**Engine types this mod actually touches:**

- `IPlayer` / `IServerPlayer`  
- `IPlayerInventoryManager` → **`GetOwnInventory(string)`**  
- `GlobalConstants.backpackInvClassName`  
- `IInventory`, indexer `this[int]`, `Count`  
- `ItemSlot` → **`Empty`**  
- `Entity.Stats` → `Set` / `Remove` with a **string layer** and **`persistent: false`**

That is a **short** list—good for **auditing** when a future game version renames a constant.

### No `System.Linq` in hot paths

The stock loop is **C-style** `for` and **no** LINQ—consistent with the constraints some Vintage Story mod setups have when the game’s **in-proc** compiler is involved. If you add helpers, keep **allocation** out of the **per player per second** path when possible.

---

## Flavor

**Tone:** the mod tells a small **fable of burden**: the path under your feet is **quicker** when the pack on your back is **mostly air**, and **slower** when you have **filled** every **pocket** the engine gave you, **one slot at a time**—a **cruel equality** for **feather** and **ingot** alike. There is no epic loot table, no “you are cursed with lead boots”; there is just **a grid**, a **pivot** at the halfway mark, and the **Character** sheet to **read** the truth. If you need **poetry** for a store page, it is **muddy boots and an honest pack**; the **prose** for a workshop page is **the tables in this file**.

---

## Credits

**Inventory move speed** — see **`modinfo.json`** for author string and version number. Game **Vintage Story** and **Anego Studios** own the **engine**, **IInventory** model, and **default** `backpack` behavior; this mod is a **thin** layer on that API.
