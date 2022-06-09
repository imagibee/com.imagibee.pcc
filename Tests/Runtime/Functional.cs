using NUnit.Framework;
using Unity.Collections;
using Imagibee.Parallel;
using UnityEngine;
using System.Collections.Generic;

public class Functional
{
    [Test]
    public void Sum()
    {
        Assert.AreEqual(15, Functions.Sum(new float[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public void SumProd()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        Assert.AreEqual(55, Functions.SumProd(x, x));
    }

    [Test]
    public void ParallelSum()
    {
        var buf1 = new NativeArray<float>(5, Allocator.TempJob);
        var buf2 = new NativeReference<float>(Allocator.TempJob);
        var sumJob = new SumJob()
        {
            Src = buf1,
            Result = buf2
        };
        sumJob.Src.CopyFrom(new float[] { 1, 2, 3, 4, 5 });
        sumJob.Schedule(sumJob.Src.Length, 2).Complete();
        var result = sumJob.Result.Value;
        buf1.Dispose();
        buf2.Dispose();
        Assert.AreEqual(15f, result);
    }

    [Test]
    public void ParallelProduct()
    {
        var data = new float[] { 1, 2, 3, 4, 5 };
        var buf1 = new NativeArray<float>(data.Length, Allocator.TempJob);
        var buf2 = new NativeArray<float>(data.Length, Allocator.TempJob);
        var buf3 = new NativeArray<float>(data.Length, Allocator.TempJob);
        var dotJob = new ProductJob
        {
            Src1 = buf1,
            Src2 = buf2,
            Dst = buf3
        };
        dotJob.Src1.CopyFrom(data);
        dotJob.Src2.CopyFrom(data);
        dotJob.Schedule(data.Length, 2).Complete();
        dotJob.Dst.CopyTo(data);
        buf1.Dispose();
        buf2.Dispose();
        buf3.Dispose();
        Assert.AreEqual(new float[] { 1, 4, 9, 16, 25 }, data);
    }

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
        var pccJobv2 = new PccJobv2();

        pccJobv2.Allocate(x.Length, y.Length/x.Length);
        pccJobv2.X.CopyFrom(x);
        pccJobv2.Y.CopyFrom(y);
        pccJobv2.Schedule().Complete();
        for (var i=0; i< y.Length / x.Length; ++i) {
            results.Add(pccJobv2.R[i]);
        }
        pccJobv2.Dispose();
        Assert.AreEqual(1f, results[0]);
        Assert.AreEqual(-1f, results[1]);
    }
}
