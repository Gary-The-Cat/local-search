using Game.Helpers;

namespace Game.GeneticAlgorithm
{
    public static class GAConfig
    {
        public static int MaxGenerations => 400;

        public static double MutationChance => 0.05;

        public static int PopulationCount => 100;

        public static int MaxNoImprovementCount => 20;

        public static bool UseBiasRandomSpawning => true;

        public static int GenerationsBetweenLocalSearch => 5;

        // 5% of the population are used in local search.
        public static double LocalSearchPopulationCountFactor => 0.05;
        
        public static bool IsLocalACOSearchEnabled => false;
        
        public static bool IsLocalTwoOptSearchEnabled => false;

        // WARNING: Towns may overlap as there is no logic for their placement.
        public static bool UseRandomTowns { get; set; } = true;

        // NOTE: TownCount only applies when using random towns.
        public static int RandomTownCount => 100;

        public static int TownCount => UseRandomTowns ? RandomTownCount : TownHelper.TownPositions.Count;
    }
}
