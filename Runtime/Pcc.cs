using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using UnityEngine;

namespace Imagibee.Parallel
{
    [BurstCompile]
    public struct PccJobv1 : IDisposable
    {
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

    [BurstCompile]
    public struct PccJobv2 : IDisposable {
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
        struct PccPartitionJob : IJobParallelForBatch {
            public int YCount;
            public int Length;
            [ReadOnly]
            public NativeArray<float> Partition;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> Sum;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> SumProd;

            public void Execute(int startIndex, int count)
            {
                var sum = 0f;
                var sumProd = 0f;
                var sumIndex = startIndex / Length;
                for (var i=startIndex; i< startIndex+count; ++i) {
                    sum += Partition[i];
                    sumProd += Partition[i] * Partition[i];
                }
                Sum[sumIndex] = sum;
                SumProd[sumIndex] = sumProd;
            }
        }

        [BurstCompile]
        struct PccSeamJob : IJobParallelForBatch {
            public int Length;
            public int YCount;
            [ReadOnly]
            public NativeArray<float> Partition1;
            [ReadOnly]
            public NativeArray<float> Partition2;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> SumProd;

            public void Execute(int startIndex, int count)
            {
                var sumProd = 0f;
                var sumIndex = startIndex / Length;
                for (var i = startIndex; i < startIndex+count; ++i) {
                    sumProd += Partition1[i-startIndex] * Partition2[i];
                }
                SumProd[sumIndex] = sumProd;
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
            var jobs = new NativeList<JobHandle>(3, Allocator.TempJob);
            var xPartitionJob = new PccPartitionJob
            {
                Length = Length,
                YCount = 1,
                Partition = X,
                Sum = XSum,
                SumProd = XXSumProd
            };
            jobs.Add(xPartitionJob.ScheduleBatch(Length, Length, deps));

            var yPartitionJob = new PccPartitionJob
            {
                Length = Length,
                YCount = YCount,
                Partition = Y,
                Sum = YSum,
                SumProd = YYSumProd
            };
            jobs.Add(yPartitionJob.ScheduleBatch(Length * YCount, Length, deps));

            var ySeamJob = new PccSeamJob
            {
                Length = Length,
                YCount = YCount,
                Partition1 = X,
                Partition2 = Y,
                SumProd = XYSumProd
            };
            jobs.Add(ySeamJob.ScheduleBatch(Length * YCount, Length, deps));

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

    [BurstCompile]
    public struct PccJobv4 : IDisposable {
        public int Length;
        public int YCount;
        public NativeArray<float> X;
        public NativeArray<float> Y;
        public NativeArray<float> XResults;
        public NativeArray<float> YSum;
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
            public NativeArray<float> XResults; // 0: XSum, 1: sqrt(Length*XXSumProd - XSum*XSum) 

            public void Execute(int startIndex, int count)
            {
                var sum = 0f;
                var sumProd = 0f;
                for (var i = startIndex; i < startIndex + count; ++i) {
                    sum += X[i];
                    sumProd += X[i] * X[i];
                }
                XResults[0] = sum;
                XResults[1] = math.sqrt(Length * sumProd - sum * sum);
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
            public NativeArray<float> XResults;
            [ReadOnly]
            public NativeArray<float> YSum;
            [ReadOnly]
            public NativeArray<float> YYSumProd;
            [ReadOnly]
            public NativeArray<float> XYSumProd;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> R;

            public void Execute(int i)
            {
                //Debug.Log($"i={i}, XSum={XResults[0]}, XResult={XResults[1]}, YSum={YSum[i]}, YYSUmProd={YYSumProd[0]}, XYSumProd={XYSumProd[i]}");
                R[i] =
                    (Length * XYSumProd[i] - XResults[0] * YSum[i]) /
                    XResults[1] /
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
                XResults = XResults
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
                XResults = XResults,
                YSum = YSum,
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
            XResults = new NativeArray<float>(2, allocator);
            YSum = new NativeArray<float>(YCount, allocator);
            YYSumProd = new NativeArray<float>(YCount, allocator);
            XYSumProd = new NativeArray<float>(YCount, allocator);
            R = new NativeArray<float>(YCount, allocator);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            XResults.Dispose();
            YSum.Dispose();
            YYSumProd.Dispose();
            XYSumProd.Dispose();
            R.Dispose();
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct PccJobv6 : IDisposable {
        public int Length;
        public int YCount;
        public NativeArray<float> X;
        public NativeArray<float> Y;
        public NativeArray<float> XResults;
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
            public NativeArray<float> XResults; // 0: XSum, 1: sqrt(Length*XXSumProd - XSum*XSum) 

            public void Execute(int startIndex, int count)
            {
                var sum = 0f;
                var sumProd = 0f;
                for (var i = startIndex; i < startIndex + count; ++i) {
                    sum += X[i];
                    sumProd += X[i] * X[i];
                }
                XResults[0] = sum;
                XResults[1] = math.sqrt(Length * sumProd - sum * sum);
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
        struct PccMergeJob : IJobParallelFor {
            public int Length;
            [ReadOnly]
            public NativeArray<float> XResults;
            [ReadOnly]
            public NativeArray<float> YSum;
            [ReadOnly]
            public NativeArray<float> YYSumProd;
            [ReadOnly]
            public NativeArray<float> XYSumProd;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<float> R;

            public void Execute(int i)
            {
                //Debug.Log($"i={i}, XSum={XResults[0]}, XResult={XResults[1]}, YSum={YSum[i]}, YYSUmProd={YYSumProd[0]}, XYSumProd={XYSumProd[i]}");
                R[i] =
                    (Length * XYSumProd[i] - XResults[0] * YSum[i]) /
                    XResults[1] /
                    (math.sqrt(Length * YYSumProd[i] - YSum[i] * YSum[i]));
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
                XResults = XResults
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
                XResults = XResults,
                YSum = YSum,
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
            XResults = new NativeArray<float>(2, allocator);
            YSum = new NativeArray<float>(YCount, allocator);
            YYSumProd = new NativeArray<float>(YCount, allocator);
            XYSumProd = new NativeArray<float>(YCount, allocator);
            R = new NativeArray<float>(YCount, allocator);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            XResults.Dispose();
            YSum.Dispose();
            YYSumProd.Dispose();
            XYSumProd.Dispose();
            R.Dispose();
        }
    }
}
