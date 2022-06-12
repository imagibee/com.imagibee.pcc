using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using UnityEngine;

namespace Imagibee.Parallel {
    [BurstCompile]
    public struct PccJobv1 : IDisposable {
        public Allocator Allocator;
        public int Length;
        public int Width;
        public bool DisposeX;

        public NativeArray<float> X;
        public NativeArray<float> Y;
        public NativeReference<float> SumX;
        public NativeReference<float> SumY;
        public NativeReference<float> SumProdXY;
        public NativeReference<float> SumProdXX;
        public NativeReference<float> SumProdYY;
        public NativeArray<float> ProdXY;
        public NativeArray<float> ProdXX;
        public NativeArray<float> ProdYY;
        public NativeReference<float> Result;

        [BurstCompile]
        struct MergePccJob : IJob {
            [ReadOnly]
            public NativeReference<float> SumX;
            [ReadOnly]
            public NativeReference<float> SumY;
            [ReadOnly]
            public NativeReference<float> SumProdXY;
            [ReadOnly]
            public NativeReference<float> SumProdXX;
            [ReadOnly]
            public NativeReference<float> SumProdYY;
            [WriteOnly]
            public NativeReference<float> Result;
            public int Length;

            public void Execute()
            {
                Result.Value =
                    (Length * SumProdXY.Value - SumX.Value * SumY.Value) /
                    math.sqrt(Length * SumProdXX.Value - SumX.Value * SumX.Value) /
                    math.sqrt(Length * SumProdYY.Value - SumY.Value * SumY.Value);
            }
        }

        public void Allocate(NativeArray<float> X)
        {
            this.X = X;
            Y = new NativeArray<float>(Length, Allocator);
            SumX = new NativeReference<float>(Allocator);
            SumY = new NativeReference<float>(Allocator);
            ProdXY = new NativeArray<float>(Length, Allocator);
            ProdXX = new NativeArray<float>(Length, Allocator);
            ProdYY = new NativeArray<float>(Length, Allocator);
            SumProdXY = new NativeReference<float>(Allocator);
            SumProdXX = new NativeReference<float>(Allocator);
            SumProdYY = new NativeReference<float>(Allocator);
            Result = new NativeReference<float>(Allocator);
        }
        public void Allocate()
        {
            Allocate(new NativeArray<float>(Length, Allocator));
            DisposeX = true;
        }

        public JobHandle Schedule(JobHandle deps = new JobHandle())
        {
            var sumX = new SumJob
            {
                Src = X,
                Result = SumX
            };
            var sumY = new SumJob
            {
                Src = Y,
                Result = SumY
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
                Result = SumProdXY
            };
            var sumProdXX = new SumJob
            {
                Src = ProdXX,
                Result = SumProdXX
            };
            var sumProdYY = new SumJob
            {
                Src = ProdYY,
                Result = SumProdYY
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
            if (DisposeX) {
                X.Dispose();
            }
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