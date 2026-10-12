## ⚠️ Mandatory PR Policy
> **Pull Requests are ONLY reviewed and accepted if they are linked to an existing, pre-approved open issue.**
> **Opening a Pull Request without an associated open issue will result in the PR being automatically closed without review.**

**Associated Issue**: Closes #___

---

## Summary of Changes
A concise summary of the changes made in this Pull Request.

---

## Contributor Checklist
- [ ] This PR directly links to an existing open issue above (e.g. `Closes #123` or `Fixes #123`).
- [ ] Code adheres to the repository's ground rules (`AGENTS.md`):
  - [ ] Explicit typing over `var` (no lazy `var`).
  - [ ] Unity null checks use `== null` / `!= null` (no `?.` or `is null` on `UnityEngine.Object`).
  - [ ] No `/// <summary>` blocks on internal methods.
  - [ ] Evaluated across Solo, Player-Hosted, and Dedicated Server modes.
- [ ] Does **NOT** modify version numbers in `manifest.json`, `AssemblyInfo.cs`, or `.csproj`.
- [ ] Does **NOT** modify `CHANGELOG.md` or release archives (releases are handled by maintainers).
