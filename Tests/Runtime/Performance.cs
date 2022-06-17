using NUnit.Framework;
using Unity.PerformanceTesting;
using Imagibee.Parallel;

public class Performance {
    readonly int[] LENGTHS = { 10, 100, 1000, 10000, 20000, 100000, 1000000 };
    readonly int[] YCOUNTS = { 100000, 10000, 1000, 100, 50, 10, 1 };

    [Test, Performance]
    public void BaselinePccOriginal()
    {
        const int LENGTH = 1000;
        const int YCOUNT = 1000;
        var x = new float[LENGTH];
        var y = new float[LENGTH];
        Measure.Method(() =>
        {
            for (var i = 0; i < YCOUNT; ++i) {
                var r = Functions.Pcc(x, y);
            }
        }).SampleGroup($"Baseline Pcc Original (length={LENGTH}, ycount={YCOUNT})").Run();
    }

    [Test, Performance]
    public void BaselinePcc()
    {
        for (int k = 0; k < LENGTHS.Length; ++k) {
            var x = new float[LENGTHS[k]];
            var y = new float[YCOUNTS[k]][];
            for (var i = 0; i < YCOUNTS[k]; ++i) {
                y[i] = new float[LENGTHS[k]];
            }
            Measure.Method(() =>
            {
                var r = Functions.Pcc(x, y);
            }).SampleGroup($"Baseline Pcc (length={LENGTHS[k]}, ycount={YCOUNTS[k]})").Run();
        }
    }

    [Test, Performance]
    public void OptimizedPcc()
    {
        for (int k = 0; k < LENGTHS.Length; ++k) {
            var x = new float[LENGTHS[k]];
            var pccJob = new PccJob();
            pccJob.Allocate(LENGTHS[k], YCOUNTS[k]);
            Measure.Method(() =>
            {
                pccJob.X.CopyFrom(x);
                pccJob.Schedule().Complete();
            }).SampleGroup($"Optimized PCC (length={LENGTHS[k]}, ycount={YCOUNTS[k]})").Run();
            pccJob.Dispose();
        }
    }
}
