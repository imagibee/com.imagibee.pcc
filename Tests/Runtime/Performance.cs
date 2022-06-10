using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Collections;
using Imagibee.Parallel;

public class Performance {
    private readonly int[] LENGTHS = { 1000, 10000, 1000000 };
    private readonly int[] WIDTHS = { 1000, 3000, 3000 };
    private const int TINY_LENGTH = 1000;
    private const int TINY_YCOUNT = 1000;
    private const int LARGE_LENGTH = 1000000;
    private const int LARGE_YCOUNT = 1;

    [Test, Performance]
    public void SerialPcc()
    {
        foreach (var length in LENGTHS) {
            var x = new float[length];
            var y = new float[length];
            var c = 0f;
            Measure.Method(() =>
            {
                c = Functions.Pcc(x, y);
            }).SampleGroup($"Serial Pcc (length={length})").Run();
        }
    }

    [Test, Performance]
    public void ParallelPccv1()
    {
        for (var i = 0; i < LENGTHS.Length; ++i) {
            var pccJob = new PccJobv1()
            {
                Allocator = Allocator.Persistent,
                Length = LENGTHS[i],
                Width = WIDTHS[i]
            };
            pccJob.Allocate();
            Measure.Method(() =>
            {
                pccJob.Schedule().Complete();
            }).SampleGroup($"Parallel PCCv1 (length={LENGTHS[i]}, width={WIDTHS[i]})").Run();
            pccJob.Dispose();
        }
    }

    [Test, Performance]
    public void ParallelPccv2Tiny()
    {
        var x = new float[TINY_LENGTH];
        var pccJob = new PccJobv2();
        pccJob.Allocate(TINY_LENGTH, TINY_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCCv2 (length={TINY_LENGTH}, ycount={TINY_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void ParallelPccv2Large()
    {
        var x = new float[LARGE_LENGTH];
        var pccJob = new PccJobv2();
        pccJob.Allocate(LARGE_LENGTH, LARGE_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCCv2 (length={LARGE_LENGTH}, ycount={LARGE_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void ParallelPccv3Tiny()
    {
        var x = new float[TINY_LENGTH];
        var pccJob = new PccJobv3();
        pccJob.Allocate(TINY_LENGTH, TINY_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCCv3 (length={TINY_LENGTH}, ycount={TINY_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void ParallelPccv3Large()
    {
        var x = new float[LARGE_LENGTH];
        var pccJob = new PccJobv3();
        pccJob.Allocate(LARGE_LENGTH, LARGE_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCCv3 (length={LARGE_LENGTH}, ycount={LARGE_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void ParallelPccv4Tiny()
    {

        var x = new float[TINY_LENGTH];
        var pccJob = new PccJobv4();
        pccJob.Allocate(TINY_LENGTH, TINY_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCCv4 (length={TINY_LENGTH}, ycount={TINY_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void ParallelPccv4Large()
    {
        var x = new float[LARGE_LENGTH];
        var pccJob = new PccJobv4();
        pccJob.Allocate(LARGE_LENGTH, LARGE_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCCv4 (length={LARGE_LENGTH}, ycount={LARGE_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void SerialPccv5Tiny()
    {
        var x = new float[TINY_LENGTH];
        var y = new float[TINY_YCOUNT][];
        for (var i = 0; i < TINY_YCOUNT; ++i) {
            y[i] = new float[TINY_LENGTH];
        }
        Measure.Method(() =>
        {
            var r = Functions.Pccv5(x, y);
        }).SampleGroup($"Serial Pccv5 (length={TINY_LENGTH}, ycount={TINY_YCOUNT})").Run();
    }

    [Test, Performance]
    public void SerialPccv5Large()
    {
        var x = new float[LARGE_LENGTH];
        var y = new float[LARGE_YCOUNT][];
        for (var i = 0; i < LARGE_YCOUNT; ++i) {
            y[i] = new float[LARGE_LENGTH];
        }
        Measure.Method(() =>
        {
            var r = Functions.Pccv5(x, y);
        }).SampleGroup($"Serial Pccv5 (length={LARGE_LENGTH}, ycount={LARGE_YCOUNT})").Run();
    }

    //[Test, Performance]
    //public void ParallelPcc2()
    //{
    //    const int LENGTH = 1000;
    //    const int NUMJOBS = 1000;
    //    var X = new NativeArray<float>(LENGTH, Allocator.Persistent);
    //    var pccJobs = new List<PccJob>();
    //    var job = new JobHandle();
    //    for (var i = 0; i < NUMJOBS; i++) {
    //        var pccJob = new PccJob
    //        {
    //            Allocator = Allocator.Persistent,
    //            Length = LENGTH,
    //            Width = LENGTH
    //        };
    //        pccJob.Allocate(X);
    //        pccJobs.Add(pccJob);
    //    }
    //    Measure.Method(() =>
    //    {
    //        for (var i = 0; i < NUMJOBS; i++) {
    //            job = pccJobs[i].Schedule(job);
    //        }
    //        job.Complete();
    //    }).SampleGroup($"Parallel PCC2 (length={LENGTH}, jobs={NUMJOBS})").Run();
    //    for (var i = 0; i < NUMJOBS; i++) {
    //        pccJobs[i].Dispose();
    //    }
    //    X.Dispose();
    //}

    //[Test, Performance]
    //public void SerialSum()
    //{
    //    foreach (var length in LENGTHS) {
    //        var x = new float[length];
    //        var sum = 0f;
    //        Measure.Method(() =>
    //        {
    //            for (var j = 0; j < length; ++j) {
    //                sum += x[j];
    //            }
    //        }).SampleGroup($"Serial sum (length={length})").Run();
    //    }
    //}

    //[Test, Performance]
    //public void ParallelSum()
    //{
    //    for (var i = 0; i < LENGTHS.Length; ++i) {
    //        var buf1 = new NativeArray<float>(LENGTHS[i], Allocator.TempJob);
    //        var buf2 = new NativeReference<float>(Allocator.TempJob);
    //        var sumJob = new SumJob
    //        {
    //            Src = buf1,
    //            Result = buf2
    //        };
    //        Measure.Method(() =>
    //        {
    //            sumJob.Schedule(LENGTHS[i], WIDTHS[i]).Complete();
    //        }).SampleGroup($"Parallel sum (length={LENGTHS[i]}, width={WIDTHS[i]})").Run();
    //        buf1.Dispose();
    //        buf2.Dispose();
    //    }
    //}

    //[Test, Performance]
    //public void ParallelProduct()
    //{
    //    for (var i = 0; i < LENGTHS.Length; ++i) {
    //        var buf1 = new NativeArray<float>(LENGTHS[i], Allocator.TempJob);
    //        var buf2 = new NativeArray<float>(LENGTHS[i], Allocator.TempJob);
    //        var buf3 = new NativeArray<float>(LENGTHS[i], Allocator.TempJob);
    //        var productJob = new ProductJob
    //        {
    //            Src1 = buf1,
    //            Src2 = buf2,
    //            Dst = buf3
    //        };
    //        Measure.Method(() =>
    //        {
    //            productJob.Schedule(LENGTHS[i], WIDTHS[i]).Complete();
    //        }).SampleGroup($"Parallel product (length={LENGTHS[i]}, width={WIDTHS[i]})").Run();
    //        buf1.Dispose();
    //        buf2.Dispose();
    //        buf3.Dispose();
    //    }
    //}


    //[Test, Performance]
    //public void ParallelCopyFrom()
    //{
    //    for (var i = 0; i < LENGTHS.Length; ++i) {
    //        var x = new float[LENGTHS[i]];
    //        var pccJob = new PccJobv1()
    //        {
    //            Allocator = Allocator.Persistent,
    //            Length = LENGTHS[i],
    //            Width = WIDTHS[i]
    //        };
    //        pccJob.Allocate();
    //        Measure.Method(() =>
    //        {
    //            pccJob.X.CopyFrom(x);
    //            pccJob.Y.CopyFrom(x);
    //        }).SampleGroup($"Parallel copy from (length={LENGTHS[i]})").Run();
    //        pccJob.Dispose();
    //    }
    //}
}
