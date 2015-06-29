using AForge.Genetic;

namespace PixelSortApp
{
    public class SortOptions
    {
        public int Iterations;
        public int ChunkSize;
        public SortMode Mode;
        public double MoveScale;
        public bool BiDirectional;
        public ISelectionMethod GeneticMode;
        public int Passes;
        public int PassesRemaining;
    }
}