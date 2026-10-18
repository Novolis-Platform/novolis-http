# Release

Packages are versioned with the Novolis platform `2026.1.*` line and published to GitHub Packages by the repository merge workflow.

Before release:

1. Run the repository tests.
2. Run the NuGet-only and layer-boundary checks from `d:\novolis\novolis-governance`.
3. Verify that package README files and XML documentation are current.
4. Merge to `main` so CI can publish the packages.
