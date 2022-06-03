using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;

namespace Imagibee.AudioId {

    [BurstCompile]
    public struct SumJob : IDisposable
    {
        private const int DEFAULT_WIDTH = 3000;

        [ReadOnly]
        public NativeArray<float> Src;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> Dst;

        [BurstCompile]
        struct BatchSumJob : IJobParallelForBatch
        {
            [ReadOnly]
            public NativeArray<float> Src;
            [NativeDisableParallelForRestriction]
            public NativeArray<float> Dst;

            public void Execute(int startIndex, int count)
            {
                var sum = Src[startIndex];
                for (var i=startIndex+1; i<startIndex+count; ++i) {
                    sum += Src[i];
                }
                Dst[startIndex] = sum;
            }
        }

        [BurstCompile]
        struct MergeSumJob : IJob
        {
            public NativeArray<float> Data;
            public int Length;
            public int Width;

            public void Execute() {
                var sum = Data[0];
                for (var i=Width; i<Length; i+=Width) {
                    sum += Data[i];
                }
                Data[0] = sum;
            }
        }

        public JobHandle Schedule(int length, int width=0, JobHandle deps = new JobHandle())
        {
            if (length == 0) {
                return deps;
            }
            if (width < 2) {
                width = math.max(length, DEFAULT_WIDTH);
            }
            if (width > length) {
                width = length;
            }
            if (length > Src.Length) {
                throw new ArgumentException();
            }

            var segmentSumJob = new BatchSumJob
            {
                Src = Src,
                Dst = Dst
            };
            var segmentSumMergeJob = new MergeSumJob
            {
                Data = Dst,
                Length = length,
                Width = width
            };
            return segmentSumMergeJob.Schedule(
                segmentSumJob.ScheduleBatch(length, width, deps));
        }

        public void Dispose()
        {
            Src.Dispose();
            Dst.Dispose();
        }
    }
}