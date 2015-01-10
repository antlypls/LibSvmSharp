LibSvmSharp
===========

LibSVM# is a C# port of java version of [libsvm](http://www.csie.ntu.edu.tw/~cjlin/libsvm/), a library for several [SVM](http://en.wikipedia.org/wiki/Support_vector_machine) methods. This project also provides F# bindings.

For more information about SVM, see original libsvm [practical guide](http://www.csie.ntu.edu.tw/~cjlin/papers/guide/guide.pdf) and [implementation details](http://www.csie.ntu.edu.tw/~cjlin/papers/libsvm.pdf).

At this time the aim of the project to provide a .NET version of the core library only, without additional tools provided by libsvm project, like command-line tools (svm-train, svm-predict, etc) or svm-toy.

Current LibSvmSharp version is based on libsvm 3.20.


Solution Details
----------------

The Solution consists of 6 projects:

* *LibSvm* &ndash; main LibSvmSharp project. It's a port of original libsvm java source (with some modifications and refactorings to make it looks in C# style). The code kept as close as possible to original codebase, so it can be easily updated to reflect changes in the libsvm.
* *LibSvmDemo* &ndash; Sample project for demonstration of how LibSvm assenbly can be used.
* *LibSvmExtras* &ndash; contains some helpers, provides more convenient way to create and configure SVMs.
* *LibSvmExtrasDemo* &ndash; demo project for LibSvmExtras assembly.
* *LibSvmFs* &ndash; F# wrapper for LibSvm.
* *LibSvmFsDemo* &ndash; demo project for LibSvmFs.

## Differencies from libsvm
There is only one big difference, that can affect you. The libsvm implementation stores features as sparse vectors (most feature values are zero), so feature vectors represented as array of index-value pairs.
