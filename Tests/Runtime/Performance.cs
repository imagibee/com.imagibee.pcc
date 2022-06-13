using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Collections;
using Imagibee.Parallel;

public class Performance {
    private const int TINY_LENGTH = 1000;
    private const int TINY_YCOUNT = 1000;
    private const int LARGE_LENGTH = 1000000;
    private const int LARGE_YCOUNT = 1;

    [Test, Performance]
    public void SerialPccTiny()
    {
        var x = new float[TINY_LENGTH];
        var y = new float[TINY_YCOUNT][];
        for (var i = 0; i < TINY_YCOUNT; ++i) {
            y[i] = new float[TINY_LENGTH];
        }
        Measure.Method(() =>
        {
            var r = Functions.Pcc(x, y);
        }).SampleGroup($"Serial Pcc (length={TINY_LENGTH}, ycount={TINY_YCOUNT})").Run();
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
            var r = Functions.Pcc(x, y);
        }).SampleGroup($"Serial Pcc (length={LARGE_LENGTH}, ycount={LARGE_YCOUNT})").Run();
    }

    [Test, Performance]
    public void ParallelPccvTiny()
    {

        var x = new float[TINY_LENGTH];
        var pccJob = new PccJob();
        pccJob.Allocate(TINY_LENGTH, TINY_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCC (length={TINY_LENGTH}, ycount={TINY_YCOUNT})").Run();
        pccJob.Dispose();
    }

    [Test, Performance]
    public void ParallelPccLarge()
    {
        var x = new float[LARGE_LENGTH];
        var pccJob = new PccJob();
        pccJob.Allocate(LARGE_LENGTH, LARGE_YCOUNT);
        Measure.Method(() =>
        {
            pccJob.X.CopyFrom(x);
            pccJob.Schedule().Complete();
        }).SampleGroup($"Parallel PCC (length={LARGE_LENGTH}, ycount={LARGE_YCOUNT})").Run();
        pccJob.Dispose();
    }
}
