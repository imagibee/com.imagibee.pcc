# com.imagibee.parallel
A Unity package that implements a parallelized [Pearson Correlation Coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) (PCC) algorithm.  The problem being solved by this package is performance improvements for applications that need to compute many correlations.  The package includes:

* `PccJob` - computes the PCC of a given array vs 1 or more reference arrays

## Performance Summary
Before going to the trouble of incorporating this package into your application it is nice to have some idea how much improvement is expected.  This section is intended to provide just that. The performance improvement for Mac and iOS configurations is summarized in the table below.  The optimized Mac version was compared to both Mono and IL2CPP baselines.  See _Performance_ tests for details. Performance measurements were made with the Burst Compiler's safety checks, leak detection, and debugger all turned off.

| Baseline Configuration | Peak Improvement | Typical Improvement
|:-----------------------|:-----------------|:-------------------
| Mac Mono               | x140             | x90
| Mac IL2CPP             | x30              | x20
| iOS                    | x9               | x6

For array lengths between 100 and 100,000 the performance improvement for the Mac Mono configuration stayed above x90 (ie. the optimized code ran ninety times faster than this baseline).  Performance peaked around x140 with an array length of 10,000.  Of the configurations tested the Mac Mono baseline benefitted the most which is not suprising since Mono produces code that runs slower than IL2CPP.

For array lengths between 1,000 and 100,000 the performance improvement of the Mac IL2CPP baseline stayed above x20.  Performance peaked around x30 with an array length of 10,000.  

For array lengths between 100 and 100,000 the performance improvement of the iOS baseline stayed above x6.  Performance peaked around x9 with an array length of 10,000.  The iOS baseline benefitted the least from this type of optimization.  In fact it was the fastest baseline.  This result suprised me.  I did not expect the iOS baseline to be faster than the Mac IL2CPP baseline. I can only speculate as to the reason.  Perhaps it is due to the massive size of L2 cache on the iOS device compared to the Mac (see [Hardware Used](#hardware-used)).

## Usage
Here is a contrived example that is meant to illustrate how `PccJob` may be incorporated by a Unity MonoBehaviour.
```cs
using Imagibee.Parallel;
...

class MyCorrelator : MonoBehaviour
{
    DataSource liveData;
    DataLibrary referenceData;
    PccJob pccJob;

    void Start()
    {
        // The source of the live data we want to correlate
        liveData = GameObject.FindWithTag("LiveData")
            .GetComponent<DataSource>();

        // The library of reference data we want to correlate against
        referenceData = GameObject.FindWithTag("ReferenceData")
            .GetComponent<DataLibrary>();

        // Allocate native storage for the lifetime of the MonoBehaviour
        pccJob.Allocate(
            referenceData.Values[0].Length,
            referenceData.Values.Count);

        // Flatten reference data into a single array
        var y = new float[pccJob.Length * pccJob.YCount];
        for (var i=0; i<pccJob.YCount; ++i) {
            Array.Copy(
                referenceData.Values[i],
                0,
                y,
                i * pccJob.Length,
                pccJob.Length); 
        }

        // Copy flattened reference data into native storage
        pccJob.Y.CopyFrom(y);
    }

    void Update()
    {
        // Temporary storage for sorting the results where
        // Item1: i, Item2: R[i]
        var corData = new List<Tuple<int, float>>();

        // Copy the latest data from our data source into X
        pccJob.CopyToX(liveData.Value, 0);

        // Compute the correlation of X vs each Y
        pccJob.Schedule.Complete();

        // Store correlations and index so we can sort them
        for (var i = 0; i < pccJob.YCount; ++i) {
            corData.Add(new Tuple<int, float>(i, pccJob.R[i]));
        }

        // Sort by descending correlation
        corData.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        // Do something with the correlation data
        ProcessCorData(corData);
    }

    void ProcessCorData(List<Tuple<int, float>>  corData)
    {
        Debug.Log($"max correlation at {corData[0].Item1} is {corData[0].Item2}");
    }

    void OnDestroy()
    {
        // Dispose of native storage
        pccJob.Dispose();
    }
}
```

## Burst Settings
By default `PccJob` uses `Unity.Burst.FloatMode.Fast` and `Unity.Burst.FloatPrecision.Low`.  The following pre-processor symbols [may be defined by a project](https://docs.unity3d.com/Manual/CustomScriptingSymbols.html) to change the `FloatMode` or `FloatPrecision` used by the PccJob.

* IMAGIBEE_PARALLEL_FLOATMODE_DEFAULT
* IMAGIBEE_PARALLEL_FLOATMODE_DETERMINISTIC
* IMAGIBEE_PARALLEL_FLOATMODE_STRICT
* IMAGIBEE_PARALLEL_FLOATPRECISION_STANDARD
* IMAGIBEE_PARALLEL_FLOATPRECISION_HIGH
* IMAGIBEE_PARALLEL_FLOATPRECISION_MEDIUM

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

## Issues
Report and track issues [here](https://github.com/imagibee/com.imagibee.parallel/issues).

## Contributing
Minor changes such as bug fixes are welcome.  Simply make a [pull request](https://opensource.com/article/19/7/create-pull-request-github).  Please discuss more significant changes prior to making the pull request by opening a new issue that describes the change.

## Testing
The package includes _Functional_ and _Performance_ tests.  To run the tests open the [Test Runner](https://docs.unity3d.com/2020.3/Documentation/Manual/testing-editortestsrunner.html).  Select the `Imagibee.Parallel.Tests.dll` and _Run Selected_.  If the tests do not show up in the _Test Runner_ you might need to add the following entry to your [manifest file](https://docs.unity3d.com/2020.3/Documentation/Manual/upm-manifestPrj.html).

```json
"testables": [
    "com.imagibee.parallel"
  ]
```

### Performance Tests
Here are the detailed results of the __Performance Tests__.
#### Mac Baseline (Mono)
```shell
Baseline Pcc (length=10, ycount=100000) Millisecond Median:31.29 Min:31.18 Max:31.59 Avg:31.30 Std:0.11 SampleCount: 9 Sum: 281.73
Baseline Pcc (length=100, ycount=10000) Millisecond Median:23.03 Min:22.94 Max:23.26 Avg:23.07 Std:0.10 SampleCount: 9 Sum: 207.64
Baseline Pcc (length=1000, ycount=1000) Millisecond Median:22.13 Min:22.11 Max:22.23 Avg:22.14 Std:0.04 SampleCount: 9 Sum: 199.23
Baseline Pcc (length=10000, ycount=100) Millisecond Median:22.17 Min:22.15 Max:22.21 Avg:22.17 Std:0.02 SampleCount: 9 Sum: 199.56
Baseline Pcc (length=20000, ycount=50) Millisecond Median:22.34 Min:22.31 Max:22.43 Avg:22.34 Std:0.03 SampleCount: 9 Sum: 201.08
Baseline Pcc (length=100000, ycount=10) Millisecond Median:23.48 Min:23.46 Max:23.52 Avg:23.49 Std:0.02 SampleCount: 9 Sum: 211.40
Baseline Pcc (length=1000000, ycount=1) Millisecond Median:36.72 Min:36.68 Max:36.81 Avg:36.73 Std:0.05 SampleCount: 9 Sum: 330.59
```
#### Mac Baseline (IL2CPP)
```shell
Baseline Pcc (length=10, ycount=100000) Millisecond Median:3.06 Min:3.02 Max:3.15 Avg:3.07 Std:0.04 SampleCount: 9 Sum: 27.67
Baseline Pcc (length=100, ycount=10000) Millisecond Median:3.33 Min:3.32 Max:3.46 Avg:3.35 Std:0.05 SampleCount: 9 Sum: 30.18
Baseline Pcc (length=1000, ycount=1000) Millisecond Median:4.55 Min:4.53 Max:4.56 Avg:4.55 Std:0.01 SampleCount: 9 Sum: 40.92
Baseline Pcc (length=10000, ycount=100) Millisecond Median:4.66 Min:4.66 Max:4.67 Avg:4.66 Std:0.01 SampleCount: 9 Sum: 41.97
Baseline Pcc (length=20000, ycount=50) Millisecond Median:4.70 Min:4.69 Max:4.71 Avg:4.70 Std:0.01 SampleCount: 9 Sum: 42.28
Baseline Pcc (length=100000, ycount=10) Millisecond Median:4.95 Min:4.94 Max:5.72 Avg:5.03 Std:0.24 SampleCount: 9 Sum: 45.29
Baseline Pcc (length=1000000, ycount=1) Millisecond Median:7.74 Min:7.73 Max:7.76 Avg:7.74 Std:0.01 SampleCount: 9 Sum: 69.64
```
#### iOS Baseline
```shell
Baseline Pcc (length=10, ycount=100000) Millisecond Median:2.58 Min:2.58 Max:2.61 Avg:2.59 Std:0.01 SampleCount: 9 Sum: 23.31
Baseline Pcc (length=100, ycount=10000) Millisecond Median:2.40 Min:2.39 Max:2.42 Avg:2.40 Std:0.01 SampleCount: 9 Sum: 21.60
Baseline Pcc (length=1000, ycount=1000) Millisecond Median:2.96 Min:2.96 Max:2.97 Avg:2.97 Std:0.00 SampleCount: 9 Sum: 26.69
Baseline Pcc (length=10000, ycount=100) Millisecond Median:3.03 Min:3.03 Max:3.05 Avg:3.03 Std:0.01 SampleCount: 9 Sum: 27.29
Baseline Pcc (length=20000, ycount=50) Millisecond Median:3.06 Min:3.06 Max:3.07 Avg:3.06 Std:0.00 SampleCount: 9 Sum: 27.57
Baseline Pcc (length=100000, ycount=10) Millisecond Median:3.22 Min:3.21 Max:3.23 Avg:3.22 Std:0.00 SampleCount: 9 Sum: 28.98
Baseline Pcc (length=1000000, ycount=1) Millisecond Median:5.05 Min:5.03 Max:5.06 Avg:5.05 Std:0.01 SampleCount: 9 Sum: 45.41
```
#### Mac Optimized
```shell
Optimized PCC (length=10, ycount=100000) Millisecond Median:0.88 Min:0.83 Max:0.93 Avg:0.88 Std:0.03 SampleCount: 9 Sum: 7.94
Optimized PCC (length=100, ycount=10000) Millisecond Median:0.24 Min:0.24 Max:0.26 Avg:0.25 Std:0.01 SampleCount: 9 Sum: 2.23
Optimized PCC (length=1000, ycount=1000) Millisecond Median:0.17 Min:0.17 Max:0.18 Avg:0.17 Std:0.00 SampleCount: 9 Sum: 1.52
Optimized PCC (length=10000, ycount=100) Millisecond Median:0.16 Min:0.16 Max:0.17 Avg:0.16 Std:0.00 SampleCount: 9 Sum: 1.45
Optimized PCC (length=20000, ycount=50) Millisecond Median:0.17 Min:0.17 Max:0.18 Avg:0.17 Std:0.00 SampleCount: 9 Sum: 1.56
Optimized PCC (length=100000, ycount=10) Millisecond Median:0.25 Min:0.24 Max:0.26 Avg:0.25 Std:0.00 SampleCount: 9 Sum: 2.23
Optimized PCC (length=1000000, ycount=1) Millisecond Median:1.95 Min:1.91 Max:1.97 Avg:1.94 Std:0.02 SampleCount: 9 Sum: 17.49
```
#### iOS Optimized
```shell
Optimized PCC (length=10, ycount=100000) Millisecond Median:0.93 Min:0.91 Max:0.95 Avg:0.93 Std:0.01 SampleCount: 9 Sum: 8.39
Optimized PCC (length=100, ycount=10000) Millisecond Median:0.39 Min:0.38 Max:0.40 Avg:0.39 Std:0.00 SampleCount: 9 Sum: 3.51
Optimized PCC (length=1000, ycount=1000) Millisecond Median:0.37 Min:0.36 Max:0.39 Avg:0.38 Std:0.01 SampleCount: 9 Sum: 3.39
Optimized PCC (length=10000, ycount=100) Millisecond Median:0.40 Min:0.36 Max:0.41 Avg:0.40 Std:0.02 SampleCount: 9 Sum: 3.56
Optimized PCC (length=20000, ycount=50) Millisecond Median:0.39 Min:0.38 Max:0.41 Avg:0.39 Std:0.01 SampleCount: 9 Sum: 3.50
Optimized PCC (length=100000, ycount=10) Millisecond Median:0.47 Min:0.45 Max:0.48 Avg:0.47 Std:0.01 SampleCount: 9 Sum: 4.20
Optimized PCC (length=1000000, ycount=1) Millisecond Median:1.29 Min:1.29 Max:1.30 Avg:1.29 Std:0.00 SampleCount: 9 Sum: 11.63
```

#### Hardware Used
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
