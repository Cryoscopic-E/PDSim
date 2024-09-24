# PDSim: Planning Domain Simulation
Visualization and animation of PDDL plans.

Use PDDL domain and problem files to setup a simulation.

# Installation: <NEW> Unity Package (finally)
You can now install PDSim as Unity package and download the samples from the Unity Package manager.

- Open the Package Manager from the Unity Editor:

From the top bar
`Window > Package Manager`

- Click on the plus button on the Top Left and selct

`Add Package from GIT url`

- Paste the following url: 

https://github.com/Cryoscopic-E/PDSim.git?path=/Packages/com.cryoscopic-e.pdsim

## Using the samples

Once installed you can check and import the sample visualisation scenes that comes with PDSim.

Available (for now):
- blocks world
- sokoban 2D

More to come later!

# Installation: Back-End
PDSim use a back-end server to generate the Unity's components used for the simulation, install it from [here](https://github.com/Cryoscopic-E/PDSim-Backend)

# Assets for your Visualisation
PDSim actively support the amazing work of [Kenney] (https://twitter.com/KenneyNL)

You can find tons of CC0 2D and 3D assets for you visualisations here: https://kenney.nl/assets



Additional resources:

- https://itch.io/game-assets
- https://polyhaven.com/models
- https://www.blenderkit.com/


# Project

- PDDL `predicates` are used to define animation in Unity using the built-in visual scripting language.
![Animation Example](./readme_img/animation.png)
- PDDL `types` is defined are use to create objects prefab of a particular category. Import all the models you want to repesent PDDL types.
![Type Customisation](./readme_img/type.png)
- PDDL `actions' effects and init` are the main animated component. Interact with the UI to get info about the state of the environment and each component with the built-in UI

![PDSim UI](./readme_img/ui.png)

Example with Blocks World:

![BLOCKS WORLD ANIMATION](./readme_img/pdsim.gif)


## Documentation
Check the wiki [here](https://github.com/Cryoscopic-E/PDSim/wiki)

## Acknowledgement
PDSim is being developed for the AIPlan4EU H2020 project (https://aiplan4eu-project.eu)

[@aiplan4eu](https://github.com/aiplan4eu) for the [Unified Planning Library](https://github.com/aiplan4eu/unified-planning)

## Publication
If you are using PDSim in your work please cite it:
```
@inproceedings{de2024planning,
  title={Planning Domain Simulation: An Interactive System for Plan Visualisation},
  author={De Pellegrin, Emanuele and Petrick, Ronald PA},
  booktitle={Proceedings of the International Conference on Automated Planning and Scheduling},
  volume={34},
  pages={133--141},
  year={2024}
}
```
