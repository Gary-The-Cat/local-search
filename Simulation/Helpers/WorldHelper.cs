using Game.ExtensionMethods;
using Game.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Helpers
{
    /// <summary>
    /// A set of static helper methods to interact with a sequence based genetic algorithm.
    /// </summary>
    public static class WorldHelper
    {
        public static Random Random = new Random(42);

        public static List<Individual> SpawnPopulation()
        {
            var population = new List<Individual>();
            TownHelper.InitializeDistanceLookup(GAConfig.TownCount);

            // Generate {PopulationCount} individuals
            while (population.Count < GAConfig.PopulationCount)
            {
                var isGeneratingBiasedRandomIndividual = GAConfig.UseBiasRandomSpawning && population.Count % 2 == 0;

                var individual = isGeneratingBiasedRandomIndividual
                    ? GenerateBiasedIndividual(GAConfig.TownCount)
                    : GenerateIndividual(GAConfig.TownCount);

                if (!population.Contains(individual))
                {
                    population.Add(individual);
                }
            }

            return population;
        }

        public static Individual GenerateIndividual(int sequenceLength)
        {
            // Generate a list of numbers [0, 1, 2, 3... 9]
            var sequence = Enumerable.Range(0, sequenceLength).ToList();

            // Randomly shuffle the list [3, 1, 5, 9... 4]
            sequence.Shuffle();

            // Create a new individual with our random sequence
            return new Individual(sequence);
        }

        public static Individual GenerateBiasedIndividual(int sequenceLength)
        {
            var sequence = new List<int>();
            var availableTowns = Enumerable.Range(0, sequenceLength).ToList();

            // Select the first town randomly
            int currentTown = availableTowns[Random.Next(availableTowns.Count)];
            sequence.Add(currentTown);
            availableTowns.Remove(currentTown);

            while (availableTowns.Count > 0)
            {
                // Calculate distances and weights
                var weightedTowns = availableTowns
                    .Select(town => new
                    {
                        Town = town,
                        Weight = Math.Pow(TownHelper.TownDistanceMap[currentTown, town], -GAConfig.BiasSpawningFactor)
                    })
                    .ToList();

                // Select the next town based on weighted random choice
                var totalWeight = weightedTowns.Sum(w => w.Weight);
                var randomWeight = Random.NextDouble() * totalWeight;
                var cumulativeWeight = 0.0;
                int selectedTown = weightedTowns.First().Town;

                foreach (var weightedTown in weightedTowns)
                {
                    cumulativeWeight += weightedTown.Weight;
                    if (cumulativeWeight >= randomWeight)
                    {
                        selectedTown = weightedTown.Town;
                        break;
                    }
                }

                // Update sequence and available towns
                sequence.Add(selectedTown);
                availableTowns.Remove(selectedTown);
                currentTown = selectedTown;
            }

            // Return the new individual with the generated sequence
            return new Individual(sequence);
        }


        public static (Individual, Individual) GetCandidateParents(List<Individual> population)
        {
            // Grab two random individuals from the population
            var candidateA = population[Random.Next(population.Count())];
            var candidateB = population[Random.Next(population.Count())];

            // Ensure that the two individuals are unique
            while (candidateA == candidateB)
            {
                candidateB = population[Random.Next(population.Count())];
            }

            return (candidateA, candidateB);
        }

        public static Individual TournamentSelection(Individual candidateA, Individual candidateB)
        {
            // Return the individual that has the higher fitness value
            if (candidateA.Rank < candidateB.Rank)
            {
                return candidateA;
            }
            else if (candidateA.Rank == candidateB.Rank)
            {
                return candidateA.CrowdingDistance > candidateB.CrowdingDistance
                    ? candidateA
                    : candidateB;
            }
            else
            {
                return candidateB;
            }
        }

        public static Individual DoCrossover(Individual individualA, Individual individualB, int crossoverPosition = -1)
        {
            // Generate a number between 1 and sequence length - 1 to be our crossover position
            crossoverPosition = crossoverPosition == -1 
                ? Random.Next(1, individualA.Sequence.Count - 1)
                : crossoverPosition;

            // Grab the head from the first individual
            var offspringSequence = individualA.Sequence.Take(crossoverPosition).ToList();

            // Create a hash for quicker 'exists in head' checks
            var appeared = offspringSequence.ToHashSet();

            // Append individualB to the head, skipping any values that have already shown up in the head
            foreach (var town in individualB.Sequence)
            {
                if (appeared.Contains(town))
                {
                    continue;
                }

                offspringSequence.Add(town);
            }

            // Return our new offspring!

            return new Individual(offspringSequence);
        }

        public static (int, int) GetUniqueTowns(List<int> sequence)
        {
            // Randomly select two towns
            var townA = Random.Next(sequence.Count());
            var townB = Random.Next(sequence.Count());

            // Ensure that the two towns are not the same
            while (townB == townA)
            {
                townB = Random.Next(sequence.Count());
            }

            return (townA, townB);
        }

        public static Individual DoRotateMutate(Individual individual)
        {
            // Grab two unique towns
            var (townA, townB) = GetUniqueTowns(individual.Sequence);

            // Grab a reference to the sequence - just to make code below tidier
            var sequence = individual.Sequence;

            // Determine which of the indices chosen comes before the other
            int firstIndex = townA < townB ? townA : townB;
            int secondIndex = townA > townB ? townA : townB;

            // Grab the head of the sequence
            var newSequence = sequence.Take(firstIndex).ToList();

            // Grab the centre and rotate it
            var middle = sequence.Skip(firstIndex).Take(secondIndex - firstIndex).Reverse();

            // Grab the end of the sequence
            var end = sequence.Skip(secondIndex).ToList();

            // Add all components of the new sequence together
            newSequence.AddRange(middle);
            newSequence.AddRange(end);

            // Return a new individual with our new sequence
            return new Individual(newSequence);
        }

        public static Individual DoSwapMutate(Individual individual)
        {
            // Grab a copy of our current sequence
            var sequence = individual.Sequence.ToList();

            // Get the indices of the towns we want to swap
            var (townA, townB) = GetUniqueTowns(individual.Sequence);

            sequence.SwapInPlace(townA, townB);

            return new Individual(sequence);
        }

        public static (Individual, Individual) Mutate(Individual individualA, Individual individualB)
        {
            // Grab a copy of our individual in its current state, not the most efficient way
            // but certainly a very testable way.
            var newIndividualA = new Individual(individualA.Sequence);
            var newindividualB = new Individual(individualB.Sequence);

            // Generate a number between 0-1, if it is lower than our mutation chance (0.05 - 5%), mutate!
            if (Random.NextDouble() < GAConfig.MutationChance)
            {
                newIndividualA = DoMutate(individualA);
            }

            // Generate a number between 0-1, if it is lower than our mutation chance (0.05 - 5%), mutate!
            if (Random.NextDouble() < GAConfig.MutationChance)
            {
                newindividualB = DoMutate(individualB);
            }

            return (newIndividualA, newindividualB);
        }

        private static Individual DoMutate(Individual individual)
        {
            // Half the time, use one mutation method, and other half use the other.
            if (Random.NextDouble() > 0.5)
            {
                return DoSwapMutate(individual);
            }
            else
            {
                return DoRotateMutate(individual);
            }
        }
    }
}
