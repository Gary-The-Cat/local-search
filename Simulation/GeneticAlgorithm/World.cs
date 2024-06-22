using Game.Helpers;
using Game.Helpers.LocalSearch;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game.GeneticAlgorithm
{
    public class World
    {
        private static readonly Random random = new Random();

        public List<Individual> Population { get; set; }

        // Toggle flag for visualization mode
        public bool ShowPheromonePaths { get; set; } = false;

        public List<double> FitnessOverTime { get; private set; }

        public int GenerationCount { get; private set; } = 0;

        public int NoImprovementCount { get; private set; } = 0;


        // Checks if the algorithm has converged based on generation or no improvement count
        public bool HasConverged =>
            GenerationCount > GAConfig.MaxGenerations || NoImprovementCount > GAConfig.MaxNoImprovementCount;

        public World()
        {
            Population = new List<Individual>();
            FitnessOverTime = new List<double>();
        }

        // Spawns the initial population
        public void Spawn()
        {
            Population.AddRange(WorldHelper.SpawnPopulation());
        }

        // Executes one generation of the genetic algorithm
        public void DoGeneration()
        {
            GenerationCount++;

            // Create offspring population
            var offspring = new List<Individual>();
            while (offspring.Count < GAConfig.PopulationCount)
            {
                var mother = GetParent();
                var father = GetParent();

                // Ensure mother and father are different
                while (mother == father)
                {
                    father = GetParent();
                }

                var (offspringA, offspringB) = GetOffspring(mother, father);
                (offspringA, offspringB) = Mutate(offspringA, offspringB);

                offspring.Add(offspringA);
                offspring.Add(offspringB);
            }

            // Add offspring to the population
            Population.AddRange(offspring);

            // Perform local search if enabled
            if (GenerationCount % GAConfig.GenerationsBetweenLocalSearch == 0)
            {
                if (GAConfig.IsLocalACOSearchEnabled)
                {
                    PerformLocalACOSearch();
                }
                if (GAConfig.IsLocalTwoOptSearchEnabled)
                {
                    PerformLocalTwoOptSearch();
                }
            }

            // Log fitness information
            LogFitness();

            // Update fitness and select the next generation
            MultiObjectiveHelper.UpdatePopulationFitness(Population);
            var newPopulation = SelectNextGeneration(Population);
            Population.Clear();
            Population.AddRange(newPopulation);
        }

        // Performs local ACO search on a subset of the population
        private void PerformLocalACOSearch()
        {
            int numIndividualsToImprove = (int)(Population.Count * GAConfig.LocalSearchPopulationCountFactor);
            var individualsToImprove = Population.OrderBy(x => Guid.NewGuid()).Take(numIndividualsToImprove).ToList();

            individualsToImprove.ForEach(AntColonyOptimizer.ApplyACO);
        }

        // Performs local Two-Opt search on a subset of the population
        private void PerformLocalTwoOptSearch()
        {
            int numIndividualsToImprove = (int)(Population.Count * GAConfig.LocalSearchPopulationCountFactor);
            var individualsToImprove = Population.OrderBy(x => Guid.NewGuid()).Take(numIndividualsToImprove).ToList();

            individualsToImprove.ForEach(TwoOptOptimizer.ApplyTwoOpt);
        }

        // Logs the average fitness values of the best individuals
        private void LogFitness()
        {
            var bestIndividuals = Population.Where(i => i.Rank == 0).ToList();
            var averageTime = bestIndividuals.Average(b => b.TimeFitness);
            var averageDistance = bestIndividuals.Average(b => b.DistanceFitness);
            var isLocalSearchGeneration = GenerationCount % GAConfig.GenerationsBetweenLocalSearch == 0;
            Debug.WriteLine($"{isLocalSearchGeneration}:\t Time {averageTime:F2},\t Distance {averageDistance:F2}");
        }

        // Selects the next generation of the population
        private List<Individual> SelectNextGeneration(List<Individual> currentPopulation)
        {
            return currentPopulation
                .OrderBy(i => i.Rank)
                .ThenByDescending(i => i.CrowdingDistance)
                .Distinct()
                .Take(GAConfig.PopulationCount)
                .ToList();
        }

        // Returns the best individual from the population
        public Individual GetBestIndividual()
        {
            var firstRank = Population.Where(i => i.Rank == 0).ToArray();
            return firstRank.MinBy(r => r.DistanceFitness);
        }

        // Toggles the visualization mode for showing pheromone paths
        public void ToggleVisualizationMode()
        {
            ShowPheromonePaths = !ShowPheromonePaths;
        }

        // Gets the visualizations of the paths for rendering
        public List<ConvexShape> GetPathVisualizations()
        {
            return ShowPheromonePaths && AntColonyOptimizer.PreviousPheromones != null
                ? TownHelper.GetAllPheromonePaths(AntColonyOptimizer.PreviousPheromones)
                : TownHelper.GetTownSequencePath(GetBestIndividual().Sequence);
        }

        // Mutates two individuals and returns the mutated offspring
        private (Individual, Individual) Mutate(Individual individualA, Individual individualB)
        {
            return WorldHelper.Mutate(individualA, individualB);
        }

        // Generates offspring from two parents using crossover
        private (Individual, Individual) GetOffspring(Individual mother, Individual father)
        {
            var offspringA = DoCrossover(mother, father);
            var offspringB = DoCrossover(father, mother);
            return (offspringA, offspringB);
        }

        // Performs crossover between two individuals
        private Individual DoCrossover(Individual parentA, Individual parentB)
        {
            return WorldHelper.DoCrossover(parentA, parentB);
        }

        // Selects a parent using tournament selection
        private Individual GetParent()
        {
            var (candidate1, candidate2) = WorldHelper.GetCandidateParents(Population);
            return WorldHelper.TournamentSelection(candidate1, candidate2);
        }
    }
}
