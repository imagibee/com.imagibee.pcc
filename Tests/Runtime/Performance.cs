using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Collections;
using Imagibee.Parallel;

public class Performance {
    private readonly int[] LENGTHS = { 8192, 1000000 };
    private readonly int[] WIDTHS = { 3000, 3000 };

    [Test, Performance]
    public void SerialPcc()
    {
        foreach (var length in LENGTHS) {
            var x = new float[length];
            var y = new float[length];
            var c = 0f;
            Measure.Method(() =>
            {
                c = Baseline.Pcc(x, y);
            }).SampleGroup($"Serial Pcc (length={length})").Run();
        }
    }

    [Test, Performance]
    public void SerialSum()
    {
        foreach (var length in LENGTHS) {
            var x = new float[length];
            var sum = 0f;
            Measure.Method(() =>
            {
                for (var j = 0; j < length; ++j) {
                    sum += x[j];
                }
            }).SampleGroup($"Serial sum (length={length})").Run();
        }
    }

    [Test, Performance]
    public void ParallelSum()
    {
        for (var i = 0; i < LENGTHS.Length; ++i) {
            var sumJob = new SumJob
            {
                Src = new NativeArray<float>(LENGTHS[i], Allocator.TempJob),
                Dst = new NativeArray<float>(LENGTHS[i], Allocator.TempJob),
            };
            Measure.Method(() =>
            {
                sumJob.Schedule(LENGTHS[i], WIDTHS[i]).Complete();
            }).SampleGroup($"Parallel sum (length={LENGTHS[i]}, width={WIDTHS[i]})").Run();
            sumJob.Dispose();
        }
    }

    [Test, Performance]
    public void ParallelProduct()
    {
        for (var i = 0; i < LENGTHS.Length; ++i) {
            var productJob = new ProductJob
            {
                Src1 = new NativeArray<float>(LENGTHS[i], Allocator.TempJob),
                Src2 = new NativeArray<float>(LENGTHS[i], Allocator.TempJob),
                Dst = new NativeArray<float>(LENGTHS[i], Allocator.TempJob)
            };
            Measure.Method(() =>
            {
                productJob.Schedule(LENGTHS[i], WIDTHS[i]).Complete();
            }).SampleGroup($"Parallel product (length={LENGTHS[i]}, width={WIDTHS[i]})").Run();
            productJob.Dispose();
        }
    }

    [Test, Performance]
    public void ParallelPcc()
    {
        for (var i = 0; i < LENGTHS.Length; ++i) {
            var pccJob = new PccJob()
            {
                Allocator = Allocator.Persistent,
                Length = LENGTHS[i],
                Width = WIDTHS[i]
            };
            pccJob.Allocate();
            Measure.Method(() =>
            {
                pccJob.Schedule().Complete();
            }).SampleGroup($"Parallel PCC (length={LENGTHS[i]}, width={WIDTHS[i]})").Run();
            pccJob.Dispose();
        }
    }

    [Test, Performance]
    public void ParallelCopyFrom()
    {
        for (var i = 0; i < LENGTHS.Length; ++i) {
            var x = new float[LENGTHS[i]];
            var pccJob = new PccJob()
            {
                Allocator = Allocator.Persistent,
                Length = LENGTHS[i],
                Width = WIDTHS[i]
            };
            pccJob.Allocate();
            Measure.Method(() =>
            {
                pccJob.X.CopyFrom(x);
                pccJob.Y.CopyFrom(x);
            }).SampleGroup($"Parallel copy from (length={LENGTHS[i]})").Run();
            pccJob.Dispose();
        }
    }
}
