using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using Imagibee.AudioId;

public class Functional
{
    [Test]
    public void Sum()
    {
        Assert.AreEqual(15, Baseline.Sum(new float[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public void SumProd()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        Assert.AreEqual(55, Baseline.SumProd(x, x));
    }

    [Test]
    public void ParallelSum()
    {
        var r = new SumJob()
        {
            Src = new NativeArray<float>(5, Allocator.TempJob),
            Dst = new NativeArray<float>(5, Allocator.TempJob)
        };
        r.Src.CopyFrom(new float[] { 1, 2, 3, 4, 5 });
        r.Schedule(r.Src.Length, 2).Complete();
        var result = r.Dst[0];
        r.Dispose();
        Assert.AreEqual(15f, result);
    }

    [Test]
    public void ParallelProduct()
    {
        var data = new float[] { 1, 2, 3, 4, 5 };
        var prodJob = new ProductJob
        {
            Src1 = new NativeArray<float>(data.Length, Allocator.TempJob),
            Src2 = new NativeArray<float>(data.Length, Allocator.TempJob),
            Dst = new NativeArray<float>(data.Length, Allocator.TempJob)
        };
        prodJob.Src1.CopyFrom(data);
        prodJob.Src2.CopyFrom(data);
        prodJob.Schedule(data.Length, 2).Complete();
        prodJob.Dst.CopyTo(data);
        prodJob.Dispose();
        Assert.AreEqual(new float[] { 1, 4, 9, 16, 25 }, data);
    }

    [Test]
    public void SerialCorrelation()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { -1, -2, -3, -4, -5 };
        Assert.AreEqual(1f, Baseline.Correlation(x, x));
        Assert.AreEqual(-1f, Baseline.Correlation(x, y));
    }

    [Test]
    public void ParallelCorrelation()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { -1, -2, -3, -4, -5 };
        var correlationJob = new CorrelationJob()
        {
            Allocator = Allocator.Persistent,
            Length = x.Length,
            Width = 2
        };
        correlationJob.Allocate();
        correlationJob.X.CopyFrom(x);
        correlationJob.Y.CopyFrom(x);
        correlationJob.Schedule().Complete();
        var valueXX = correlationJob.Result.Value;
        correlationJob.Y.CopyFrom(y);
        correlationJob.Schedule().Complete();
        var valueXY = correlationJob.Result.Value;
        correlationJob.Dispose();
        Assert.AreEqual(1f, valueXX);
        Assert.AreEqual(-1f, valueXY);
    }
}
