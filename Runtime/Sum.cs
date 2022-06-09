using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace Imagibee.Parallel {

    [BurstCompile]
    public struct SumJob
    {
        private const int DEFAULT_WIDTH = 3000;

        [ReadOnly]
        public NativeSlice<float> Src;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> Dst;
        [WriteOnly]
        public NativeReference<float> Result;

        [BurstCompile]
        struct BatchSumJob : IJobParallelForBatch
        {
            public int Width;

            [ReadOnly]
            public NativeSlice<float> Src;
            [NativeDisableParallelForRestriction]
            public NativeArray<float> Dst;

            public void Execute(int startIndex, int count)
            {
                var chunkIndex = startIndex / Width; 
                var sum = Src[startIndex];
                for (var i=startIndex+1; i<startIndex+count; ++i) {
                    sum += Src[i];
                }
                Dst[chunkIndex] = sum;
            }
        }

        [BurstCompile]
        struct MergeSumJob : IJob
        {
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<float> Dst;
            [WriteOnly]
            public NativeReference<float> Result;
            public int NumChunks;

            public void Execute() {
                var sum = 0f;
                for (var i=0; i<NumChunks; ++i) {
                    sum += Dst[i];
                }
                Result.Value = sum;
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
            var numChunks = (length + width - 1) / width;

            Dst = new NativeArray<float>(numChunks, Allocator.TempJob);
            var segmentSumJob = new BatchSumJob
            {
                Src = Src,
                Dst = Dst,
                Width = width
            };
            var segmentSumMergeJob = new MergeSumJob
            {
                Dst = Dst,
                Result = Result,
                NumChunks = numChunks
            };
            return segmentSumMergeJob.Schedule(
                segmentSumJob.ScheduleBatch(length, width, deps));
        }
    }
}