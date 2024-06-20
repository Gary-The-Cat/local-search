using Game.GeneticAlgorithm;
using System;
using System.Collections.Generic;

namespace Game.Helpers.LocalSearch;

public static class TwoOptOptimizer
{
    // Maximum iterations for 2-opt
    public static int MaxIterations { get; set; } = 100;

    /// <summary>
    /// Applies the 2-opt algorithm to improve the given individual's sequence by reversing segments of the tour.
    /// The algorithm continues to iterate until no further improvements can be found or the maximum number of iterations is reached.
    /// </summary>
    /// <param name="individual">The individual whose sequence is to be optimized.</param>
    public static void ApplyTwoOpt(Individual individual)
    {
        int size = individual.Sequence.Count;
        bool improvementFound = true;
        int iterations = 0;

        // Continue while improvements are found and the iteration limit has not been reached
        while (improvementFound && iterations < MaxIterations)
        {
            improvementFound = false;
            iterations++;

            // Outer loop to select the first edge
            for (int i = 0; i < size - 1; i++)
            {
                // Inner loop to select the second edge, ensuring it starts after the first edge
                for (int k = i + 1; k < size; k++)
                {
                    // Store the current fitness values
                    double distanceFitness = individual.DistanceFitness;
                    double timeFitness = individual.TimeFitness;

                    // Perform 2-opt swap: reverse the segment between indices i and k
                    ReverseSegmentInPlace(individual.Sequence, i, k);
                    // Update the fitness values after reversing the segment
                    individual.UpdateFitness();

                    // Check if the new fitness values are an improvement
                    if (IsImprovement(individual.DistanceFitness, individual.TimeFitness, distanceFitness, timeFitness))
                    {
                        // If an improvement is found, set the flag to true
                        improvementFound = true;
                    }
                    else
                    {
                        // Revert the change if no improvement
                        ReverseSegmentInPlace(individual.Sequence, i, k);
                        individual.UpdateFitness();
                    }
                }
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

    /// <summary>
    /// Determines if the new fitness values represent an improvement over the current fitness values.
    /// </summary>
    /// <param name="newDistanceFitness">The new distance fitness value.</param>
    /// <param name="newTimeFitness">The new time fitness value.</param>
    /// <param name="currentDistanceFitness">The current distance fitness value.</param>
    /// <param name="currentTimeFitness">The current time fitness value.</param>
    /// <returns>True if the new fitness values are an improvement, otherwise false.</returns>
    private static bool IsImprovement(double newDistanceFitness, double newTimeFitness, double currentDistanceFitness, double currentTimeFitness)
    {
        return (newDistanceFitness <= currentDistanceFitness && newTimeFitness < currentTimeFitness) ||
               (newDistanceFitness < currentDistanceFitness && newTimeFitness <= currentTimeFitness);
    }
}
