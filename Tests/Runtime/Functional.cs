using NUnit.Framework;
using Unity.Collections;
using Imagibee.Parallel;
using UnityEngine;
using System.Collections.Generic;

public class Functional
{
    [Test]
    public void SerialPcc()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { -1, -2, -3, -4, -5 };
        Assert.AreEqual(1f, Functions.Pcc(x, x));
        Assert.AreEqual(-1f, Functions.Pcc(x, y));
    }

    [Test]
    public void ParallelPccv1()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { -1, -2, -3, -4, -5 };
        var pccJob = new PccJobv1()
        {
            Allocator = Allocator.Persistent,
            Length = x.Length,
            Width = x.Length / 2
        };
        pccJob.Allocate();
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(x);
        pccJob.Schedule().Complete();
        var pccXX = pccJob.Result.Value;
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        var pccYY = pccJob.Result.Value;
        pccJob.Dispose();
        Assert.AreEqual(1f, pccXX);
        Assert.AreEqual(-1f, pccYY);
    }

    [Test]
    public void ParallelPccv2()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJobv2();

        pccJob.Allocate(x.Length, y.Length/x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i=0; i< y.Length/x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }

    [Test]
    public void ParallelPccv3()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJobv3();

        pccJob.Allocate(x.Length, y.Length/x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i = 0; i < y.Length/x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }

    [Test]
    public void ParallelPccv4()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJobv4();

        pccJob.Allocate(x.Length, y.Length / x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i = 0; i < y.Length / x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }

    [Test]
    public void ParallelPccv6()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJobv6();

        pccJob.Allocate(x.Length, y.Length / x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i = 0; i < y.Length / x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }

    [Test]
    public void ParallelPccv7()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };
        var results = new List<float>();
        var pccJob = new PccJobv7();

        pccJob.Allocate(x.Length, y.Length / x.Length);
        pccJob.X.CopyFrom(x);
        pccJob.Y.CopyFrom(y);
        pccJob.Schedule().Complete();
        for (var i = 0; i < y.Length / x.Length; ++i) {
            results.Add(pccJob.R[i]);
        }
        pccJob.Dispose();
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }

    [Test]
    public void SerialPccv5()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[2][];
        y[0] = new float[] { 1, 2, 3, 4, 5 };
        y[1] = new float[] { -1, -2, -3, -4, -5 };
        var results = Functions.Pccv5(x, y);
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }

    [Test]
    public void PccJobUsage()
    {
        // Here x represents a frequently changing value that is
        // correlated against a set of infrequently changing y.
        var x = new List<float[]>
        {
            new float[] { 1, 2, 3, 4, 5 },
            new float[] { 1, 2, 3, 4, 5 }
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
        var pccJob = new PccJobv6();

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

    //[Test]
    //public void Sum()
    //{
    //    Assert.AreEqual(15, Functions.Sum(new float[] { 1, 2, 3, 4, 5 }));
    //}

    //[Test]
    //public void SumProd()
    //{
    //    var x = new float[] { 1, 2, 3, 4, 5 };
    //    Assert.AreEqual(55, Functions.SumProd(x, x));
    //}

    //[Test]
    //public void ParallelSum()
    //{
    //    var buf1 = new NativeArray<float>(5, Allocator.TempJob);
    //    var buf2 = new NativeReference<float>(Allocator.TempJob);
    //    var sumJob = new SumJob()
    //    {
    //        Src = buf1,
    //        Result = buf2
    //    };
    //    sumJob.Src.CopyFrom(new float[] { 1, 2, 3, 4, 5 });
    //    sumJob.Schedule(sumJob.Src.Length, 2).Complete();
    //    var result = sumJob.Result.Value;
    //    buf1.Dispose();
    //    buf2.Dispose();
    //    Assert.AreEqual(15f, result);
    //}

    //[Test]
    //public void ParallelProduct()
    //{
    //    var data = new float[] { 1, 2, 3, 4, 5 };
    //    var buf1 = new NativeArray<float>(data.Length, Allocator.TempJob);
    //    var buf2 = new NativeArray<float>(data.Length, Allocator.TempJob);
    //    var buf3 = new NativeArray<float>(data.Length, Allocator.TempJob);
    //    var dotJob = new ProductJob
    //    {
    //        Src1 = buf1,
    //        Src2 = buf2,
    //        Dst = buf3
    //    };
    //    dotJob.Src1.CopyFrom(data);
    //    dotJob.Src2.CopyFrom(data);
    //    dotJob.Schedule(data.Length, 2).Complete();
    //    dotJob.Dst.CopyTo(data);
    //    buf1.Dispose();
    //    buf2.Dispose();
    //    buf3.Dispose();
    //    Assert.AreEqual(new float[] { 1, 4, 9, 16, 25 }, data);
    //}
}
