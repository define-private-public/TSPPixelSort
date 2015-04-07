## Download:

https://github.com/ruarai/TSPPixelSort/releases/latest

# TSPPixelSort
Pixel sorting using a genetic TSP technique to produce more accurate and interesting results.

![input](http://i.imgur.com/GXLJ1r2.png)

## Usage

Basic functions:

* Iterations: This is only for the Genetic mode. It determines how many 'evolutions' the sort will go through.
* Passes: This is a bit less useful, it just keeps running the same sort on the same image.
* Chunks: This is a lot more useful. It splits the sort into a certain number of chunks.
* Move Scale: This is also important. The sorting algorithm takes the distance from the original location into account, so a higher number will result in lower distortion. Making this 0 will mean its not taken into account at all. http://gfycat.com/FastVibrantElk shows the difference it makes.
* Mode: There's two modes: Genetic and NearestNeighbour. Genetic makes more natural looking results, with lots of small gradients, but is very slow. NearestNeighbour just tries to make one big gradient, unless a move scale is present.

The program itself has a few functions to help speed up work, with rotate and scaling options in the right click menu.


#Examples
![input](http://i.imgur.com/oUQ46SP.png)

(Source Image: http://lennsan.tumblr.com/post/112127565131/original-painting-this-is-based-on)

![result](http://i.imgur.com/EbuEG17.jpg)

(Source Image: http://www.stuckincustoms.com/2014/02/18/benjamin-von-wong-for-the-arcanum/)

