using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using UnityEngine;

namespace Imagibee.Parallel {
    [BurstCompile]
    public struct PccJobv3 : IDisposable {
        public int Length;
        public int YCount;
        public NativeArray<float> X;
        public NativeArray<float> Y;
        public NativeArray<float> XSum;
        public NativeArray<float> YSum;
        public NativeArray<float> XXSumProd;
        public NativeArray<float> YYSumProd;
        public NativeArray<float> XYSumProd;
        public NativeArray<float> R;

        [BurstCompile]
        struct PccPartitionJobX : IJobParallelForBatch {
            public int YCount;
            public int Length;
            [ReadOnly]
            public NativeArray<float> X;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> XSum;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> XXSumProd;

            public void Execute(int startIndex, int count)
            {
                var sum = 0f;
                var sumProd = 0f;
                for (var i = startIndex; i < startIndex + count; ++i) {
                    sum += X[i];
                    sumProd += X[i] * X[i];
                }
                XSum[0] = sum;
                XXSumProd[0] = sumProd;
            }
        }

        [BurstCompile]
        struct PccPartitionJobY : IJobParallelForBatch {
            public int YCount;
            public int Length;
            [ReadOnly]
            public NativeArray<float> X;
            [ReadOnly]
            public NativeArray<float> Y;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> YSum;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> YYSumProd;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> XYSumProd;

            public void Execute(int startIndex, int count)
            {
                var sum = 0f;
                var sumProdYY = 0f;
                var sumProdXY = 0f;
                var sumIndex = startIndex / Length;
                for (var i = startIndex; i < startIndex + count; ++i) {
                    sum += Y[i];
                    sumProdYY += Y[i] * Y[i];
                    sumProdXY += Y[i] * X[i - startIndex];
                }
                YSum[sumIndex] = sum;
                YYSumProd[sumIndex] = sumProdYY;
                XYSumProd[sumIndex] = sumProdXY;
            }
        }

        [BurstCompile]
        struct PccMergeJob : IJobParallelFor {
            public int Length;
            [ReadOnly]
            public NativeArray<float> XSum;
            [ReadOnly]
            public NativeArray<float> YSum;
            [ReadOnly]
            public NativeArray<float> XXSumProd;
            [ReadOnly]
            public NativeArray<float> YYSumProd;
            [ReadOnly]
            public NativeArray<float> XYSumProd;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> R;

            public void Execute(int i)
            {
                //Debug.Log($"i={i}, XSum={XSum[0]}, XXSumProd={XXSumProd[0]}, YSum={YSum[i]}, YYSUmProd={YYSumProd[0]}, XYSumProd={XYSumProd[i]}");
                R[i] =
                    (Length * XYSumProd[i] - XSum[0] * YSum[i]) /
                    (math.sqrt(Length * XXSumProd[0] - XSum[0] * XSum[0])) /
                    (math.sqrt(Length * YYSumProd[i] - YSum[i] * YSum[i]));
            }
        }
        public JobHandle Schedule(JobHandle deps = new JobHandle())
        {
            var jobs = new NativeList<JobHandle>(2, Allocator.TempJob);
            var xPartitionJob = new PccPartitionJobX
            {
                Length = Length,
                YCount = 1,
                X = X,
                XSum = XSum,
                XXSumProd = XXSumProd
            };
            jobs.Add(xPartitionJob.ScheduleBatch(Length, Length, deps));

            var yPartitionJob = new PccPartitionJobY
            {
                Length = Length,
                YCount = YCount,
                X = X,
                Y = Y,
                YSum = YSum,
                YYSumProd = YYSumProd,
                XYSumProd = XYSumProd
            };
            jobs.Add(yPartitionJob.ScheduleBatch(Length * YCount, Length, deps));

            var mergeJob = new PccMergeJob
            {
                Length = Length,
                XSum = XSum,
                YSum = YSum,
                XXSumProd = XXSumProd,
                YYSumProd = YYSumProd,
                XYSumProd = XYSumProd,
                R = R
            };
            var newDeps = mergeJob.Schedule(YCount, 1, JobHandle.CombineDependencies(jobs));
            jobs.Dispose();
            return newDeps;
        }

        public void Allocate(int length, int ycount, Allocator allocator = Allocator.Persistent)
        {
            Length = length;
            YCount = ycount;
            X = new NativeArray<float>(Length, allocator);
            Y = new NativeArray<float>(Length * YCount, allocator);
            XSum = new NativeArray<float>(1, allocator);
            YSum = new NativeArray<float>(YCount, allocator);
            XXSumProd = new NativeArray<float>(1, allocator);
            YYSumProd = new NativeArray<float>(YCount, allocator);
            XYSumProd = new NativeArray<float>(YCount, allocator);
            R = new NativeArray<float>(YCount, allocator);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            XSum.Dispose();
            YSum.Dispose();
            XXSumProd.Dispose();
            YYSumProd.Dispose();
            XYSumProd.Dispose();
            R.Dispose();
        }
    }
}