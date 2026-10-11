using Xunit;

// Las features comparten una única base SQLite en archivo; correrlas en paralelo
// produciría carreras de datos entre escenarios. Los escenarios deben ser seriales.
[assembly: CollectionBehavior(DisableTestParallelization = true)]