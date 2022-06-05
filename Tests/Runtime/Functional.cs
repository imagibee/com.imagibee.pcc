using NUnit.Framework;
using Unity.Collections;
using Imagibee.Parallel;

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
        var sumJob = new SumJob()
        {
            Src = new NativeArray<float>(5, Allocator.TempJob),
            Dst = new NativeArray<float>(5, Allocator.TempJob)
        };
        sumJob.Src.CopyFrom(new float[] { 1, 2, 3, 4, 5 });
        sumJob.Schedule(sumJob.Src.Length, 2).Complete();
        var result = sumJob.Dst[0];
        sumJob.Dispose();
        Assert.AreEqual(15f, result);
    }

    [Test]
    public void ParallelProduct()
    {
        var data = new float[] { 1, 2, 3, 4, 5 };
        var dotJob = new ProductJob
        {
            Src1 = new NativeArray<float>(data.Length, Allocator.TempJob),
            Src2 = new NativeArray<float>(data.Length, Allocator.TempJob),
            Dst = new NativeArray<float>(data.Length, Allocator.TempJob)
        };
        dotJob.Src1.CopyFrom(data);
        dotJob.Src2.CopyFrom(data);
        dotJob.Schedule(data.Length, 2).Complete();
        dotJob.Dst.CopyTo(data);
        dotJob.Dispose();
        Assert.AreEqual(new float[] { 1, 4, 9, 16, 25 }, data);
    }

    [Test]
    public void SerialPcc()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { -1, -2, -3, -4, -5 };
        Assert.AreEqual(1f, Baseline.Pcc(x, x));
        Assert.AreEqual(-1f, Baseline.Pcc(x, y));
    }

    [Test]
    public void ParallelPcc()
    {
        var x = new float[] { 1, 2, 3, 4, 5 };
        var y = new float[] { -1, -2, -3, -4, -5 };
        var pccJob = new PccJob()
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
}
