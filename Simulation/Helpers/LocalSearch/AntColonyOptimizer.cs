using Game.GeneticAlgorithm;
using Game.ExtensionMethods;
using SFML.System;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using Game.Helpers;

public static class AntColonyOptimizer
{
    private static int numAnts = 10;
    private static int numIterations = 10;
    private static float alpha = 1.0f; // Importance of pheromone
    private static float beta = 5.0f;  // Importance of heuristic (distance)
    private static float evaporationRate = 0.5f;
    private static float pheromoneDeposit = 5.0f;
    private static float initialPheromone = 0.1f;
    public static float[,] PreviousPheromones;

    public static void ApplyACO(Individual individual)
    {
        List<Vector2f> cities = TownHelper.TownPositions;
        int numCities = cities.Count;
        float[,] distances = new float[numCities, numCities];
        float[,] pheromones = new float[numCities, numCities];

        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                distances[i, j] = cities[i].Magnitude(cities[j]);
                pheromones[i, j] = initialPheromone;
            }
        }

        // Slightly bias the current individuals path in the initial pheremone deposit.
        for (int i = 1; i < individual.Sequence.Count; i++)
        {
            pheromones[i - 1, i] += 0.1f;
        }

        List<int> bestPath = null;
        float bestLength = float.MaxValue;

        for (int iter = 0; iter < numIterations; iter++)
        {
            List<List<int>> paths = new List<List<int>>();
            List<float> lengths = new List<float>();

            for (int ant = 0; ant < numAnts; ant++)
            {
                List<int> path = new List<int> { individual.Sequence.First() };
                while (path.Count < numCities)
                {
                    int currentTown = path.Last();
                    float[] probabilities = CalculateProbabilities(currentTown, path, pheromones, distances);
                    int nextTown = SelectNextTown(probabilities, path);
                    path.Add(nextTown);
                }
                paths.Add(path);
                lengths.Add(CalculatePathLength(path, distances));

            }

            UpdatePheromones(paths, lengths, pheromones);

            float minLength = lengths.Min();
            if (minLength < bestLength)
            {
                bestLength = minLength;
                bestPath = paths[lengths.IndexOf(minLength)];
            }
        }

        PreviousPheromones = pheromones;
        individual.Sequence = bestPath;
        individual.UpdateFitness();
    }

    private static float[] CalculateProbabilities(int currentTown, List<int> visited, float[,] pheromones, float[,] distances)
    {
        int numCities = pheromones.GetLength(0);
        float[] probabilities = new float[numCities];
        float sum = 0.0f;

        for (int i = 0; i < numCities; i++)
        {
            if (visited.Contains(i))
            {
                continue;
            }

            float pheromone = pheromones[currentTown, i];
            float heuristic = 1.0f / distances[currentTown, i];
            probabilities[i] = (float)Math.Pow(pheromone, alpha) * (float)Math.Pow(heuristic, beta);
            sum += probabilities[i];
        }

        for (int i = 0; i < numCities; i++)
        {
            probabilities[i] /= sum;
        }

        return probabilities;
    }

    private static int SelectNextTown(float[] probabilities, List<int> path)
    {
        float cumulative = 0.0f;
        float r = (float)WorldHelper.Random.NextDouble();
        List<int> remainingTowns = new List<int>();

        for (int i = 0; i < probabilities.Length; i++)
        {
            if (!path.Contains(i))
            {
                remainingTowns.Add(i);
            }
        }

        // If no remaining towns, return an invalid index
        if (remainingTowns.Count == 0)
        {
            throw new InvalidOperationException("No remaining towns to visit.");
        }

        // Normalize probabilities for remaining towns
        float totalProbability = remainingTowns.Sum(town => probabilities[town]);

        if (totalProbability == 0)
        {
            // If all remaining probabilities are 0, select a town randomly
            return remainingTowns[WorldHelper.Random.Next(remainingTowns.Count)];
        }

        for (int i = 0; i < remainingTowns.Count; i++)
        {
            int town = remainingTowns[i];
            cumulative += probabilities[town] / totalProbability;
            if (r <= cumulative)
            {
                return town;
            }
        }

        return remainingTowns.Last();
    }

    private static float CalculatePathLength(List<int> path, float[,] distances)
    {
        float length = 0.0f;
        for (int i = 1; i < path.Count; i++)
        {
            length += distances[path[i - 1], path[i]];
        }
        return length;
    }

    private static void UpdatePheromones(List<List<int>> paths, List<float> lengths, float[,] pheromones)
    {
        int numCities = pheromones.GetLength(0);

        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                pheromones[i, j] *= (1 - evaporationRate);
            }
        }

        for (int k = 0; k < paths.Count; k++)
        {
            List<int> path = paths[k];
            float length = lengths[k];

            for (int i = 1; i < path.Count; i++)
            {
                int from = path[i - 1];
                int to = path[i];
                pheromones[from, to] += pheromoneDeposit / length;
                pheromones[to, from] += pheromoneDeposit / length;
            }
        }
    }

    private static void PrintPheromoneLevels(float[,] pheromones)
    {
        int numCities = pheromones.GetLength(0);
        Console.WriteLine("\nPheromone levels:");
        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                Debug.WriteLine($"Pheromone level between town {i} and town {j}: {pheromones[i, j]:F4}");
            }
        }
    }
}