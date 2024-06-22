using Game.GeneticAlgorithm;
using Game.Helpers;
using Game.Helpers.LocalSearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Game.Tests.Tests.LocalSearch;

[TestClass]
public class TwoOptOptimizerTests
{
    [TestMethod]
    public void ApplyTwoOpt_ImprovesIndividualSequence()
    {
        // Arrange
        GAConfig.UseRandomTowns = false;
        TownHelper.Initialize([new(50, 50), new(0, 0), new(100, 100)]);
        var initialSequence = new List<int> { 0, 2, 1 };
        var individual = new Individual(initialSequence);
        var initialTimeFitness = individual.TimeFitness;
        var initialDistanceFitness = individual.DistanceFitness;

        // Act
        TwoOptOptimizer.ApplyTwoOpt(individual);
        individual.UpdateFitness();

        // Assert
        // We expect that the individual's fitness should improve after applying 2-opt
        Assert.IsTrue(individual.DistanceFitness < initialDistanceFitness, "Distance fitness did not improve.");
        Assert.IsTrue(individual.TimeFitness < initialTimeFitness, "Time fitness did not improve.");

        // Additional check to ensure the sequence has been modified
        CollectionAssert.AreNotEqual(initialSequence, individual.Sequence, "The sequence was not modified.");
    }
}
