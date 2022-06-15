using NUnit.Framework;
using Unity.Collections;
using Imagibee.Parallel;
using UnityEngine;
using System.Collections.Generic;

public class Functional
{
    [Test]
    public void ParallelPcc()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJob();
        pccJob.Allocate(x.Length, y.Length / x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i = 0; i < y.Length / x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.AreEqual(results[0], 1f);
        Assert.AreEqual(results[1], -1f);
    }

    [Test]
    public void SerialPcc()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[2][];
        y[0] = new float[] { 1, 2, 3, 4, 5 };
        y[1] = new float[] { -1, -2, -3, -4, -5 };
        var results = Functions.Pcc(x, y);
        Assert.AreEqual(results[0], 1f);
        Assert.AreEqual(results[1], -1f);
    }

    [Test]
    public void ParallelPccDivideByZero()
    {
        var x = new float[] { 1, 1, 1, 1, 1 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJob();
        pccJob.Allocate(x.Length, y.Length / x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i = 0; i < y.Length / x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.IsNaN(results[0]);
        Assert.IsNaN(results[1]);
    }

    [Test]
    public void ParallelPccRandom()
    {
        var random = new System.Random();
        const int LENGTH = 1000;
        var x = new float[LENGTH];
        var y = new float[LENGTH];
        for (var i=0; i<LENGTH; ++i) {
            x[i] = (float)random.NextDouble();
            y[i] = (float)random.NextDouble();
        }
        var pccJob = new PccJob();
        pccJob.Allocate(x.Length, y.Length / x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        var result = pccJob.R[0];
        Debug.Log($"correlation is {result}");
        pccJob.Dispose();
        Assert.LessOrEqual(result, 1f);
        Assert.GreaterOrEqual(result, -1f);
    }

    [Test]
    public void ParallelPccUsage()
    {
        // Here x represents a frequently changing value that is
        // correlated against a set of infrequently changing y.
        var x = new List<float[]>
        {
            new float[] { 1, 2, 3, 4, 5 },
            new float[] { 2, 4, 6, 8, 10 }
        };
        // There can be 1 or more y, concatenated together, the
        // length of each y must match the length of x.  Here we
        // have two y, each of length 5, concatenated together.
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };

        // length is the length of each x and y
        var length = x[0].Length;

        // ycount is the number of arrays contained in y
        var ycount = y.Length / length;

        // Create a correlation job
        var pccJob = new PccJob();

        // Allocate the native containers for the job
        pccJob.Allocate(length, ycount, Allocator.Persistent);

        // Copy the infrequently changing values of y to native storage
        pccJob.Y.CopyFrom(y);

        // Compute the correlation of two values of x using the same set of y
        for (var i = 0; i < x.Count; ++i) {
            // Copy the current value of x to native storage
            pccJob.X.CopyFrom(x[i]);

            // Schedule and wait for completion of two corelation values
            // in pccJob.R
            // 1) the correlation of x[i] with the first 5 elements of y
            // 2) the correlation of x[i] with the last 5 elements of y
            pccJob.Schedule().Complete();

            // Do something with results
            for (var j = 0; j < ycount; ++j) {
                Debug.Log($"loop {i}, result {j}: {pccJob.R[j]}");
            }
        }

        // Dispose of the native containers once we are through with the job
        pccJob.Dispose();
    }
}
