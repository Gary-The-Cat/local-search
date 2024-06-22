using Game.Factories;
using Game.GeneticAlgorithm;
using Game.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tests.Tests.LocalSearch;

[TestClass]
public class AntColonyOptimizerTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TestACOImprovesSolution()
    {
        Configuration.UseRandomTowns = false;
        TownHelper.Initialize(new List<Vector2f> { new(0, 50), new(0, 0), new(0, 100), new(0, 30) });

        var initialSequence = new List<int> { 0, 1, 2, 3 };
        var individual = new Individual(initialSequence);
        individual.UpdateFitness();

        double initialFitness = individual.DistanceFitness + individual.TimeFitness;
        AntColonyOptimizer.ApplyACO(individual);
        double finalFitness = individual.DistanceFitness + individual.TimeFitness;

        Assert.IsTrue(finalFitness < initialFitness, "ACO did not improve the solution.");
    }
}