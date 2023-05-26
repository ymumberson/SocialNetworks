# SocialNetworks
4th Year Project. Growing realistic social networks from the ground up in an agent-based city landscape.

The city landscape is a simple 2D tile that reads in an image and parses it by pixel colours to instantiate the landscape. Agents have simple lifecycles, where kids go to school and adults go to work. After work/school, agents can organise social meetups with their friends.

Agents store a list of their friends, and make friends with other agents at work/school and social events. The social networks are then rendered in a seperate view.

Rendering the network uses Force-Directed drawing, and is accelerated using compute shaders.

## Gifs
### Growing Social Networks
![Growing Social Networks](https://github.com/ymumberson/SocialNetworks/blob/ac829f91b561e98c887761144e91d8da0c269c11/Recordings/Growing%20Social%20Networks.gif)

### City Simulation
![City Simulation](https://github.com/ymumberson/SocialNetworks/blob/ac829f91b561e98c887761144e91d8da0c269c11/Recordings/map1.gif)
