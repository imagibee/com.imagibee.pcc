using NUnit.Framework;
using Unity.PerformanceTesting;
using Imagibee.Parallel;

public class Performance {
    readonly int[] LENGTHS = { 10, 100, 1000, 10000, 20000, 100000, 1000000 };
    readonly int[] YCOUNTS = { 100000, 10000, 1000, 100,   50,    10,     1};

    [Test, Performance]
    public void SerialPcc()
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
            }).SampleGroup($"Serial Pcc (length={LENGTHS[k]}, ycount={YCOUNTS[k]})").Run();
        }
    }

    [Test, Performance]
    public void ParallelPcc()
    {
        for (int k = 0; k < LENGTHS.Length; ++k) {
            var x = new float[LENGTHS[k]];
            var pccJob = new PccJob();
            pccJob.Allocate(LENGTHS[k], YCOUNTS[k]);
            Measure.Method(() =>
            {
                pccJob.X.CopyFrom(x);
                pccJob.Schedule().Complete();
            }).SampleGroup($"Parallel PCC (length={LENGTHS[k]}, ycount={YCOUNTS[k]})").Run();
            pccJob.Dispose();
        }
    }
}
