using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace Imagibee.Parallel {

    [BurstCompile]
    public struct ProductJob : IDisposable {
        private const int DEFAULT_WIDTH = 3000;
        [ReadOnly]
        public NativeArray<float> Src1;
        [ReadOnly]
        public NativeArray<float> Src2;
        [WriteOnly]
        public NativeArray<float> Dst;

        [BurstCompile]
        public struct BatchProductJob : IJobParallelForBatch {
            [ReadOnly]
            public NativeArray<float> Src1;
            [ReadOnly]
            public NativeArray<float> Src2;
            [WriteOnly]
            public NativeArray<float> Dst;
            public void Execute(int startIndex, int count)
            {
                for (var i = startIndex; i < startIndex + count; ++i) {
                    Dst[i] = Src1[i] * Src2[i];
                }
            }
        }

        public JobHandle Schedule(int length, int width = 0, JobHandle deps = new JobHandle())
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
            if ((length > Src1.Length) ||
                (length > Src2.Length) ||
                (length > Dst.Length)) {
                throw new ArgumentException();
            }
            var batchProductJob = new BatchProductJob
            {
                Src1 = Src1,
                Src2 = Src2,
                Dst = Dst
            };
            return batchProductJob.ScheduleBatch(length, width, deps);
        }

        public void Dispose()
        {
            Src1.Dispose();
            Src2.Dispose();
            Dst.Dispose();
        }
    }
}