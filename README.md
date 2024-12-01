# Code Samples
- There are only my scripts without a Unity Project.
- These scripts will not run on their own as they have dependencies on the Unity Engine and other systems in place for the project.
- These scripts are meant to show some of my programming practices and self-made systems. 
## Project: Deadly Haunted Livestream
- These scripts show how an AI agent (Guard) behaves using separate Task scripts and outside detection systems to update it's FSM. The Guard will play some animations, walk to pre-determined points in the game, and chase/search for the player in case it's detected, in case of losing sight of the player the Guard will resume it's sequenced path, then desapear once the sequence it's finished.
- This is an on-going project (Horror game) in Unity using C# and github. I mainly contribute as the AI programmmer but also assit on gameplay programming.
- Assembled a costumizable AI system that uses individual scripts for behaviors to easily switch and combine behaviors by adding those scripts to one or multiple AI agents. This system is meant to allow for quick developement of AI agents that behave differently but share commonalities in how their behaviors work. 
- Created a sight detection system by filtering with layermasks and detecting colliders with raycasts. I also included debugging visuals to aid developers find issues within their FSMs.
- Developed a sound reporting system that is yet to be implemented in the game, but the logic has been constructed.
