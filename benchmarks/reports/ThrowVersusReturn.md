```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7623/24H2/2024Update/HudsonValley)
AMD Ryzen 7 6800H with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.102
  [Host]         : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
  .NET 10.0      : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
  NativeAOT 10.0 : .NET 10.0.2, X64 NativeAOT x86-64-v3


```
| Method                     | Job            | Runtime        | Mode      | Mean          | Error     | StdDev    | Gen0   | Allocated |
|--------------------------- |--------------- |--------------- |---------- |--------------:|----------:|----------:|-------:|----------:|
| **&#39;int or throw&#39;**             | **.NET 10.0**      | **.NET 10.0**      | **HappyPath** |     **0.6039 ns** | **0.0021 ns** | **0.0016 ns** |      **-** |         **-** |
| &#39;Expected&lt;int, Exception&gt;&#39; | .NET 10.0      | .NET 10.0      | HappyPath |     0.3906 ns | 0.0220 ns | 0.0172 ns |      - |         - |
| &#39;Expected&lt;int, int?&gt;&#39;      | .NET 10.0      | .NET 10.0      | HappyPath |     0.8271 ns | 0.0019 ns | 0.0017 ns |      - |         - |
| &#39;int or throw&#39;             | NativeAOT 10.0 | NativeAOT 10.0 | HappyPath |     2.0684 ns | 0.0429 ns | 0.0358 ns |      - |         - |
| &#39;Expected&lt;int, Exception&gt;&#39; | NativeAOT 10.0 | NativeAOT 10.0 | HappyPath |     2.2089 ns | 0.0335 ns | 0.0261 ns |      - |         - |
| &#39;Expected&lt;int, int?&gt;&#39;      | NativeAOT 10.0 | NativeAOT 10.0 | HappyPath |     1.5372 ns | 0.0383 ns | 0.0340 ns |      - |         - |
| **&#39;int or throw&#39;**             | **.NET 10.0**      | **.NET 10.0**      | **ErrorPath** | **1,168.8768 ns** | **5.2164 ns** | **4.8795 ns** | **0.0248** |     **216 B** |
| &#39;Expected&lt;int, Exception&gt;&#39; | .NET 10.0      | .NET 10.0      | ErrorPath |     4.4915 ns | 0.1075 ns | 0.0953 ns | 0.0143 |     120 B |
| &#39;Expected&lt;int, int?&gt;&#39;      | .NET 10.0      | .NET 10.0      | ErrorPath |     0.6155 ns | 0.0233 ns | 0.0194 ns |      - |         - |
| &#39;int or throw&#39;             | NativeAOT 10.0 | NativeAOT 10.0 | ErrorPath |   481.0296 ns | 7.6799 ns | 6.8080 ns | 0.0315 |     264 B |
| &#39;Expected&lt;int, Exception&gt;&#39; | NativeAOT 10.0 | NativeAOT 10.0 | ErrorPath |     6.2202 ns | 0.0637 ns | 0.0596 ns | 0.0105 |      88 B |
| &#39;Expected&lt;int, int?&gt;&#39;      | NativeAOT 10.0 | NativeAOT 10.0 | ErrorPath |     1.7837 ns | 0.0095 ns | 0.0089 ns |      - |         - |
