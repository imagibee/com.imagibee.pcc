using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace Imagibee.Parallel
{
    [BurstCompile]
    public struct PccJob : IDisposable
    {
        public Allocator Allocator;
        public int Length;
        public int Width;

        [ReadOnly]
        public NativeArray<float> X;
        [ReadOnly]
        public NativeArray<float> Y;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> SumX;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> SumY;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> SumProdXY;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> SumProdXX;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> SumProdYY;
        [WriteOnly]
        public NativeArray<float> ProdXY;
        [WriteOnly]
        public NativeArray<float> ProdXX;
        [WriteOnly]
        public NativeArray<float> ProdYY;
        [WriteOnly]
        public NativeReference<float> Result;

        [BurstCompile]
        struct MergePccJob : IJob {
            public NativeArray<float> SumX;
            public NativeArray<float> SumY;
            public NativeArray<float> SumProdXY;
            public NativeArray<float> SumProdXX;
            public NativeArray<float> SumProdYY;
            public NativeReference<float> Result;
            public int Length;

            public void Execute()
            {
                Result.Value =
                    (Length * SumProdXY[0] - SumX[0] * SumY[0]) /
                    math.sqrt(Length * SumProdXX[0] - SumX[0] * SumX[0]) /
                    math.sqrt(Length * SumProdYY[0] - SumY[0] * SumY[0]);
            }
        }

        public void Allocate()
        {
            X = new NativeArray<float>(Length, Allocator);
            Y = new NativeArray<float>(Length, Allocator);
            SumX = new NativeArray<float>(Length, Allocator);
            SumY = new NativeArray<float>(Length, Allocator);
            ProdXY = new NativeArray<float>(Length, Allocator);
            ProdXX = new NativeArray<float>(Length, Allocator);
            ProdYY = new NativeArray<float>(Length, Allocator);
            SumProdXY = new NativeArray<float>(Length, Allocator);
            SumProdXX = new NativeArray<float>(Length, Allocator);
            SumProdYY = new NativeArray<float>(Length, Allocator);
            Result = new NativeReference<float>(Allocator);
        }

        public JobHandle Schedule(JobHandle deps = new JobHandle())
        {
            var sumX = new SumJob
            {
                Src = X,
                Dst = SumX
            };
            var sumY = new SumJob
            {
                Src = Y,
                Dst = SumY
            };
            var prodXY = new ProductJob
            {
                Src1 = X,
                Src2 = Y,
                Dst = ProdXY
            };
            var prodXX = new ProductJob
            {
                Src1 = X,
                Src2 = X,
                Dst = ProdXX
            };
            var prodYY = new ProductJob
            {
                Src1 = Y,
                Src2 = Y,
                Dst = ProdYY
            };
            var sumProdXY = new SumJob
            {
                Src = ProdXY,
                Dst = SumProdXY
            };
            var sumProdXX = new SumJob
            {
                Src = ProdXX,
                Dst = SumProdXX
            };
            var sumProdYY = new SumJob
            {
                Src = ProdYY,
                Dst = SumProdYY
            };
            var correlation = new MergePccJob
            {
                SumX = SumX,
                SumY = SumY,
                SumProdXY = SumProdXY,
                SumProdXX = SumProdXX,
                SumProdYY = SumProdYY,
                Result = Result,
                Length = Length
            };
            var sumJobs = JobHandle.CombineDependencies(
                sumX.Schedule(Length, Width, deps),
                sumY.Schedule(Length, Width, deps));
            var prodXYJob = prodXY.Schedule(Length, Width, deps);
            var prodXXJob = prodXX.Schedule(Length, Width, deps);
            var prodYYJob = prodYY.Schedule(Length, Width, deps);
            var sumProdJobs = JobHandle.CombineDependencies(
                sumProdXY.Schedule(Length, Width, prodXYJob),
                sumProdXX.Schedule(Length, Width, prodXXJob),
                sumProdYY.Schedule(Length, Width, prodYYJob));
            return correlation.Schedule(JobHandle.CombineDependencies(
                sumJobs,
                sumProdJobs));
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            SumX.Dispose();
            SumY.Dispose();
            ProdXY.Dispose();
            ProdXX.Dispose();
            ProdYY.Dispose();
            SumProdXY.Dispose();
            SumProdXX.Dispose();
            SumProdYY.Dispose();
            Result.Dispose();
        }
    }
}
