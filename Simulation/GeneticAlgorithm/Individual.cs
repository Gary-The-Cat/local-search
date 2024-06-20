using Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.GeneticAlgorithm
{
    public class Individual
    {
        // The genome of our individual.
        public List<int> Sequence { get; set; }

        // Rank of the individual in a population
        public int Rank { get; set; }

        // Crowding distance used in multi-objective optimization
        public double CrowdingDistance { get; set; }

        // Fitness values
        public float DistanceFitness { get; set; }
        public float TimeFitness { get; set; }
        public float NormalizedDistanceFitness { get; set; }
        public float NormalizedTimeFitness { get; set; }

        public Individual(List<int> sequence)
        {
            Sequence = new List<int>(sequence);
            DistanceFitness = CalculateTotalDistance();
            TimeFitness = CalculateTotalTime();
        }

        /// <summary>
        /// Calculates the total distance by decoding the individual's sequence.
        /// </summary>
        /// <returns>Total distance of the path in pixels.</returns>
        public float CalculateTotalDistance()
        {
            float totalDistance = 0.0f;

            // Loop over each of the line segments and add them up to get the total path distance.
            for (int i = 1; i < Sequence.Count; i++)
            {
                var fromTown = TownHelper.TownPositions[Sequence[i - 1]];
                var toTown = TownHelper.TownPositions[Sequence[i]];

                float deltaX = toTown.X - fromTown.X;
                float deltaY = toTown.Y - fromTown.Y;

                float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                totalDistance += distance;
            }

            return totalDistance;
        }

        /// <summary>
        /// Calculates the total time taken by decoding the individual's sequence.
        /// </summary>
        /// <returns>Total time taken to travel the path in seconds.</returns>
        public float CalculateTotalTime()
        {
            float totalTime = 0.0f;

            // Loop over each of the line segments and add them up to get the total travel time.
            for (int i = 1; i < Sequence.Count; i++)
            {
                var fromTownIndex = Sequence[i - 1];
                var toTownIndex = Sequence[i];

                var fromTown = TownHelper.TownPositions[fromTownIndex];
                var toTown = TownHelper.TownPositions[toTownIndex];

                float deltaX = toTown.X - fromTown.X;
                float deltaY = toTown.Y - fromTown.Y;

                float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                float speedLimit = TownHelper.PathSpeedLimits[(fromTownIndex, toTownIndex)];

                totalTime += distance / speedLimit;
            }

            return totalTime;
        }

        // Updates the fitness values of the individual
        public void UpdateFitness()
        {
            DistanceFitness = CalculateTotalDistance();
            TimeFitness = CalculateTotalTime();
        }

        public override bool Equals(object obj)
        {
            return obj is Individual individual && Sequence.SequenceEqual(individual.Sequence);
        }

        public static bool operator ==(Individual a, Individual b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Individual a, Individual b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Sequence);
        }
    }
}
