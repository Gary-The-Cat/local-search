using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.GeneticAlgorithm.EqualityComparers;
internal class IndividualEqualityComparer : IEqualityComparer<Individual>
{
    public bool Equals(Individual x, Individual y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.Sequence.SequenceEqual(y.Sequence);
    }

    public int GetHashCode(Individual obj)
    {
        if (obj == null || obj.Sequence == null)
        {
            return 0;
        }

        int hash = 19;
        foreach (var item in obj.Sequence)
        {
            hash = hash * 31 + item.GetHashCode();
        }
        return hash;
    }
}