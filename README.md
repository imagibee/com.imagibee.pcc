# com.imagibee.parallel
A Unity package supporting a variety of parallel computations.  The package includes:

* __SumJob__ - computes the sum of the elements of an array
* __ProductJob__ - computes the element-wise product of two arrays
* __PccJob__ - computes the [Pearson correlation coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) of two arrays

## Usage
Here is a brief example that illustrates the usage.  Refer to `Runtime/Pcc.cs` to see how `SumJob` and `ProductJob` may be combined to form more complex computations.
```cs
using Imagibee.Parallel

var x = new float[] { 1, 2, 3, 4, 5 };
var y = new float[] { -1, -2, -3, -4, -5 };
var pccJob = new PccJob()
{
    Allocator = Allocator.Persistent,
    Length = x.Length,
    Width = x.Length / 2
};
pccJob.Allocate();
pccJob.X.CopyFrom(x);
pccJob.Y.CopyFrom(x);
pccJob.Schedule().Complete();
var pccXX = pccJob.Result.Value;
pccJob.Y.CopyFrom(y);
pccJob.Schedule().Complete();
var pccYY = pccJob.Result.Value;
pccJob.Dispose();
Assert.AreEqual(1f, pccXX);
Assert.AreEqual(-1f, pccYY);
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
Minor changes such as bug fixes and performance improvements are welcome any time.  Simply make a [pull request](https://opensource.com/article/19/7/create-pull-request-github).  Please discuss more significant changes prior to making the pull request by opening a new issue that describes the change.
