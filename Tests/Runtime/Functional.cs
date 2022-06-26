using NUnit.Framework;
using Imagibee.Parallel;
using UnityEngine;
using System.Collections.Generic;

public class Functional
{
    [Test]
    public void ParallelPcc()
    {
        var x = new float[] { 0, 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJob();
        pccJob.Allocate(x.Length-1, y.Length / x.Length-1);
        pccJob.CopyToX(x, 1);
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
        pccJob.CopyToX(x, 0);
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
        pccJob.CopyToX(x, 0);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        var result = pccJob.R[0];
        var resultBaseline = Functions.Pcc(x, y);
        Debug.Log($"PccJob correlation is {result} (baseline is {resultBaseline})");
        pccJob.Dispose();
        Assert.LessOrEqual(result, 1f);
        Assert.GreaterOrEqual(result, -1f);
        Assert.LessOrEqual(result / resultBaseline, 1.01f);
        Assert.GreaterOrEqual(result / resultBaseline, 0.99f);
    }
}
