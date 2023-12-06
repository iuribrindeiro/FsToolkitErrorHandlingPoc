# FsToolkitErrorHandlingPoc
Trying out once again the FsToolkit.ErrorHandling lib


A few months ago when I first started playing with F# it was hard to play with the library.

I was still learning some basic concepts and functions like `apply`, `map`, `bind` etc.

It was hard for me to figure out how to shape the `Result` type as to what I wanted without writing it on my own.
Especially, it was hard to use the [FsToolkit.ErrorHandling](https://demystifyfp.gitbook.io/fstoolkit-errorhandling/) utility functions the way I wanted.

Now that I have a bit more experience with the language I can finally use this library to achieve the result I want.

You can look at the `Program.fs` and see that I was able to combine multiple errors from different properties when trying to build a record 
and even aggregate them into a single error for an specific property of another record (`Order -> Product list`).

This library feels amazing now that I can use the `Result` monad with a bit more confidence and knowledge.
