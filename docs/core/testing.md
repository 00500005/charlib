These are our contributor testing guidelines

Currently we use the built-in dotnet test running (from dotnet 6.0 and later)

# How to Run Tests

```dotnet test```

The command `dotnet test` in the project root folder will run all project tests.

If it fails, you'll likely need to install or update the dotnet SDK.

To run one or more specific tests, use `dotnet test --filter`. The documentation for it can be found [here](https://docs.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest)

## Installing dotnet

See the microsoft docs for instructions on how to install on your OS [here](https://dotnet.microsoft.com/en-us/download)

You may also want to check that you have all required versions (both SDK and runtime). We target net461 (4.6.1) and net60 (6.0). You can check by following the microsoft instructions [here](https://docs.microsoft.com/en-us/dotnet/core/install/how-to-detect-installed-versions)

# The Why and How of Testing in General

I've noticed that in the vintage story community testing is not commonly done. Part of the reason is that it is not straight forward to add tests to a mod.

This is because we target `net461` as part of vintage story. However, more modern test runners (that are also capable of directly running on linux) use `net60` and later.

We technically *could* build and run tests using `mono` with `mcs` and some test runner, but this is much more onerous. Even I haven't figured out a clean way of doing so (ie, without creating some potentially very brittle scripts)

Because of this, we build multiple targets for our mods: `net461`, `net60`. `net461` is put into the actual mod zip, but `net60` is used by our testing framework (`CharlibTests`).

> **Dev Note:** I haven't actually checked that vintage story can't run a `net60` binary, but I'm not optimistic

> **Dev Note:** Although doing this does make it easier to setup testing, the fact that we're testing against a different target framework means there might be bugs that get missed by our test (though it's not likely)

> **Dev Note:** Because we build with `dotnet` directly, there may be subtle differences from a `mono` built binary (using `mcs`). Ideally we'd build with `mono`, as that is our target run time. In theory it's compatible, but in practice I've noticed that the debugger has some critical issues (such as failing to find break points, and hanging if the code calls `Debugger.Break`)

> **Dev TODO:** We'll continue to look into building with `mono` to see if we can resolve the debugger issue.

# Guidelines for Writing Tests

> **TODO** For now, just ask 00500005 in chat for help and to review