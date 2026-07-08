# LetterGarden Handoff

## Project Snapshot

- Unity version: 6000.3.19f1
- Render pipeline: Universal Render Pipeline package is installed (`com.unity.render-pipelines.universal` 17.3.0)
- Current Git branch: `develop`
- Main dev scene: `Assets/Scenes/DevGameScene.unity`
- Prototype level data: `Assets/Resources/Levels/dev_levels.json`

## Current Gameplay Architecture

Core gameplay is Unity-independent under `Assets/Scripts/Core`:

- `PuzzleLevel` stores level ID, available letters, required words, bonus words, accepted words, and difficulty.
- `GameSession` manages the active level, current selected word, required/bonus found word sets, completion state, and word submission result.
- `LetterSelection` tracks selected letter indices and builds the current word.
- `WordValidator` remains available for basic case-insensitive word validation/found tracking.
- `WordSubmitResult` represents submit outcomes for invalid, already found, required found, and bonus found words.

The core scripts have an assembly definition at `Assets/Scripts/Core/LetterGarden.Core.asmdef`.

## Current UI Prototype

UI scripts are under `Assets/Scripts/UI`:

- `DevGameController` loads JSON levels, creates a `GameSession`, spawns letter buttons dynamically, handles drag-only input, auto-submits on drag end, updates status text, handles level completion, and manages next-level flow.
- `LetterButtonInput` forwards pointer down/enter/up events from spawned letter buttons into `DevGameController`.
- `RequiredWordBoardView` renders required words as letter tiles, showing hidden words with `_` until found.
- `LetterDragPathView` draws simple UI `Image` segments between selected letter buttons during a drag.

Current input model:

- Gameplay is drag-only.
- Submit, Clear, and Backspace buttons are no longer used.
- Starting a drag clears the previous current word.
- Dragging over new letters selects them if their index has not already been selected.
- Releasing the pointer auto-submits the current word.

Level complete behavior:

- The level complete panel appears when the final required word is found.
- Input is locked while the level complete panel is open.
- The letter wheel is hidden while the panel is visible.
- Keep Playing hides the panel, restores the letter wheel, unlocks input, and shows the persistent Next Level button.
- On the final level, pressing Next Level shows `No more levels yet.` while keeping the current level active so bonus words can still be found.

## Data

Current JSON levels:

- `level-1-cat`: letters `CAT`, required `CAT`, `ACT`, bonus `AT`
- `level-2-dog`: letters `DOG`, required `DOG`, `GOD`, bonus `DO`, `GO`
- `level-3-eat`: letters `EAT`, required `EAT`, `TEA`, `ATE`, bonus `AT`

Note: A dictionary-based bonus word system was discussed earlier, but the current workspace only shows `Assets/Resources/Levels/dev_levels.json`. No `Assets/Resources/Dictionaries/common_words.txt` file is currently present.

## Tests

Edit Mode tests exist under `Assets/Tests/EditMode`:

- `WordValidatorTests`
- `PuzzleLevelTests`
- `LetterSelectionTests`
- `GameSessionTests`

The tests target the Unity-independent core gameplay classes. The UI prototype scripts are not currently covered by automated tests.

## Current Folder Highlights

- `Assets/Scripts/Core`: Unity-independent gameplay code
- `Assets/Scripts/UI`: dev prototype UI scripts
- `Assets/Tests/EditMode`: core Edit Mode tests
- `Assets/Resources/Levels`: JSON level data
- `Assets/Prefabs`: includes `LetterButtonPrefab.prefab`
- `Assets/Scenes`: includes the dev scene

## Next Steps

1. Open Unity and verify the current scene bindings:
   - `DevGameController.levelText`
   - `DevGameController.requiredWordBoardView`
   - `DevGameController.letterDragPathView`
   - `DevGameController.letterButtonContainer`
   - `DevGameController.letterButtonPrefab`
   - level complete panel fields and buttons

2. Verify the drag-only loop in Play Mode:
   - Drag across letters to submit a required word.
   - Confirm required word tiles reveal correctly.
   - Confirm bonus words update in the text display.
   - Confirm the drag path appears during selection and clears after submit.
   - Confirm the level complete panel locks input and hides the letter wheel.

3. Run Edit Mode tests from the Unity Test Runner.

4. Decide whether to reintroduce the dictionary-based bonus system:
   - Add `Assets/Resources/Dictionaries/common_words.txt`.
   - Add or restore a `WordFormation` helper if desired.
   - Merge generated dictionary bonus words with JSON bonus words when loading levels.

5. Add more content:
   - Add the RATE level or other larger levels to `dev_levels.json`.
   - Expand required and bonus word lists.
   - Tune difficulty values.

6. Improve UI polish:
   - Style required word tiles and drag path colors.
   - Add selected-letter feedback beyond scale changes if needed.
   - Confirm layout works on target aspect ratios.

7. Consider UI tests or Play Mode smoke tests once the scene stabilizes.
