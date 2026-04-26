ROAE BARISTA WELCOME SCAFFOLDING

What this bootstrap created:
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Core
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Config
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/AI
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Runtime
- Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Configs/BaristaWelcomeConfig_Default.asset

How to use the vertical slice:
1. Create an empty GameObject in BarInside scene called BaristaWelcomeSystem.
2. Add BaristaWelcomeBrain.
3. Add BaristaWelcomeController.
4. Assign BaristaWelcomeConfig_Default to the brain.
5. Optional: add BaristaWelcomeDebugMenu for right-click context debugging.
6. Use controller methods from buttons / ActionLists / events:
   - ResolveOpeningTone
   - ApplyNaiveResponse
   - ApplyGuardedResponse
   - GiveAcceptedDrinkIfPossible
   - TryOrderCola
   - TryOrderPhotosyntheticSap
   - TryDrinkHeldDrink
7. Completion condition:
   - barista_intro_done == true
   - drank_photosynthetic_drink == true

Recommended next manual cleanup:
- keep DialogueTrigger
- keep CreativeCore
- keep CreativeHUD
- stop growing DialogueTrigger2
- stop using DialogueFlags for new content

Planning highlight:
- plannerMode = ValueIteration uses dynamic programming for best opening tone
- plannerMode = PolicyIteration uses policy evaluation + policy improvement
