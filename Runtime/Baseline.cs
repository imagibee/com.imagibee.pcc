using UnityEngine;

namespace Imagibee.Parallel {
    // Only intended to form a baseline for testing
    public class Baseline {
        // Returns the Pearson correlation coefficient of two arrays
        public static float Pcc(float[] x, float[] y)
        {
            var sumX = Sum(x);
            var sumY = Sum(y);
            var n = x.Length;
            return
                (n * SumProd(x, y) - sumX * sumY) /
                Mathf.Sqrt(n * SumProd(x, x) - sumX * sumX) /
                Mathf.Sqrt(n * SumProd(y, y) - sumY * sumY);
        }

        // Returns the sum of the array
        public static float Sum(float[] x)
        {
            var sum = 0f;
            for (var i = 0; i < x.Length; ++i) {
                sum += x[i];
            }
            return sum;
        }

        // Returns the sum of the product of the arrays
        public static float SumProd(float[] x, float[] y)
        {
            var sum = 0f;
            for (var i = 0; i < x.Length; ++i) {
                sum += x[i] * y[i];
            }
            return sum;
        }
    }
}