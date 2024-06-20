using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using Game.ExtensionMethods;
using System.Linq;
using System.Diagnostics;

namespace Game.Helpers
{
    public static class TownHelper
    {
        private const int Linethickness = 5;
        private const int PathOffsetFromTown = 180;
        private const int MinimumSpeedInPixels = 10;
        private const int MaximumSpeedInPixels = 100;
        private const int SpeedRangeInPixels = MaximumSpeedInPixels - MinimumSpeedInPixels;
        private static Random random = new Random();

        public static List<Vector2f> TownPositions { get; private set; }

        public static Dictionary<(int, int), float> PathSpeedLimits { get; private set; }

        /// <summary>
        /// Static constructor to initialize static properties.
        /// </summary>
        static TownHelper()
        {
            TownPositions = new List<Vector2f>();
            PathSpeedLimits = new Dictionary<(int, int), float>();
        }

        /// <summary>
        /// Initializes the TownHelper with predefined towns and speed limits.
        /// </summary>
        /// <param name="userPredefinedPositions">The list of predefined towns.</param>
        /// <param name="useRandomTowns">Flag indicating whether to use random towns.</param>
        public static void Initialize(List<Vector2f> userPredefinedPositions = null)
        {
            TownPositions.Clear();
            PathSpeedLimits.Clear();

            if (Configuration.UseRandomTowns)
            {
                PopulateRandomTowns(Configuration.RandomTownCount);
            }
            else if (userPredefinedPositions != null)
            {
                TownPositions.AddRange(userPredefinedPositions);
            }
            else
            {
                var defaultPositions = new List<Vector2f>()
                {
                    new Vector2f(3060, 1300),
                    new Vector2f(1050, 450),
                    new Vector2f(450, 750),
                    new Vector2f(690, 1890),
                    new Vector2f(1410, 1830),
                    new Vector2f(2070, 1560),
                    new Vector2f(1725, 1080),
                    new Vector2f(3360, 810),
                    new Vector2f(3450, 1770),
                    new Vector2f(2460, 240),
                };

                TownPositions.AddRange(defaultPositions);
            }

            PopulateSpeedLimits();
        }

        /// <summary>
        /// To draw the path of our current sequence, we have to create a bunch of convex shapes.
        /// SFML has a native way to draw lines, but they are 1px wide, and do not show up well
        /// in recordings, so there's a little extra work to calculate the line.
        /// </summary>
        /// <param name="townSequence">The genome of the sequence we want to display</param>
        /// <returns>The line visuals for the requested path</returns>
        public static List<ConvexShape> GetTownSequencePath(List<int> townSequence)
        {
            var paths = new List<ConvexShape>();

            for (int i = 1; i < townSequence.Count; i++)
            {
                // Get the two towns that our line will be joining
                var fromTown = TownPositions[townSequence[i - 1]];
                var toTown = TownPositions[townSequence[i]];

                // Get the normalized vector in the direction of fromTown to toTown
                var directionVector = (toTown - fromTown).Normalize();

                // Now that we have the vector pointing from fromTown to toTown, we can traverse it to give our towns
                // some space around them when we draw our line.
                var startingPoint = fromTown + (directionVector * PathOffsetFromTown);
                var endingPoint = toTown - (directionVector * PathOffsetFromTown);

                // We want to fade the lines from black - grey to show the direction of the path
                var lumination = Convert.ToByte((200.0 / TownPositions.Count) * (i - 1));

                // Convert the points we have into a 'ConvexShape' :( damn SFML.
                paths.Add(SFMLGraphicsHelper.GetLine(startingPoint, endingPoint, Linethickness, new Color(lumination, lumination, lumination)));
            }

            return paths;
        }

        public static List<ConvexShape> GetAllPheromonePaths(float[,] pheromones)
        {
            var pheromoneTolerance = 0.7;
            var paths = new List<ConvexShape>();

            var minPheromone = float.MaxValue;
            var maxPheromone = float.MinValue;

            // Find min and max pheromone levels
            for (int i = 0; i < pheromones.GetLength(0); i++)
            {
                for (int j = 0; j < pheromones.GetLength(1); j++)
                {
                    var pheromoneLevel = pheromones[i, j];
                    if (pheromoneLevel < minPheromone)
                    {
                        minPheromone = pheromoneLevel;
                    }
                    if (pheromoneLevel > maxPheromone)
                    {
                        maxPheromone = pheromoneLevel;
                    }
                }
            }

            var pheromoneRange = maxPheromone - minPheromone;

            // Normalize pheromone levels
            float[,] normalizedPheromones = new float[pheromones.GetLength(0), pheromones.GetLength(1)];
            for (int i = 0; i < pheromones.GetLength(0); i++)
            {
                for (int j = 0; j < pheromones.GetLength(1); j++)
                {
                    normalizedPheromones[i, j] = (pheromones[i, j] - minPheromone) / pheromoneRange;
                }
            }

            // Create paths using normalized pheromone levels
            for (int i = 0; i < normalizedPheromones.GetLength(0); i++)
            {
                for (int j = 0; j < normalizedPheromones.GetLength(1); j++)
                {
                    if (i == j)
                    {
                        continue; // Skip self-loops
                    }

                    var fromTown = TownPositions[i];
                    var toTown = TownPositions[j];
                    var normalizedPheromone = normalizedPheromones[i, j];

                    var directionVector = (toTown - fromTown).Normalize();

                    var startingPoint = fromTown + (directionVector * PathOffsetFromTown);
                    var endingPoint = toTown - (directionVector * PathOffsetFromTown);

                    if (normalizedPheromone > pheromoneTolerance)
                    {
                        var normalizedPheromoneVisual = (normalizedPheromone - pheromoneTolerance) / (1 - pheromoneTolerance);
                        // Use a blue-to-red gradient
                        var color = new Color(
                            (byte)(255 * normalizedPheromoneVisual), // Red
                            (byte)(0),                         // Green
                            (byte)(255 * (1 - normalizedPheromoneVisual))  // Blue
                        );

                        paths.Add(SFMLGraphicsHelper.GetLine(startingPoint, endingPoint, Linethickness, color));
                    }
                }
            }

            return paths;
        }

        private static void PopulateSpeedLimits()
        {
            var localRandom = new Random(17);

            for (int fromTown = 0; fromTown < TownPositions.Count; fromTown++)
            {
                for (int toTown = 0; toTown < TownPositions.Count; toTown++)
                {
                    // If our from town is our to town, no need to calculate a path
                    if (fromTown == toTown)
                    {
                        continue;
                    }

                    // Calculate the path distance as speed is distance dependent
                    var pathDistance = TownPositions[toTown].Distance(TownPositions[fromTown]);

                    // Add the speed for this directional path
                    PathSpeedLimits.Add(
                        (fromTown, toTown),
                        (float)(MinimumSpeedInPixels + SpeedRangeInPixels * localRandom.NextDouble() * pathDistance / 1000));
                }
            }
        }

        private static void PopulateRandomTowns(int randomTownCount)
        {
            for (int i = 0; i < randomTownCount; i++)
            {
                // Note that random town placements can overlap
                TownPositions.Add(GenerateRandomTownPosition());
            }
        }

        private static Vector2f GenerateRandomTownPosition()
        {
            return new Vector2f
            {
                X = 100 + ((float)random.NextDouble() * (Configuration.Width - 100)),
                Y = 100 + ((float)random.NextDouble() * (Configuration.Height - 100))
            };
        }
    }
}