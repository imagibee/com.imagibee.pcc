# com.imagibee.parallel
A Unity package that implements a parallelized [Pearson Correlation Coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) (PCC) algorithm.  The problem being solved by this package is performance improvements for applications that need to compute many correlations.  The package includes:

* __PccJob__ - computes the PCC of a given array vs 1 or more reference arrays

## Performance
The performance improvement for three tested configurations is summarized in the table below.  See _Performance_ tests for details. Performance measurements were made with the Burst Compiler's safety checks, leak detection, and debugger all turned off.

| Configuration | Peak Improvement | Typical Improvement
|:--------------|:-----------------|:-------------------
| Mac Mono      | x140             | x90
| Mac IL2CPP    | x31              | x20
| iOS           | x8.4             | x6

The performance improvement for the Mac Mono configuration stayed above x90 (ie. ninety times faster than baseline) for array lengths between 100 and 100,000.  At a length of 10,000 a x140 improvement was measured.  Of the configurations tested Mac Mono benefitted most.  This result matches expectations since the Mono baseline is inherently slower than IL2CPP.

The performance improvement for the Mac IL2CPP configuration stayed above x20 for array lengths between 1000 and 100,000.  At a length of 10,000 a peak of x31 improvement was measured.  

The performance improvement for the iOS configuration stayed above x6 (ie. six times faster than baseline) for array lengths between 100 and 100,000.  At a length of 10,000 a x8.4 improvement was measured.  I was a little suprised to see the iOS baseline outperform the Mac IL2CPP baseline by about 30%.

### Mac Baseline (Mono)
```shell
Baseline Pcc (length=10, ycount=100000) Millisecond Median:31.29 Min:31.18 Max:31.59 Avg:31.30 Std:0.11 SampleCount: 9 Sum: 281.73
Baseline Pcc (length=100, ycount=10000) Millisecond Median:23.03 Min:22.94 Max:23.26 Avg:23.07 Std:0.10 SampleCount: 9 Sum: 207.64
Baseline Pcc (length=1000, ycount=1000) Millisecond Median:22.13 Min:22.11 Max:22.23 Avg:22.14 Std:0.04 SampleCount: 9 Sum: 199.23
Baseline Pcc (length=10000, ycount=100) Millisecond Median:22.17 Min:22.15 Max:22.21 Avg:22.17 Std:0.02 SampleCount: 9 Sum: 199.56
Baseline Pcc (length=20000, ycount=50) Millisecond Median:22.34 Min:22.31 Max:22.43 Avg:22.34 Std:0.03 SampleCount: 9 Sum: 201.08
Baseline Pcc (length=100000, ycount=10) Millisecond Median:23.48 Min:23.46 Max:23.52 Avg:23.49 Std:0.02 SampleCount: 9 Sum: 211.40
Baseline Pcc (length=1000000, ycount=1) Millisecond Median:36.72 Min:36.68 Max:36.81 Avg:36.73 Std:0.05 SampleCount: 9 Sum: 330.59
```
### Mac Baseline (IL2CPP)
```shell
Baseline Pcc (length=10, ycount=100000) Millisecond Median:3.06 Min:3.02 Max:3.15 Avg:3.07 Std:0.04 SampleCount: 9 Sum: 27.67
Baseline Pcc (length=100, ycount=10000) Millisecond Median:3.33 Min:3.32 Max:3.46 Avg:3.35 Std:0.05 SampleCount: 9 Sum: 30.18
Baseline Pcc (length=1000, ycount=1000) Millisecond Median:4.55 Min:4.53 Max:4.56 Avg:4.55 Std:0.01 SampleCount: 9 Sum: 40.92
Baseline Pcc (length=10000, ycount=100) Millisecond Median:4.66 Min:4.66 Max:4.67 Avg:4.66 Std:0.01 SampleCount: 9 Sum: 41.97
Baseline Pcc (length=20000, ycount=50) Millisecond Median:4.70 Min:4.69 Max:4.71 Avg:4.70 Std:0.01 SampleCount: 9 Sum: 42.28
Baseline Pcc (length=100000, ycount=10) Millisecond Median:4.95 Min:4.94 Max:5.72 Avg:5.03 Std:0.24 SampleCount: 9 Sum: 45.29
Baseline Pcc (length=1000000, ycount=1) Millisecond Median:7.74 Min:7.73 Max:7.76 Avg:7.74 Std:0.01 SampleCount: 9 Sum: 69.64
```
### iOS Baseline
```shell
Baseline Pcc (length=10, ycount=100000) Millisecond Median:2.58 Min:2.58 Max:2.61 Avg:2.59 Std:0.01 SampleCount: 9 Sum: 23.31
Baseline Pcc (length=100, ycount=10000) Millisecond Median:2.40 Min:2.39 Max:2.42 Avg:2.40 Std:0.01 SampleCount: 9 Sum: 21.60
Baseline Pcc (length=1000, ycount=1000) Millisecond Median:2.96 Min:2.96 Max:2.97 Avg:2.97 Std:0.00 SampleCount: 9 Sum: 26.69
Baseline Pcc (length=10000, ycount=100) Millisecond Median:3.03 Min:3.03 Max:3.05 Avg:3.03 Std:0.01 SampleCount: 9 Sum: 27.29
Baseline Pcc (length=20000, ycount=50) Millisecond Median:3.06 Min:3.06 Max:3.07 Avg:3.06 Std:0.00 SampleCount: 9 Sum: 27.57
Baseline Pcc (length=100000, ycount=10) Millisecond Median:3.22 Min:3.21 Max:3.23 Avg:3.22 Std:0.00 SampleCount: 9 Sum: 28.98
Baseline Pcc (length=1000000, ycount=1) Millisecond Median:5.05 Min:5.03 Max:5.06 Avg:5.05 Std:0.01 SampleCount: 9 Sum: 45.41
```
### Mac Optimized
```shell
Optimized PCC (length=10, ycount=100000) Millisecond Median:1.07 Min:1.05 Max:1.09 Avg:1.07 Std:0.01 SampleCount: 9 Sum: 9.64
Optimized PCC (length=100, ycount=10000) Millisecond Median:0.25 Min:0.25 Max:0.27 Avg:0.25 Std:0.00 SampleCount: 9 Sum: 2.27
Optimized PCC (length=1000, ycount=1000) Millisecond Median:0.18 Min:0.18 Max:0.18 Avg:0.18 Std:0.00 SampleCount: 9 Sum: 1.63
Optimized PCC (length=10000, ycount=100) Millisecond Median:0.16 Min:0.15 Max:0.16 Avg:0.16 Std:0.00 SampleCount: 9 Sum: 1.40
Optimized PCC (length=20000, ycount=50) Millisecond Median:0.17 Min:0.16 Max:0.18 Avg:0.17 Std:0.00 SampleCount: 9 Sum: 1.55
Optimized PCC (length=100000, ycount=10) Millisecond Median:0.26 Min:0.25 Max:0.26 Avg:0.26 Std:0.00 SampleCount: 9 Sum: 2.33
Optimized PCC (length=1000000, ycount=1) Millisecond Median:1.95 Min:1.94 Max:1.99 Avg:1.96 Std:0.02 SampleCount: 9 Sum: 17.60
```
### iOS Optimized
```shell
Optimized PCC (length=10, ycount=100000) Millisecond Median:0.93 Min:0.91 Max:0.95 Avg:0.93 Std:0.01 SampleCount: 9 Sum: 8.39
Optimized PCC (length=100, ycount=10000) Millisecond Median:0.39 Min:0.38 Max:0.40 Avg:0.39 Std:0.00 SampleCount: 9 Sum: 3.51
Optimized PCC (length=1000, ycount=1000) Millisecond Median:0.37 Min:0.36 Max:0.39 Avg:0.38 Std:0.01 SampleCount: 9 Sum: 3.39
Optimized PCC (length=10000, ycount=100) Millisecond Median:0.40 Min:0.36 Max:0.41 Avg:0.40 Std:0.02 SampleCount: 9 Sum: 3.56
Optimized PCC (length=20000, ycount=50) Millisecond Median:0.39 Min:0.38 Max:0.41 Avg:0.39 Std:0.01 SampleCount: 9 Sum: 3.50
Optimized PCC (length=100000, ycount=10) Millisecond Median:0.47 Min:0.45 Max:0.48 Avg:0.47 Std:0.01 SampleCount: 9 Sum: 4.20
Optimized PCC (length=1000000, ycount=1) Millisecond Median:1.29 Min:1.29 Max:1.30 Avg:1.29 Std:0.00 SampleCount: 9 Sum: 11.63
```

### Hardware used
The hardware used to measure performance for Mac was a Macbook Pro
- 8-Core Intel Core i9
- L2 Cache (per Core):	256 KB
- L3 Cache:	16 MB
- Memory:	16 GB

The hardware used to measure performance for iOS was iPhone 12 Pro
- 6-Core ARMv8
- L2 Cache (per Core): 4 or 8 MB
- L3 Cache: 16 MB
- Memory:    6 GB
## Usage
This example illustrates the usage of _PccJob_ with some annotation.
```cs
using Imagibee.Parallel;

// Here x represents a frequently changing value that is
// correlated against a set of infrequently changing y.
var x = new List<float[]>
{
    new float[] { 1, 2, 3, 4, 5 },
    new float[] { 2, 4, 6, 8, 10 }
};
// There can be 1 or more y, concatenated together, the
// length of each y must match the length of x.  Here we
// have two y, each of length 5, concatenated together.
var y = new float[] { 1, 2, 3, 4, 5, -1, -2, -3, -4, -5 };

// length is the length of each x and y
var length = x[0].Length;

// ycount is the number of arrays contained in y
var ycount = y.Length / length;

// Create a correlation job
var pccJob = new PccJob();

// Allocate the native containers for the job
pccJob.Allocate(length, ycount, Allocator.Persistent);

// Copy the infrequently changing values of y to native storage
pccJob.Y.CopyFrom(y);

// Compute the correlation of two values of x using the same set of y
for (var i = 0; i < x.Count; ++i) {
    // Copy the current value of x to native storage
    pccJob.X.CopyFrom(x[i]);

    // Schedule and wait for completion of two corelation values
    // in pccJob.R
    // 1) the correlation of x[i] with the first 5 elements of y
    // 2) the correlation of x[i] with the last 5 elements of y
    pccJob.Schedule().Complete();

    // Do something with results
    for (var j = 0; j < ycount; ++j) {
        Debug.Log($"loop {i}, result {j}: {pccJob.R[j]}");
    }
}

// Dispose of the native containers once we are through with the job
pccJob.Dispose();
```

The console output is
```shell
loop 0, result 0: 1
loop 0, result 1: -1
loop 1, result 0: 1
loop 1, result 1: -1
```

## License
[MIT](https://www.mit.edu/~amini/LICENSE.md)

## Dependencies
* Unity 2021.3
* com.unity.burst
* com.unity.collections
* com.unity.jobs
* com.unity.mathematics
* com.unity.test-framework
* com.unity.test-framework.performance

## Versioning
This package uses [semantic versioning](https://en.wikipedia.org/wiki/Software_versioning#Semantic_versioning).  Tags on the main branch indicate versions.  It is recomended to use a tagged version.  The latest version on the main branch should be considered _under development_ when it is not tagged.

## Installation
This package is intended to be used from an existing Unity project.  Using the Package Manager select _Add package from git URL..._ and provide the URL to the version you want in this git repository.

## Testing
The package includes _Functional_ and _Performance_ tests.  To run the tests open the [Test Runner](https://docs.unity3d.com/2020.3/Documentation/Manual/testing-editortestsrunner.html).  Select the `Imagibee.Parallel.Tests.dll` and _Run Selected_.  If the tests do not show up in the _Test Runner_ you might need to add the following entry to your [manifest file](https://docs.unity3d.com/2020.3/Documentation/Manual/upm-manifestPrj.html).

```json
"testables": [
    "com.imagibee.parallel"
  ]
```

## Issues
Report and track issues [here](https://github.com/imagibee/com.imagibee.parallel/issues).

## Contributing
Minor changes such as bug fixes are welcome.  Simply make a [pull request](https://opensource.com/article/19/7/create-pull-request-github).  Please discuss more significant changes prior to making the pull request by opening a new issue that describes the change.
