using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using UnityEngine;

namespace Imagibee.Parallel {
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct PccJob : IDisposable {
        public int Length;
        public int YCount;
        public NativeArray<float> X;
        public NativeArray<float> Y;
        public NativeReference<float> XSum;
        public NativeReference<float> XResult; // sqrt(Length*XXSumProd - XSum*XSum) 
        public NativeArray<float> YSum;
        public NativeArray<float> YYSumProd;
        public NativeArray<float> XYSumProd;
        public NativeArray<float> R;

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        struct PccPartitionJobX : IJobParallelForBatch {
            public int YCount;
            public int Length;
            [ReadOnly]
            public NativeArray<float> X;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeReference<float> XSum;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeReference<float> XResult; // sqrt(Length*XXSumProd - XSum*XSum) 

            public void Execute(int startIndex, int count)
            {
                var sum = 0f;
                var sumProd = 0f;
                for (var i = startIndex; i < startIndex + count; ++i) {
                    sum += X[i];
                    sumProd += X[i] * X[i];
                }
                XSum.Value = sum;
                XResult.Value = math.sqrt(Length * sumProd - sum * sum);
            }
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
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

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        struct PccMergeJob : IJobParallelForBatch {
            public int Length;
            [ReadOnly]
            public NativeReference<float> XSum;
            [ReadOnly]
            public NativeReference<float> XResult; // sqrt(Length*XXSumProd - XSum*XSum) 
            [ReadOnly]
            public NativeArray<float> YSum;
            [ReadOnly]
            public NativeArray<float> YYSumProd;
            [ReadOnly]
            public NativeArray<float> XYSumProd;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> R;

            public void Execute(int startIndex, int count)
            {
                for (int i = startIndex; i < startIndex + count; ++i) {
                    //Debug.Log($"i={i}, XSum={XResults[0]}, XResult={XResults[1]}, YSum={YSum[i]}, YYSUmProd={YYSumProd[0]}, XYSumProd={XYSumProd[i]}");
                    R[i] =
                        (Length * XYSumProd[i] - XSum.Value * YSum[i]) /
                        XResult.Value /
                        (math.sqrt(Length * YYSumProd[i] - YSum[i] * YSum[i]));
                }
            }
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public JobHandle Schedule(JobHandle deps = new JobHandle())
        {
            var jobs = new NativeList<JobHandle>(2, Allocator.TempJob);
            var xPartitionJob = new PccPartitionJobX
            {
                Length = Length,
                YCount = 1,
                X = X,
                XSum = XSum,
                XResult = XResult
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
                XResult = XResult,
                YSum = YSum,
                YYSumProd = YYSumProd,
                XYSumProd = XYSumProd,
                R = R
            };
            var newDeps = mergeJob.ScheduleBatch(YCount, 4, JobHandle.CombineDependencies(jobs));
            jobs.Dispose();
            return newDeps;
        }

        public void Allocate(int length, int ycount, Allocator allocator = Allocator.Persistent)
        {
            Length = length;
            YCount = ycount;
            X = new NativeArray<float>(Length, allocator);
            Y = new NativeArray<float>(Length * YCount, allocator);
            XSum = new NativeReference<float>(allocator);
            XResult = new NativeReference<float>(allocator);
            YSum = new NativeArray<float>(YCount, allocator);
            YYSumProd = new NativeArray<float>(YCount, allocator);
            XYSumProd = new NativeArray<float>(YCount, allocator);
            R = new NativeArray<float>(YCount, allocator);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            XSum.Dispose();
            XResult.Dispose();
            YSum.Dispose();
            YYSumProd.Dispose();
            XYSumProd.Dispose();
            R.Dispose();
        }
    }
}
