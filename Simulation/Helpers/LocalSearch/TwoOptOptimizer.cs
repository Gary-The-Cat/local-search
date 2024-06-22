using Game.GeneticAlgorithm;
using System;
using System.Collections.Generic;

namespace Game.Helpers.LocalSearch;

public static class TwoOptOptimizer
{
    // Maximum iterations for 2-opt
    public static int MaxIterations { get; set; } = 20;

    /// <summary>
    /// Applies the 2-opt algorithm to improve the given individual's sequence by reversing segments of the tour.
    /// The algorithm continues to iterate until no further improvements can be found or the maximum number of iterations is reached.
    /// </summary>
    /// <param name="individual">The individual whose sequence is to be optimized.</param>
    public static void ApplyTwoOpt(Individual individual)
    {
        int size = individual.Sequence.Count;
        var random = new Random();
        int iterations = 0;

        // Continue while the iteration limit has not been reached
        while (iterations < MaxIterations)
        {
            iterations++;

            // Randomly select two indices to perform the 2-opt swap
            int i = random.Next(size - 1);
            int k = random.Next(i + 1, size);

            // Store the current fitness value
            double distanceFitness = individual.DistanceFitness;

            // Perform 2-opt swap: reverse the segment between indices i and k
            ReverseSegmentInPlace(individual.Sequence, i, k);

            // Update the fitness value after reversing the segment
            individual.UpdateFitness();

            // Check if the new fitness value is an improvement
            if (individual.DistanceFitness < distanceFitness)
            {
                // If an improvement is found, keep the change
                iterations = 0; // Reset the iteration counter if improvement is found
            }
            else
            {
                // Revert the change if no improvement
                ReverseSegmentInPlace(individual.Sequence, i, k);
                individual.UpdateFitness();
            }
        }
    }

    /// <summary>
    /// Reverses the segment of the list between the given indices in place.
    /// </summary>
    /// <param name="sequence">The list of integers to modify.</param>
    /// <param name="start">The starting index of the segment to reverse.</param>
    /// <param name="end">The ending index of the segment to reverse.</param>
    private static void ReverseSegmentInPlace(List<int> sequence, int start, int end)
    {
        for (int i = start, j = end; i < j; i++, j--)
        {
            var temp = sequence[i];
            sequence[i] = sequence[j];
            sequence[j] = temp;
        }
    }
}
