# [PROJECT NAME]_grid

This static library is responsible for representing and managing grid-based data.

It is written and maintained by Lucille *(birth-name Lucas)* Schonkeren, aka Lucy, for the purpose of the Unity game project Wall-B (working title).

## Goal

The purpose of this library is to provide all of the spatial multi-dimensional data handling the project requires. The two chief goals of this C++ library are **performance** and **extensibility**. Both of these goals are achieved through a mix of performant C++ code, native multi-threading, and compile-time generics. 

## Core concepts

All of the logic within this library is based on the idea that the **world** consists of numerous **grids** that span, theoretically, infinitely far in any spatial direction, made up of **tiles** that carry data and care about their position within the grid and the world.

mathematically, grids are **bidirectional graphs** where each node always has exactly ``3*N-1`` neighbours, where ``N`` is the dimensionality of the grid.

The **world** consists of an arbitrary number of independent **grids**, which are themselves made up of static **chunks** of data. Each such chunk is a tightly packed packet of spatially indexed tile data, and each chunk also has an immutable 1-tile **halo** that mirrors the **edge tiles** of all of its neighbouring chunks. This halo is an independent strip of data that gives spatial information to the chunk's edge tiles without having to look it up into neighbouring chunks.

All chunks are double-buffered and carry their buffers in contiguous memory within themselves. Thus, the total byte count of a chunk's tile data buffer for a chunk spanning ``W`` tiles in ``N`` dimensions is: ``sizeof(tiledata) * (W+2)^N * 2``

The whole idea behind a **grid** is that its data lives somewhere in **space**. All grids in a **world** therefore abstractly occupy this same space, and are always considered to share an origin (one grid in a world can never be offset from another). In this way, tiles of different grids with the same coordinate are said to exist at the same point in space.

## Terminology

The following terminology is used throughout this library with the following definitions in mind:

### Data

- **Grid**: a dynamic, multi-dimensional data structure stretching 'infinitely' in both directions of each dimension, representing arbitrary, spatially-indexed data.
- **Tile** *(data)*: a single point of data on a grid, occupying a single point on the grid.
- **Chunk**: A static, multi-dimensional data structure of compile-time defined size, representing a fragment of a grid.
- **World**: The collection of all grids and their data that exist together and whose data is relevant to the current scene.

### Spatial / numeric

- **Coord**: a multi-dimensional integer index, i.e. coordinate. Represents a tile's position in a chunk or grid, a chunk's position in its grid, etc.
- **Tile(s)** *(length)*: a length as represented by a number of tiles.
- **Chunk Width**: the amount of tiles a chunk represents in **each** dimension. The term 'width' here is used as shorthand, but all dimensions of a chunk are equal in length.
- **Origin**:
	- **Grid origin**: the origin of the whole grid, i.e. the tile with grid-coord 0 along all dimensions.
	- **Chunk origin**: the origin of a chunk, i.e. the  tile with chunk-coord 0 along all dimensions. In a 2d grid this is the topleft most tile in a grid. Usually expressed in terms of grid-space. Note that this is **not** the tile with index 0 in its data buffer, as the chunk data buffer includes a halo. Its actual index is ``CHUNK_WIDTH+3``

### Directions on a grid

- **North**: in the positive direction of the y axis
- **East**: in the positive direction of the x axis
- **South**: in the negative direction of the y axis
- **West**: in the negative direction of the x axis