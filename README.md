# com.imagibee.parallel
A Unity package that implements a parallelized PCC algorithm.  The goal of this algorithm is to enable performance improvements of applications that need to compute many correlations.  The package includes:

* __PccJob__ - computes the [Pearson correlation coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) of two arrays

## Performance
A 134 x improvement was achieved for the _Pcc tiny_ test case.  See _Performance_ tests for details. Performance measurements made with Burst safety checks, leak detection, and debugger all turned off.

| _Performance test_  | _Length_  | _YCount_  | _Time [us]_  |
|:----------|:----------|:----------|:----------|
| SerialPccTiny (baseline) | 1,000    | 1,000    | 22,130    |
| SerialPccLarge (baseline) | 1,000,000    | 1    | 36,660    |
| ParallelPccTiny | 1,000    | 1,000    | 165   |
| ParallelPccLarge | 1,000,000    | 1    | 1,880    |

The hardware used to measure performance was a Macbook Pro:
- 8-Core Intel Core i9
- Processor Speed:	2.3 GHz
- Number of Processors:	1
- Total Number of Cores:	8
- L2 Cache (per Core):	256 KB
- L3 Cache:	16 MB
- Hyper-Threading Technology:	Enabled
- Memory:	16 GB

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
var pccJob = new PccJobv6();

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
* [Unity 2020.3](https://unity3d.com/unity/whats-new/2020.3.0)
* [Entities 0.50.1-preview.2](https://docs.unity3d.com/Packages/com.unity.entities@0.50/manual/index.html)

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
