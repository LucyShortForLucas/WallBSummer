# Intro 
This document will cover everything related to sound & and it's design in our game. 
The game's genres are open world, city builder and tower defense. The goal of the player is to revitalize the world to bring back all the greenery and plants.

# Sound Tech Stack
For the sound specific tech stack we'll be using REAPER as our Digital Audio Workstation to make our sounds in and we'll be using Audiokinetic's Wwise to author our events and 
create soundbanks to put in the game. 

Since each sound designer has a different way of approaching their design, every designer is allowed to use their own plugins. However we want to emphasize that we want to work
in a sparing way with our effects that will be applied in real-time. Effects that are static and won't be applied are expected to either be rendered in REAPER, or to be rendered
in Wwise. If possible avoid applying effects in Wwise to keep the UI clean and not fill it up with effects.

# Sound Direction
For the direction we want to go with the sound in our game it's imperative to understand the storytelling we want to achieve. We want the sound to be dynamic in relation with how
revitalized the world is. For example the ambience will start out as something more akin to what a dessert would feel like. Once the player starts spreading fertile land 
and the overal fertility goes up, the ambience will very slowly (almost unnoticably) start to shift to what a grassland would feel like. The heavy dessert air slowly fading out
while a cooler breeze starts to appear, more and different animals will become audible.

Knowing that it becomes clear that we'll have 2 completely different moods in our game, starting out pretty heavy and melodramatic while slowly shifting towards a happier and lighter 
vibe. This means that for the base sound design we want the majority of the sounds to have 2 different variants. A bit of a darker, sadder variant as well as a lighter and happier
variant. 

The sound design itself should be realistic with stylized and cartoonish layers. This creates a semi realistic feel to the sounds without them sounding unrealistic, while keeping
them close to the visual style.

# Sound Design layers
Every sound, except ambience and music, will consist of 2 main layers. A base layer and a style layer. 

#- Base layer
The base layer is defined as the main identity of the sound. For example when making a gunshot the base layer would contain the tail, the shot and the impact of the weapon.
Playing this alone has to sound good on it's own and will convey what is meant to be used for. 
#- Style layer
The style layer is defined as the sounds that make the sound fit in the game's context. When making a gunshot this would be the sci fi effects to make the base layer not just sound 
like a regular weapon, but make it sound like the weapon on screen. 
It's a layer to sweeten the sound and make it feel more like it fits in the game world.

The reason for this is because we want the base layer to always persist, while the style layer slowly changes to something different throughout the progression of the game.

# Sound Design good pratices
Working with a lot of effects on our sounds can make them sound off, that's why we prefer to work with layering and then fine tune with effects.
Sounds that have a lot of low-ends have to be avoided to play together. This is because working with a lot of low ends tends to cause a lot of phasing issues.
When Designing sounds, test them out with an example of the ambience.

# Sound list
Can be found as an attached excel sheet.

# Outro	
For any questions related to this document or if any revisions have to be made, please contact the sound lead.