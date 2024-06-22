using Game.DataStructures;
using Game.GeneticAlgorithm;
using Game.Helpers;
using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace Game.Factories
{
    public static class TownFactory
    {
        // Generates a list of towns with textures based on configuration
        public static List<Town> GetTowns()
        {
            var towns = new List<Town>();

            // Load all towns based on their positions in the TownHelper
            for (int index = 0; index < TownHelper.TownPositions.Count; index++)
            {
                // Get the hardcoded position of the town
                var townPosition = TownHelper.TownPositions[index];

                // Create a new town with a position and texture
                towns.Add(new Town(townPosition, new Texture(GetTownTexture(index))));
            }

            return towns;
        }

        // Determines the texture path for a town based on the configuration
        private static string GetTownTexture(int index)
        {
            return GAConfig.UseRandomTowns
                ? $"Resources/Town_{WorldHelper.Random.Next(1, 10)}.png"
                : $"Resources/Town_{index + 1}.png";
        }
    }
}